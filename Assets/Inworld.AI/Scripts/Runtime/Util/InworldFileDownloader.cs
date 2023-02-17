/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Inworld.Util
{
    /// <summary>
    ///     This class is used to get data fetching progress of InworldCharacter.
    /// </summary>
    public class CharacterFetchingProgress
    {
        public UnityWebRequestAsyncOperation avatarProgress;
        public bool needDownloadAvatar;
        public bool needDownloadThumbnail;
        public UnityWebRequestAsyncOperation thumbnailProgress;

        public CharacterFetchingProgress(bool downloadAvatar = false, bool downloadThumb = false)
        {
            needDownloadAvatar = downloadAvatar;
            needDownloadThumbnail = downloadThumb;
        }
        public float Progress
        {
            get
            {
                float fThumbProgress = !needDownloadThumbnail ? 0.2f : thumbnailProgress?.progress * 0.2f ?? 0;
                float fAvatarProgress = !needDownloadAvatar ? 0.8f : avatarProgress?.progress * 0.8f ?? 0;
                return fThumbProgress + fAvatarProgress;
            }
        }
    }
    /// <summary>
    ///     The class for downloading related thumbnail/avatars of Inworld Character Data.
    /// </summary>
    public class InworldFileDownloader : MonoBehaviour
    {
        #region Private Members & Functions
        const string k_ResourcePath = "Assets/Inworld.AI/Resources";
        [SerializeField] protected bool m_DownloadThumbnail;
        [SerializeField] protected bool m_DownloadAvatar;
        protected readonly Dictionary<InworldCharacterData, CharacterFetchingProgress> m_RequestPool = new Dictionary<InworldCharacterData, CharacterFetchingProgress>();
        #endregion

        #region Events
        public event Action<InworldCharacterData> OnAvatarDownloaded;
        public event Action<InworldCharacterData> OnThumbnailDownloaded;
        public event Action<InworldCharacterData> OnAvatarFailed;
        public event Action<InworldCharacterData> OnThumbnailFailed;
        #endregion

        #region Callbacks
        UnityWebRequest _GetResponse(AsyncOperation op)
        {
            return op is not UnityWebRequestAsyncOperation webTask ? null : webTask.webRequest;
        }
        void OnThumbnailComplete(AsyncOperation op)
        {
            UnityWebRequest uwr = _GetResponse(op);
            if (uwr == null)
                return;
            foreach (InworldCharacterData charData in m_RequestPool.Keys.Where(charData => charData.previewImgUri == uwr.url))
            {
                m_RequestPool[charData].needDownloadThumbnail = false;
                if (uwr.isDone)
                    OnThumbnailDownloaded?.Invoke(charData);
                else
                    OnThumbnailFailed?.Invoke(charData);
            }
        }
        void OnAvatarUpdate(AsyncOperation op)
        {
            UnityWebRequest uwr = _GetResponse(op);
            if (uwr == null)
                return;
            foreach (InworldCharacterData charData in m_RequestPool.Keys.Where(charData => charData.modelUri == uwr.url))
            {
                m_RequestPool[charData].needDownloadAvatar = false;
                if (uwr.isDone)
                    OnAvatarDownloaded?.Invoke(charData);
                else
                    OnAvatarFailed?.Invoke(charData);
            }
        }
        #endregion

        #region Properties & Functions
        /// <summary>
        ///     Return the progress of all the downloading objects.
        /// </summary>
        public float Progress
        {
            get
            {
                if (m_RequestPool.Count == 0)
                    return 100f;
                return m_RequestPool.Sum(req => req.Value.Progress) / m_RequestPool.Count * 100;
            }
        }
        /// <summary>
        ///     Clear all the current Downloading requests.
        /// </summary>
        public void Init()
        {
            m_RequestPool.Clear();
        }

        public CharacterFetchingProgress RequestDownloadThumbnail(InworldCharacterData charData)
        {
            if (string.IsNullOrEmpty(charData.previewImgUri))
                return null;
            if (m_RequestPool.ContainsKey(charData))
                m_RequestPool[charData].needDownloadThumbnail = true;
            else
                m_RequestPool[charData] = new CharacterFetchingProgress(false, true);
            return m_RequestPool[charData];
        }

        public CharacterFetchingProgress RequestDownloadAvatar(InworldCharacterData charData)
        {
            if (string.IsNullOrEmpty(charData.modelUri))
                return null;
            if (m_RequestPool.ContainsKey(charData))
                m_RequestPool[charData].needDownloadAvatar = true;
            else
                m_RequestPool[charData] = new CharacterFetchingProgress(true);
            return m_RequestPool[charData];
        }

        /// <summary>
        ///     Download thumbnail and avatar of Inworld Character Data.
        /// </summary>
        /// <param name="charData">target Inworld character Data</param>
        public void DownloadCharacterData(InworldCharacterData charData)
        {
            if (m_DownloadAvatar)
                DownloadAvatar(charData);
            if (m_DownloadThumbnail)
                DownloadThumbnail(charData);
        }
        /// <summary>
        ///     Download Thumbnail of the Inworld Character Data.
        /// </summary>
        /// <param name="charData">Target Inworld Character Data</param>
        public void DownloadThumbnail(InworldCharacterData charData)
        {
            if (string.IsNullOrEmpty(charData.previewImgUri))
            {
                OnThumbnailFailed?.Invoke(charData);
                return;
            }
            RequestDownloadThumbnail(charData);
            _DownloadThumbnail(charData);
        }
        protected void _DownloadThumbnail(InworldCharacterData charData)
        {
            UnityWebRequest uwrThumbnail = new UnityWebRequest(charData.previewImgUri);
            while (File.Exists(charData.LocalThumbnailFileName))
            {
                charData.index++;
            }
            uwrThumbnail.downloadHandler = new DownloadHandlerFile(charData.LocalThumbnailFileName);
            uwrThumbnail.timeout = 60;
            UnityWebRequestAsyncOperation reqThumbnail = uwrThumbnail.SendWebRequest();
            reqThumbnail.completed += OnThumbnailComplete;
            m_RequestPool[charData].thumbnailProgress = reqThumbnail;
        }
        /// <summary>
        ///     Download Avatar of the Inworld Character Data
        /// </summary>
        /// <param name="charData">Target Inworld Character Data</param>
        public void DownloadAvatar(InworldCharacterData charData)
        {
            if (string.IsNullOrEmpty(charData.modelUri))
            {
                OnAvatarFailed?.Invoke(charData);
                return;
            }
            RequestDownloadAvatar(charData);
            _DownloadAvatar(charData);
        }
        protected void _DownloadAvatar(InworldCharacterData charData)
        {
            UnityWebRequest uwrAvatar = new UnityWebRequest(charData.modelUri);
            while (File.Exists(charData.LocalAvatarFileName))
            {
                charData.index++;
            }
            uwrAvatar.downloadHandler = new DownloadHandlerFile(charData.LocalAvatarFileName);
            uwrAvatar.timeout = 300;
            
            UnityWebRequestAsyncOperation reqAvatar = uwrAvatar.SendWebRequest();
            reqAvatar.completed += OnAvatarUpdate;
            m_RequestPool[charData].avatarProgress = reqAvatar;
        }
        #endregion
    }
}

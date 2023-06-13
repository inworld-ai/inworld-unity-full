/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Util;
using System.Collections;
using System.Linq;
using UnityEngine;
namespace Inworld.Runtime
{
    public class RuntimeFileDownloader : InworldFileDownloader
    {
        [SerializeField] int m_DownloadThreads = 4;
        public bool IsRunning { get; set; }

        void Awake()
        {
            Init();
            IsRunning = true;
        }
        void Start()
        {
            StartCoroutine(_BatchAllProgress());
        }
        void OnEnable()
        {
            IsRunning = true;
        }
        void OnDisable()
        {
            IsRunning = false;
        }
        public bool IsDownloading(InworldCharacterData data)
        {
            return m_RequestPool.ContainsKey(data);
        }
        void _CheckAvatarDownloading(InworldCharacterData charData)
        {
            if (!m_RequestPool.ContainsKey(charData))
                return;
            CharacterFetchingProgress progress = m_RequestPool[charData];
            if (progress.avatarProgress == null && progress.needDownloadAvatar)
                _DownloadAvatar(charData);
        }
        void _CheckThumbnailDownloading(InworldCharacterData charData)
        {
            if (!m_RequestPool.ContainsKey(charData))
                return;
            CharacterFetchingProgress progress = m_RequestPool[charData];
            if (progress.thumbnailProgress == null && progress.needDownloadThumbnail)
                _DownloadThumbnail(charData);
        }
        IEnumerator _BatchAllProgress()
        {
            while (IsRunning)
            {
                for (int i = 0; i < Mathf.Min(m_DownloadThreads, m_RequestPool.Count); i++)
                {
                    InworldCharacterData charData = m_RequestPool.ElementAt(i).Key;
                    _CheckThumbnailDownloading(charData);
                    yield return new WaitForSeconds(0.1f);
                    _CheckAvatarDownloading(charData);
                    yield return new WaitForSeconds(0.1f);
                    if (m_RequestPool[charData].Progress > 0.95f)
                    {
                        m_RequestPool.Remove(charData);
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}

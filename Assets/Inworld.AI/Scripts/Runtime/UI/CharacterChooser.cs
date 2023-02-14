/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Util;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Inworld.Runtime
{
    public class CharacterChooser : MonoBehaviour
    {
        [SerializeField] TMP_Text m_Title;
        [SerializeField] RawImage m_Image;
        [SerializeField] Image m_Mask;
        [SerializeField] TMP_Text m_LoadingProgress;
        InworldSceneData m_ChosenScene;
        InworldCharacterData m_Data;
        int m_nCurrentIdx;
        CharacterFetchingProgress m_Progress;
        readonly List<string> m_SceneList = new List<string>();

        public bool UseLocalAvatar { get; set; } = true;
        public InworldCharacterData CharacterData
        {
            get => m_Data;
            set
            {
                m_Data = value;
                InitSceneList();
                m_Title.text = m_Data.characterName;
                if (m_Data.Thumbnail)
                    m_Image.texture = m_Data.Thumbnail;
                else
                {
                    // YAN: In runtime, we need to gradually fetch all the data,
                    //      we cannot download all at once.
                    m_Progress = RuntimeCanvas.File.RequestDownloadThumbnail(m_Data);
                }
                if (File.Exists(CharacterData.LocalAvatarFileName))
                    UseLocalAvatar = true;
                else
                {
                    m_Progress = RuntimeCanvas.File.RequestDownloadAvatar(m_Data);
                }
            }
        }
        public void Update()
        {
            if (m_Progress == null || !m_Data || !RuntimeCanvas.File.IsDownloading(m_Data))
            {
                m_Mask.gameObject.SetActive(false);
                return;
            }
            m_Mask.gameObject.SetActive(true);
            m_Mask.fillAmount = 1 - m_Progress.Progress;
            m_LoadingProgress.text = m_Progress.Progress == 0 ? "Loading" : $"{m_Progress.Progress * 100:F2}%";
        }
        void OnEnable()
        {
            RuntimeCanvas.File.OnAvatarDownloaded += OnAvatarLoaded;
            RuntimeCanvas.File.OnAvatarFailed += OnAvatarFailed;
            RuntimeCanvas.File.OnThumbnailDownloaded += OnThumbnailLoaded;
            RuntimeCanvas.File.OnThumbnailFailed += OnThumbnailFailed;
        }
        void OnDisable()
        {
            RuntimeCanvas.File.OnAvatarDownloaded -= OnAvatarLoaded;
            RuntimeCanvas.File.OnAvatarFailed -= OnAvatarFailed;
            RuntimeCanvas.File.OnThumbnailDownloaded -= OnThumbnailLoaded;
            RuntimeCanvas.File.OnThumbnailFailed -= OnThumbnailFailed;
        }
        public void Prev()
        {
            m_nCurrentIdx = (m_nCurrentIdx + m_SceneList.Count - 1) % m_SceneList.Count;
            _DisplayTitle();
        }
        public void Next()
        {
            m_nCurrentIdx = (m_nCurrentIdx + m_SceneList.Count + 1) % m_SceneList.Count;
            _DisplayTitle();
        }
        void _DisplayTitle()
        {
            if (m_SceneList[m_nCurrentIdx] == m_Data.characterName)
            {
                m_Title.text = m_Data.characterName;
                m_ChosenScene = null;
            }
            else if (InworldAI.User.InworldScenes.ContainsKey(m_SceneList[m_nCurrentIdx]))
            {
                m_ChosenScene = InworldAI.User.InworldScenes[m_SceneList[m_nCurrentIdx]];
                m_Title.text = m_ChosenScene.ShortName;
            }
        }
        public void ChooseCurrentSet()
        {
            InworldCharacter iwChar = RuntimeCanvas.Instance.BindCharacter(m_Data);
            InworldAI.AvatarLoader.ConfigureModel(iwChar, m_Data.Avatar);
            InworldAI.Game.currentWorkspace = InworldAI.User.Workspaces.Values.FirstOrDefault(ws => ws.fullName == m_Data.workspace);
            if (!InworldAI.Game.currentWorkspace)
            {
                Debug.LogError($"Cannot Find Workspace for {m_Data.brain}!");
                return;
            }
            InworldAI.Game.currentScene = m_ChosenScene;
            InworldController.CurrentScene = InworldAI.Game.currentScene;
            InworldAI.Game.currentKey = InworldAI.Game.currentWorkspace.DefaultKey;
            RuntimeCanvas.Instance.GotoCharacter();
        }
        public void InitSceneList()
        {
            m_SceneList.Clear();
            m_SceneList.Add(m_Data.characterName);
            m_SceneList.AddRange(m_Data.scenes);
        }
        void OnAvatarLoaded(InworldCharacterData charData)
        {
            if (!m_Data || charData.brain != m_Data.brain)
                return;
            Debug.Log($"Loaded Avatar {m_Data.characterName}");
        }
        void OnAvatarFailed(InworldCharacterData charData)
        {
            if (!m_Data || charData.brain != m_Data.brain)
                return;
            Debug.LogError($"Avatar Failed {m_Data.characterName}");
        }
        void OnThumbnailLoaded(InworldCharacterData charData)
        {
            if (!m_Data || charData.brain != m_Data.brain)
                return;
            if (m_Data.Thumbnail == null)
                Debug.LogError($"FUCK {charData.characterName}");
            m_Image.texture = m_Data.Thumbnail;
        }
        void OnThumbnailFailed(InworldCharacterData charData)
        {
            if (!m_Data || charData.brain != m_Data.brain)
                return;
            m_Image.texture = InworldAI.Settings.DefaultThumbnail;
        }
        public void ClearList()
        {
            m_SceneList.Clear();
        }
    }
}

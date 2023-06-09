/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Inworld.Runtime
{
    /// <summary>
    /// This is the data initializing scripts under InworldController. 
    /// </summary>
    public class InitInworld : MonoBehaviour
    {
        [SerializeField] bool m_Updatable = true;
        [SerializeField] InworldWorkspaceData m_WSData;
        [SerializeField] InworldSceneData m_InworldSceneData;
        [SerializeField] InworldCharacterData m_CharData;
        [SerializeField] InworldKeySecret m_KeySecret;
        
        const string k_ErrorTitle = "Data could not be applied to the Unity scene";
        const string k_ErrorContent = "It is suggested to insert the character into a separate scene to avoid data conflict.\nWould you still like to proceed with applying the data to the current scene?";
        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (m_WSData)
                InworldAI.Game.currentWorkspace = m_WSData;
            if (m_InworldSceneData)
                InworldAI.Game.currentScene = m_InworldSceneData;
            if (m_CharData)
                InworldAI.Game.currentCharacter = m_CharData;
            if (m_KeySecret)
                InworldAI.Game.currentKey = m_KeySecret;
        }
        void _SetData(InworldWorkspaceData wsData, InworldSceneData sceneData, InworldCharacterData charData, InworldKeySecret keySecret)
        {
            m_Updatable = true;
            if (InworldController.Instance)
                InworldController.CurrentScene = sceneData;
            m_WSData = wsData;
            m_InworldSceneData = sceneData;
            m_CharData = charData;
            m_KeySecret = keySecret;
        }
        
        #if UNITY_EDITOR
        internal void EditorLoadData()
        {
            if (m_Updatable || EditorUtility.DisplayDialog(k_ErrorTitle, k_ErrorContent, "OK", "Cancel"))
                _SetData(InworldAI.Game.currentWorkspace, InworldAI.Game.currentScene, InworldAI.Game.currentCharacter, InworldAI.Game.currentKey);
        }
        #endif
    }
}

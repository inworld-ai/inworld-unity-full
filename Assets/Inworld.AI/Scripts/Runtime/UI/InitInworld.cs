/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Inworld.Runtime
{
    /// <summary>
    /// If you want to manually load inworld data,
    /// Attach this component to your scene with related scriptable objects.
    /// </summary>
    public class InitInworld : MonoBehaviour
    {
        [SerializeField] InworldWorkspaceData m_WSData;
        [SerializeField] InworldSceneData m_InworldSceneData;
        [SerializeField] InworldCharacterData m_CharData;
        [SerializeField] InworldKeySecret m_KeySecret;
        
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
            InworldAI.Game.currentWorkspace = m_WSData;
            InworldAI.Game.currentScene = m_InworldSceneData;
            InworldAI.Game.currentCharacter = m_CharData;
            InworldAI.Game.currentKey = m_KeySecret;
        }
    }
}

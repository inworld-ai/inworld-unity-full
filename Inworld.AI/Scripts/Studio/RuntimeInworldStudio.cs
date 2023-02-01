/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Ai.Inworld.Studio.V1Alpha;
using Inworld.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
namespace Inworld.Studio
{
    public class StudioEvent : UnityEvent<StudioStatus, string> {}
    /// <summary>
    ///     This class is used to acquire studio token, connecting to studio server, and fetching data in runtime.
    /// </summary>
    public class RuntimeInworldStudio : SingletonBehavior<RuntimeInworldStudio>, IStudioDataHandler
    {
        void Awake()
        {
            m_Studio = new InworldStudio(this);
            m_Event = new StudioEvent();
            m_Progresses = new Dictionary<InworldWorkspaceData, WorkspaceFetchingProgress>();
        }
        void Start()
        {
            if (!InworldAI.Settings.EnableSharedCharacters)
                return;
            InworldWorkspaceData defaultWorkspace = InworldAI.Settings.DefaultWorkspace;
            InworldAI.User.Workspaces[defaultWorkspace.fullName] = defaultWorkspace;
            foreach (InworldSceneData sceneData in defaultWorkspace.scenes)
            {
                InworldAI.User.InworldScenes[sceneData.fullName] = sceneData;
            }
            foreach (InworldCharacterData characterData in defaultWorkspace.characters)
            {
                InworldAI.User.Characters[characterData.brain] = characterData;
            }
        }

        #region Private Variables
        InworldStudio m_Studio;
        StudioEvent m_Event;
        Dictionary<InworldWorkspaceData, WorkspaceFetchingProgress> m_Progresses;
        #endregion

        #region Properties
        public static StudioEvent Event => Instance.m_Event;
        public float Progress => m_Progresses.Count == 0 ? 0 :
            m_Progresses.Sum(kvp => kvp.Value.Progress) / m_Progresses.Count;
        #endregion

        #region Private Functions
        InworldWorkspaceData _CreateWorkspace(Workspace ws)
        {
            InworldWorkspaceData inst = ScriptableObject.CreateInstance<InworldWorkspaceData>();
            inst.title = ws.DisplayName;
            inst.fullName = ws.Name;
            inst.characters = new List<InworldCharacterData>();
            inst.scenes = new List<InworldSceneData>();
            return inst;
        }
        InworldSceneData _CreateInworldScene(Scene scene)
        {
            InworldSceneData inst = ScriptableObject.CreateInstance<InworldSceneData>();
            inst.fullName = scene.Name;
            inst.description = scene.Description;
            inst.triggers = new List<string>();
            foreach (Scene.Types.SceneTrigger trigger in scene.SceneTriggers)
            {
                inst.triggers.Add(trigger.Trigger);
            }
            inst.characters = new List<string>();
            foreach (Scene.Types.CharacterReference charRef in scene.CharacterReferences)
            {
                inst.characters.Add(charRef.Character);
            }
            return inst;
        }
        InworldCharacterData _CreateCharacter(InworldWorkspaceData workspaceData, Character character)
        {
            InworldCharacterData inst = ScriptableObject.CreateInstance<InworldCharacterData>();
            inst.characterName = character.DefaultCharacterDescription.GivenName;
            inst.brain = character.Name;
            inst.modelUri = character.DefaultCharacterAssets.RpmModelUri;
            inst.previewImgUri = character.DefaultCharacterAssets.RpmImageUriPortrait;
            inst.posUri = character.DefaultCharacterAssets.RpmImageUriPosture;
            inst.triggers = new List<string>();
            inst.workspace = workspaceData.fullName;
            inst.currentScene = inst.brain;
            foreach (Character.Types.BrainTrigger trigger in character.BrainTriggers)
            {
                inst.triggers.Add(trigger.Trigger);
            }
            inst.scenes = new List<string>();
            foreach (Scene scene in character.Scenes)
            {
                inst.scenes.Add(scene.Name);
            }
            return inst;
        }
        InworldKeySecret _CreateAPIKeySecret(ApiKey apiKey)
        {
            InworldKeySecret inst = ScriptableObject.CreateInstance<InworldKeySecret>();
            inst.key = apiKey.Key;
            inst.secret = apiKey.Secret;
            return inst;
        }
        public async Task ListScenes(InworldWorkspaceData workspace)
        {
            if (workspace.scenes.Count != 0)
            {
                foreach (InworldSceneData sceneData in workspace.scenes.Where(sceneData => !string.IsNullOrEmpty(sceneData.fullName)))
                {
                    m_Event.Invoke(StudioStatus.ListAScene, sceneData.fullName);
                }
                m_Event.Invoke(StudioStatus.ListSceneCompleted, "");
            }
            else
            {
                await m_Studio.ListScenes(workspace);
            }
        }
        public async Task ListCharacters(InworldWorkspaceData workspace)
        {
            if (workspace.characters.Count != 0)
            {
                foreach (InworldCharacterData characterData in workspace.characters)
                {
                    m_Event.Invoke(StudioStatus.ListACharacter, characterData.brain);
                }
                m_Event.Invoke(StudioStatus.ListCharacterCompleted, "");
            }
            else
            {
                await m_Studio.ListCharacters(workspace);
            }
        }
        public void ListCharacters(InworldSceneData inworldScene)
        {
            if (inworldScene.characters.Count == 0)
            {
                m_Event.Invoke(StudioStatus.ListCharacterFailed, "");
                return;
            }
            foreach (string brain in inworldScene.characters)
            {
                m_Event.Invoke(StudioStatus.ListACharacter, brain);
            }
            m_Event.Invoke(StudioStatus.ListCharacterCompleted, "");
        }
        public async Task ListAPIKey(InworldWorkspaceData workspace)
        {
            if (workspace.integrations != null && workspace.integrations.Count != 0)
            { 
                foreach (InworldKeySecret key in workspace.integrations)
                {
                    m_Event.Invoke(StudioStatus.ListAKey, key.key);
                }
                m_Event.Invoke(StudioStatus.ListKeyCompleted, "");
            }
            else
            {
                await m_Studio.ListAPIKey(workspace);
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        ///     Acquiring studio token
        /// </summary>
        /// <param name="tokenForExchange">id token to exchange.</param>
        public async void Init(string tokenForExchange)
        {
            await m_Studio.GetUserToken(tokenForExchange);
        }
        /// <summary>
        ///     Acquiring studio token by oculus account.
        /// </summary>
        /// <param name="oculusNonce">random string generated from oculus</param>
        /// <param name="oculusID">oculus id</param>
        public async void Init(string oculusNonce, string oculusID)
        {
            await m_Studio.GetUserToken($"{oculusNonce}|{oculusID}", AuthType.OculusNonce);
        }

        public async void ListWorkspace()
        {
            await m_Studio.ListWorkspace();
        }
        /// <summary>
        ///     Create Workspace, based on the response of ListWorkspace.
        /// </summary>
        /// <param name="workspaces">the workspace list</param>
        public async void CreateWorkspaces(List<Workspace> workspaces)
        {
            Dictionary<string, InworldWorkspaceData> wsData = InworldAI.User.Workspaces;
            wsData.Clear();
            foreach (Workspace ws in workspaces)
            {
                wsData[ws.Name] = _CreateWorkspace(ws);
                m_Progresses[wsData[ws.Name]] = new WorkspaceFetchingProgress();
            }
            foreach (Workspace ws in workspaces)
            {
                await ListScenes(wsData[ws.Name]);
                await ListAPIKey(wsData[ws.Name]);
                await ListCharacters(wsData[ws.Name]);
            }
            m_Event.Invoke(StudioStatus.ListWorkspaceCompleted, $"{Progress:F2}");
            InworldAI.Game.CurrentWorkspace = null;
        }

        /// <summary>
        ///     Create Scenes, based on the response of ListScene.
        /// </summary>
        /// <param name="workspace">the target workspace</param>
        /// <param name="scenes">the returned list of scenes</param>
        public void CreateScenes(InworldWorkspaceData workspace, List<Scene> scenes)
        {
            workspace.scenes ??= new List<InworldSceneData>();
            workspace.scenes.Clear();
            if (m_Progresses.ContainsKey(workspace))
                m_Progresses[workspace].totalScene = scenes.Count;
            foreach (Scene scene in scenes)
            {
                InworldAI.User.InworldScenes[scene.Name] = _CreateInworldScene(scene);
                m_Progresses[workspace].currentScene++;
                workspace.scenes.Add(InworldAI.User.InworldScenes[scene.Name]);
                m_Event.Invoke(StudioStatus.ListAScene, scene.Name);
            }
            m_Event.Invoke(StudioStatus.ListSceneCompleted, $"{Progress:F2}");
        }

        /// <summary>
        ///     Create Characters
        /// </summary>
        /// <param name="workspace">the target workspace</param>
        /// <param name="characters">the response list of characters</param>
        public void CreateCharacters(InworldWorkspaceData workspace, List<Character> characters)
        {
            workspace.characters ??= new List<InworldCharacterData>();
            workspace.characters.Clear();
            if (m_Progresses.ContainsKey(workspace))
                m_Progresses[workspace].totalCharacters = characters.Count;
            foreach (Character character in characters)
            {
                // NOTE(Yan): character.Name is actually CharacterData.brain, not characterName.
                InworldAI.User.Characters[character.Name] = _CreateCharacter(workspace, character);
                m_Progresses[workspace].currentCharacters++;
                workspace.characters.Add(InworldAI.User.Characters[character.Name]);
                m_Event.Invoke(StudioStatus.ListACharacter, character.Name);
            }
            m_Event.Invoke(StudioStatus.ListCharacterCompleted, $"{Progress:F2}");
        }

        /// <summary>
        ///     Create Integrations
        /// </summary>
        /// <param name="workspace">the target workspace</param>
        /// <param name="apiKeys">the response list of api key</param>
        public void CreateIntegrations(InworldWorkspaceData workspace, List<ApiKey> apiKeys)
        {
            workspace.integrations ??= new List<InworldKeySecret>();
            workspace.integrations.Clear();
            if (m_Progresses.ContainsKey(workspace))
                m_Progresses[workspace].totalKeys = apiKeys.Count;
            foreach (ApiKey key in apiKeys.Where(key => key.State == ApiKey.Types.State.Active))
            {
                workspace.integrations.Add(_CreateAPIKeySecret(key));
                m_Progresses[workspace].currentKeys++;
                m_Event.Invoke(StudioStatus.ListAKey, key.Key);
            }
            m_Event.Invoke(StudioStatus.ListKeyCompleted, $"{Progress:F2}");
        }

        /// <summary>
        ///     Process Error Message from Studio Server.
        /// </summary>
        /// <param name="studioStatus">The Error Reason</param>
        /// <param name="msg">the detailed message</param>
        public void OnStudioError(StudioStatus studioStatus, string msg)
        {
            m_Event.Invoke(studioStatus, msg);
        }

        /// <summary>
        ///     Invoke the event once Session Token is received.
        /// </summary>
        public void OnUserTokenCompleted()
        {
            m_Event.Invoke(StudioStatus.Initialized, "");
        }
        #endregion
    }
}

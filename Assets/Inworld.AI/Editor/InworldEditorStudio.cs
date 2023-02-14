/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Ai.Inworld.Studio.V1Alpha;
using Inworld.Studio;
using Inworld.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
#if UNITY_EDITOR
namespace Inworld.Editor
{
    /// <summary>
    ///     Inworld Editor has 3 parts.
    ///     This part is for the Connecting with Inworld Studio Server.
    ///     The other parts are for UI supports, and local data saving.
    /// </summary>
    public partial class InworldEditor : IStudioDataHandler
    {
        /// <summary>
        ///     Reconnect to Inworld Server.
        /// </summary>
        public void Reconnect()
        {
            WWWForm form = new WWWForm();
            form.AddField("grant_type", "refresh_token");
            form.AddField("refresh_token", $"{InworldAI.User.RefreshToken}");
            UnityWebRequest uwr = UnityWebRequest.Post($"{k_FirebaseURL}", form);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            UnityWebRequestAsyncOperation task = uwr.SendWebRequest();
            task.completed += OnTokenRefreshed;
        }

        #region Private Variable
        InworldStudio m_Studio;
        const string k_FirebaseURL = "https://securetoken.googleapis.com/v1/token?key=AIzaSyAPVBLVid0xPwjuU4Gmn_6_GyqxBq-SwQs";
        #endregion

        #region Call Backs
        void OnTokenRefreshed(AsyncOperation obj)
        {
            if (obj is not UnityWebRequestAsyncOperation webTask)
            {
                ErrorMessage = "Refresh Failed: Not a webTask";
                return;
            }
            if (webTask.webRequest == null)
            {
                ErrorMessage = "Refresh Failed: No Handler";
                return;
            }
            UnityWebRequest uwr = webTask.webRequest;
            if (uwr == null)
            {
                ErrorMessage = "Refresh Failed: No WebRequest";
                return;
            }
            uwr.uploadHandler?.Dispose(); //Yan: Not disposing would lead to mem leak!
            if (!uwr.isDone)
            {
                ErrorMessage = "Refresh Failed: NetworkError";
                return;
            }
            if (uwr.downloadHandler is not DownloadHandlerBuffer buffer)
            {
                ErrorMessage = "Refresh Failed: Data Fetch Error";
                return;
            }
            FBTokenResponse output = JsonConvert.DeserializeObject<FBTokenResponse>(buffer.text);
            if (output == null || string.IsNullOrEmpty(output.id_token) || string.IsNullOrEmpty(output.refresh_token))
                Status = InworldEditorStatus.Error;
            else
            {
                InworldAI.User.RefreshTokens(output.id_token, output.refresh_token);
                GetUserToken(output.id_token);
            }
        }
        public void CreateWorkspaces(List<Workspace> workspaces)
        {
            Dictionary<string, InworldWorkspaceData> wsData = InworldAI.User.Workspaces;
            wsData.Clear();
            foreach (Workspace ws in workspaces)
            {
                wsData[ws.Name] = _CreateAWorkspace(ws);
                CurrentProgress += 100f / workspaces.Count;
            }
            DropdownField wsChooser = Root.Q<DropdownField>("WorkspaceChooser");
            if (wsChooser != null)
            {
                wsChooser.choices = InworldAI.User.Workspaces.Values.Select(ws => ws.title).ToList();
            }
            m_CurrentState?.OnConnected();
        }
        public void CreateScenes(InworldWorkspaceData wsData, List<Scene> scenes)
        {
            wsData.scenes ??= new List<InworldSceneData>();
            wsData.scenes.Clear();
            Progress[wsData].totalScene = scenes.Count;

            foreach (Scene scene in scenes)
            {
                InworldAI.User.InworldScenes[scene.Name] = _CreateInworldScene(scene);
                Progress[wsData].currentScene++;
                wsData.scenes.Add(InworldAI.User.InworldScenes[scene.Name]);
            }
        }
        public void CreateCharacters(InworldWorkspaceData wsData, List<Character> characters)
        {
            wsData.characters ??= new List<InworldCharacterData>();
            wsData.characters.Clear();
            Progress[wsData].totalCharacters = characters.Count;
            foreach (Character character in characters)
            {
                // NOTE(Yan): character.Name is actually CharacterData.brain, not characterName.
                InworldAI.User.Characters[character.Name] = _CreateACharacter(character);
                Progress[wsData].currentCharacters++;
                wsData.characters.Add(InworldAI.User.Characters[character.Name]);
            }
        }
        public void CreateIntegrations(InworldWorkspaceData workspace, List<ApiKey> apiKeys)
        {
            workspace.integrations ??= new List<InworldKeySecret>();
            workspace.integrations.Clear();
            Progress[workspace].totalKeys = apiKeys.Count;
            foreach (ApiKey key in apiKeys.Where(key => key.State == ApiKey.Types.State.Active))
            {
                workspace.integrations.Add(_CreateAPIKeySecret(key));
                Progress[workspace].currentKeys++;
            }
        }
        public void OnStudioError(StudioStatus studioStatus, string msg)
        {
            if (msg.Contains("StatusCode="))
            {
                int nFirstIndex = msg.LastIndexOf("StatusCode=", StringComparison.Ordinal);
                string tmp = msg.Substring(nFirstIndex, msg.IndexOf(",", StringComparison.Ordinal) - nFirstIndex);
                ErrorMessage = $"{studioStatus} Error: {tmp}";
            }
            else
                ErrorMessage = $"{studioStatus} Error: {msg}";
            if (studioStatus == StudioStatus.InitFailed && !string.IsNullOrEmpty(InworldAI.User.RefreshToken))
                Reconnect();
        }

        public void OnUserTokenCompleted()
        {
            if (string.IsNullOrEmpty(InworldAI.User.Account) || !InworldAI.User.IsAccountValid)
                LoginStudio();
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(InworldAI.User.Account))
                VSAttribution.VSAttribution.SendAttributionEvent("Login Studio", InworldAI.k_CompanyName, InworldAI.User.Account);
#endif
            ListWorkspace();
        }
        #endregion

        #region Private Functions
        internal async void GetUserToken(string tokenForExchange, AuthType authType = AuthType.Firebase)
        {
            await m_Studio.GetUserToken(tokenForExchange, authType);
        }
        async void ListWorkspace()
        {
            await m_Studio.ListWorkspace();
        }
        internal async void ListScenes(InworldWorkspaceData ws)
        {
            await m_Studio.ListScenes(ws);
        }
        internal async void ListAPIKey(InworldWorkspaceData ws)
        {
            await m_Studio.ListAPIKey(ws);
        }
        internal async void ListCharacters(InworldWorkspaceData ws)
        {
            await m_Studio.ListCharacters(ws);
        }
        internal async void LoginStudio()
        {
            await m_Studio.LoginStudio();
        }

        InworldWorkspaceData _CreateAWorkspace(Workspace workspace)
        {
            InworldWorkspaceData inst = _GetWSData(workspace.Name, out int index);
            inst ??= CreateInstance<InworldWorkspaceData>();
            inst.title = workspace.DisplayName;
            inst.fullName = workspace.Name;
            inst.characters = new List<InworldCharacterData>();
            inst.scenes = new List<InworldSceneData>();
            inst.index = index;
            Progress[inst] = new WorkspaceFetchingProgress();
            InworldAI.Log($"Load Workspace: {inst.title}");
            return inst;
        }
        InworldSceneData _CreateInworldScene(Scene scene)
        {
            InworldSceneData inst = _GetIwSceneData(scene.Name, out int index);
            inst ??= CreateInstance<InworldSceneData>();
            inst.fullName = scene.Name;
            inst.shortName = scene.DisplayName;
            inst.description = scene.Description;
            inst.triggers = new List<string>();
            inst.index = index;
            foreach (Scene.Types.SceneTrigger trigger in scene.SceneTriggers)
            {
                inst.triggers.Add(trigger.Trigger);
            }
            inst.characters = new List<string>();
            foreach (Scene.Types.CharacterReference charRef in scene.CharacterReferences)
            {
                inst.characters.Add(charRef.Character);
            }
            InworldAI.Log($"-- Load Scene: {inst.ShortName}");
            return inst;
        }
        InworldCharacterData _CreateACharacter(Character character)
        {
            InworldCharacterData inst = _GetCharData(character.Name, out int index);
            inst ??= CreateInstance<InworldCharacterData>();
            inst.characterName = character.DefaultCharacterDescription.GivenName;
            inst.brain = character.Name;
            inst.modelUri = character.DefaultCharacterAssets.RpmModelUri;
            inst.previewImgUri = character.DefaultCharacterAssets.RpmImageUriPortrait;
            inst.posUri = character.DefaultCharacterAssets.RpmImageUriPosture;
            inst.triggers = new List<string>();
            inst.workspace = inst.brain.Split("/characters/")[0];
            inst.currentScene = inst.brain;
            inst.index = index;
            foreach (Character.Types.BrainTrigger trigger in character.BrainTriggers)
            {
                inst.triggers.Add(trigger.Trigger);
            }
            inst.scenes = new List<string>();
            foreach (Scene scene in character.Scenes)
            {
                inst.scenes.Add(scene.Name);
            }
            InworldAI.Log($"-- Load Character: {inst.characterName}");
            return inst;
        }
        InworldKeySecret _CreateAPIKeySecret(ApiKey apiKey)
        {
            InworldKeySecret inst = _GetKey(InworldAI.Settings.KeySecretDataPath, $"Key-{apiKey.Key}");
            inst ??= CreateInstance<InworldKeySecret>();
            inst.key = apiKey.Key;
            inst.secret = apiKey.Secret;
            InworldAI.Log($"-- Load Key: {inst.ShortName}");
            return inst;
        }
        #endregion
    }
}
#endif

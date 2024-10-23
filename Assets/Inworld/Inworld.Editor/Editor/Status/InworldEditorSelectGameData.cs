/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using Inworld.Entities;
using Newtonsoft.Json;

namespace Inworld.Editors
{
    // YAN: At this moment, the ws data has already filled.
    public class InworldEditorSelectGameData : IEditorState
    {
        const string k_DefaultWorkspace = "--- SELECT WORKSPACE ---";
        const string k_DefaultKey = "--- SELECT KEY---";
        const string k_DefaultGameMode = "--- SELECT USAGE ---";
        const string k_DataMissing = "Some data is missing.\nPlease make sure you have at least one scene and one key/secret in your workspace";
        const string k_LLMService = "LLM Service";
        const string k_CharacterIntegration = "Character Integration";
        string m_CurrentWorkspaceName = "--- SELECT WORKSPACE ---";
        string m_CurrentKey = "--- SELECT KEY---";
        string m_CurrentGameMode = "--- SELECT USAGE ---";

        bool m_IsCharIntegration = true;
        bool m_DisplayDataMissing;
        bool m_StartDownload;
       
        InworldWorkspaceData CurrentWorkspace => InworldAI.User.GetWorkspaceByDisplayName(m_CurrentWorkspaceName);
        InworldKeySecret CurrentKey  => CurrentWorkspace?.keySecrets.FirstOrDefault(key => key.key == m_CurrentKey);

        bool _IsReadyToProceed => CurrentWorkspace.Progress > 0.95f;
        /// <summary>
        /// Triggers when open editor window.
        /// </summary>
        public void OnOpenWindow()
        {
            
        }
        /// <summary>
        /// Triggers when drawing the title of the editor panel page.
        /// </summary>
        public void DrawTitle()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Welcome, {InworldAI.User.Name}", InworldEditor.Instance.TitleStyle);
            EditorGUILayout.Space();
        }
        /// <summary>
        /// Triggers when drawing the content of the editor panel page.
        /// </summary>
        public void DrawContent()
        {
            _DrawWorkspaceDropDown();
            _DrawKeyDropDown();
            _DrawGameModedDropDown();
            if (m_DisplayDataMissing)
                EditorGUILayout.LabelField(k_DataMissing, InworldEditor.Instance.TitleStyle);
        }
        /// <summary>
        /// Triggers when drawing the buttons at the bottom of the editor panel page.
        /// </summary>
        public void DrawButtons()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Back", InworldEditor.Instance.BtnStyle))
            {
                InworldEditor.Instance.Status = EditorStatus.Init;
            }
            if (m_CurrentWorkspaceName != k_DefaultWorkspace && !string.IsNullOrEmpty(m_CurrentWorkspaceName))
            {
                if (GUILayout.Button("Refresh", InworldEditor.Instance.BtnStyle))
                {
                    _SelectWorkspace(m_CurrentWorkspaceName);
                }
            }
            if (m_CurrentKey != k_DefaultKey && m_CurrentGameMode != k_DefaultGameMode && !string.IsNullOrEmpty(m_CurrentKey) && _IsReadyToProceed)
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Next", InworldEditor.Instance.BtnStyle))
                {
                    _SaveCurrentSettings();
                    _CreatePrefabVariants();
                    if (m_IsCharIntegration)
                        InworldEditor.Instance.Status = EditorStatus.SelectCharacter; 
                    else
                        InworldEditor.Instance.Status = EditorStatus.SelectLLMConfig;
                }
            }
            GUILayout.EndHorizontal();
        }
        /// <summary>
        /// Triggers when this state exits.
        /// </summary>
        public void OnExit()
        {
            
        }
        /// <summary>
        /// Triggers when this state enters.
        /// </summary>
        public void OnEnter()
        {
            m_DisplayDataMissing = false;
            m_StartDownload = false;
            m_CurrentKey = k_DefaultKey;
            m_CurrentWorkspaceName = k_DefaultWorkspace;
            if (InworldAI.User.Workspace.Count != 1)
                return;
            _SelectWorkspace(InworldAI.User.Workspace[0].displayName);
        }

        /// <summary>
        /// Triggers when other general update logic has been finished.
        /// </summary>
        public void PostUpdate()
        {
            if (!m_StartDownload)
                return;
            //TODO(Yan): Use Workspace to download characters.
            InworldWorkspaceData wsData = CurrentWorkspace;
            if (wsData == null)
                return;
            bool isCancelled = EditorUtility.DisplayCancelableProgressBar("Inworld", "Downloading Assets", CurrentWorkspace.Progress);
            if (isCancelled || CurrentWorkspace.Progress > 0.95f)
            {
                EditorUtility.ClearProgressBar();
                m_StartDownload = false;
            }
        }
        public void OnClose()
        {
            
        }

        void _SaveCurrentSettings()
        {
            InworldGameData gameData = _CreateGameDataAssets();
            InworldController controller = Object.FindFirstObjectByType<InworldController>();
            if (!controller)
                controller = PrefabUtility.InstantiatePrefab(InworldEditor.Instance.ControllerPrefab) as InworldController;
            if (!controller)
                return;
            controller.GameData = gameData;
            controller.transform.position = Vector3.zero; // YAN: Reset position for RPM Animation.
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        void _DownloadRelatedAssets()
        {
            Debug.Log("Download Related Assets...");
            InworldWorkspaceData wsData = CurrentWorkspace;
            if (wsData == null)
                return;
            m_StartDownload = true;
            foreach (InworldCharacterData charRef in wsData.characters)
            {
                string thumbURL = charRef.characterAssets.ThumbnailURL;
                string thumbFileName = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.ThumbnailPath}/{charRef.CharacterFileName}.png";
                
                string modelURL = charRef.characterAssets.rpmModelUri;
                string modelFileName = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.AvatarPath}/{charRef.CharacterFileName}.glb";

                if (!string.IsNullOrEmpty(thumbURL) && !File.Exists(thumbFileName))
                {
                    InworldEditorUtil.DownloadCharacterAsset(charRef.brainName, thumbURL, _OnCharThumbnailDownloaded);
                    charRef.characterAssets.thumbnailProgress = 0.1f;
                }
                else
                    charRef.characterAssets.thumbnailProgress = 1f;
                if (!string.IsNullOrEmpty(modelURL) && !File.Exists(modelFileName))
                {
                    InworldEditorUtil.DownloadCharacterAsset(charRef.brainName, modelURL, _OnCharModelDownloaded);
                    charRef.characterAssets.avatarProgress = 0.1f;
                }
                else
                    charRef.characterAssets.avatarProgress = 1;
            }
            // Meanwhile, showcasing progress bar.
        }

        InworldGameData _CreateGameDataAssets()
        {
            // Create a new SO.
            InworldGameData gameData = ScriptableObject.CreateInstance<InworldGameData>();
            InworldWorkspaceData ws = CurrentWorkspace;
            if (ws != null)
            {
                gameData.Init(ws.name, CurrentKey);
            }
            gameData.capabilities = new Capabilities(InworldAI.Capabilities);
            if (string.IsNullOrEmpty(InworldEditorUtil.UserDataPath))
            {
                InworldEditor.Instance.Error = "Failed to save game data: Current User Setting is null.";
                return null;
            }
            if (!Directory.Exists($"{InworldEditorUtil.UserDataPath}/{InworldEditor.GameDataPath}"))
            {
                Directory.CreateDirectory($"{InworldEditorUtil.UserDataPath}/{InworldEditor.GameDataPath}");
            }
            string newAssetPath = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.GameDataPath}/{gameData.WsFileName}.asset";
            AssetDatabase.CreateAsset(gameData, newAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return gameData;
        }
        void _DrawWorkspaceDropDown()
        {
            EditorGUILayout.LabelField("Choose Workspace:", InworldEditor.Instance.TitleStyle);
            List<string> wsList = InworldAI.User.Workspace.Select(ws => ws.displayName).ToList();
            InworldEditorUtil.DrawDropDown(m_CurrentWorkspaceName, wsList, _SelectWorkspace);
        }

        void _DrawGameModedDropDown()
        {
            if (m_CurrentWorkspaceName == k_DefaultWorkspace || string.IsNullOrEmpty(m_CurrentWorkspaceName))
                return;
            if (m_CurrentKey == k_DefaultKey || string.IsNullOrEmpty(m_CurrentKey))
                return;
            EditorGUILayout.LabelField("Choose Usage:", InworldEditor.Instance.TitleStyle);
            List<string> wsList = new List<string>{k_LLMService, k_CharacterIntegration};
            InworldEditorUtil.DrawDropDown(m_CurrentGameMode, wsList, _SelectGameMode);
        }
        void _DrawKeyDropDown()
        {
            if (m_CurrentWorkspaceName == k_DefaultWorkspace || string.IsNullOrEmpty(m_CurrentWorkspaceName))
                return;
            InworldWorkspaceData ws = CurrentWorkspace;
            if (ws == null || ws.keySecrets == null || ws.keySecrets.Count == 0)
                return;
            List<string> keyList = ws.keySecrets.Where(key => key.state == "ACTIVE").Select(key => key.key).ToList();
            EditorGUILayout.LabelField("Choose API Key:", InworldEditor.Instance.TitleStyle);
            InworldEditorUtil.DrawDropDown(m_CurrentKey, keyList, _SelectKeys);
            m_CurrentKey = keyList.Count == 1 ? keyList[0] : m_CurrentKey;
        }

        void _CreatePrefabVariants()
        {
            InworldWorkspaceData wsData = CurrentWorkspace;
            // 1. Get the character prefab for character in current workspace. (Default or Specific)
            if (wsData == null)
                return;
            foreach (InworldCharacterData charRef in wsData.characters)
            {
                GameObject downloadedModel = _GetModel(charRef);
                _CreateVariant(charRef, downloadedModel);
            }
            // 2. Save the prefab variant as the new data.
        }
        GameObject _GetModel(InworldCharacterData charRef)
        {
            AssetDatabase.Refresh();
            string filePath = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.AvatarPath}/{charRef.CharacterFileName}.glb";
            return !File.Exists(filePath) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
        }
        static void _CreateVariant(InworldCharacterData charRef, GameObject customModel)
        { 
            // Use Current Model
            InworldCharacter avatar = customModel ?
                Object.Instantiate(InworldEditor.Instance.RPMPrefab) :
                Object.Instantiate(InworldEditor.Instance.InnequinPrefab);

            InworldCharacter iwChar = avatar.GetComponent<InworldCharacter>();
            iwChar.Data = charRef;
            if (customModel)
            {
                GameObject newModel = PrefabUtility.InstantiatePrefab(customModel) as GameObject;
                if (newModel)
                {
                    
                    Transform oldArmature = avatar.transform.Find("Armature");
                    if (oldArmature)
                        Object.DestroyImmediate(oldArmature.gameObject);
                    newModel.transform.name = "Armature";
                    newModel.transform.SetParent(avatar.transform);
                }
            }
            if (!Directory.Exists($"{InworldEditorUtil.UserDataPath}/{InworldEditor.PrefabPath}"))
            {
                Directory.CreateDirectory($"{InworldEditorUtil.UserDataPath}/{InworldEditor.PrefabPath}");
            }
            string newAssetPath = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.PrefabPath}/{charRef.CharacterFileName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(avatar.gameObject, newAssetPath);
            AssetDatabase.SaveAssets();
            Object.DestroyImmediate(avatar.gameObject);
            AssetDatabase.Refresh();
        }
        void _ListKeys()
        {
            string wsFullName = InworldAI.User.GetWorkspaceFullName(m_CurrentWorkspaceName);
            if (string.IsNullOrEmpty(wsFullName))
                return;
            InworldEditorUtil.SendWebGetRequest(InworldEditor.ListKeyURL(wsFullName), true, _ListKeyCompleted);
        }
        void _ListScenes()
        {
            string wsFullName = InworldAI.User.GetWorkspaceFullName(m_CurrentWorkspaceName);
            if (string.IsNullOrEmpty(wsFullName))
                return;
            InworldEditorUtil.SendWebGetRequest(InworldEditor.ListScenesURL(wsFullName), true, _ListSceneCompleted);
        }
        void _ListCharacters()
        {
            string wsFullName = InworldAI.User.GetWorkspaceFullName(m_CurrentWorkspaceName);
            if (string.IsNullOrEmpty(wsFullName))
                return;
            InworldEditorUtil.SendWebGetRequest(InworldEditor.ListCharactersURL(wsFullName), true, _ListCharactersCompleted);
        }
        void _ListCharactersCompleted(AsyncOperation obj)
        {
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(obj);
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                InworldEditor.Instance.Error = $"List Characters Failed: {InworldEditor.GetError(uwr.error)}";
                EditorUtility.ClearProgressBar();
                return;
            }            
            ListCharacterResponse resp = JsonConvert.DeserializeObject<ListCharacterResponse>(uwr.downloadHandler.text);
            if (resp.characters.Count == 0)
            {
                m_DisplayDataMissing = true;
            }
            InworldWorkspaceData ws = CurrentWorkspace;
            if (ws.characters == null)
                ws.characters = new List<InworldCharacterData>();
            ws.characters.Clear();
            resp.characters.ForEach(charOverLoad => ws.characters.Add(new InworldCharacterData(charOverLoad)));
            _DownloadRelatedAssets();
        }
        void _ListSceneCompleted(AsyncOperation obj)
        {
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(obj);
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                InworldEditor.Instance.Error = $"List Scene Failed: {InworldEditor.GetError(uwr.error)}";
                EditorUtility.ClearProgressBar();
                return;
            }
            ListSceneResponse resp = JsonConvert.DeserializeObject<ListSceneResponse>(uwr.downloadHandler.text);
            InworldWorkspaceData ws = CurrentWorkspace;
            if (ws.scenes == null)
                ws.scenes = new List<InworldSceneData>();
            ws.scenes.Clear();
            ws.scenes.AddRange(resp.scenes); 
        }
        void _ListKeyCompleted(AsyncOperation obj)
        {
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(obj);
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                InworldEditor.Instance.Error = $"List Key Failed: {InworldEditor.GetError(uwr.error)}";
                EditorUtility.ClearProgressBar();
                return;
            }
            
            ListKeyResponse resp = JsonUtility.FromJson<ListKeyResponse>(uwr.downloadHandler.text);
            if (resp.apiKeys.Count == 0)
                m_DisplayDataMissing = true;
            InworldWorkspaceData ws = CurrentWorkspace;
            if (ws.keySecrets == null)
                ws.keySecrets = new List<InworldKeySecret>();
            ws.keySecrets.Clear();
            ws.keySecrets.AddRange(resp.apiKeys); 
        }
        void _SelectWorkspace(string workspaceDisplayName)
        {
            m_CurrentWorkspaceName = workspaceDisplayName;
            m_CurrentKey = k_DefaultKey; // YAN: Reset data.
            m_DisplayDataMissing = false;
            m_StartDownload = false;
            _ListCharacters();
            _ListScenes();
            _ListKeys();
        }
        void _SelectKeys(string keyDisplayName)
        {
            m_CurrentKey = keyDisplayName;
        }
        void _SelectGameMode(string gameMode)
        {
            m_CurrentGameMode = gameMode;
            m_IsCharIntegration = gameMode == k_CharacterIntegration;
        }
        // Download Avatars and put under User name's folder.
        void _OnCharModelDownloaded(string charFullName, AsyncOperation downloadContent)
        {
            InworldCharacterData charRef = CurrentWorkspace?.characters.FirstOrDefault(c => c.brainName == charFullName);
            if (charRef == null)
                return;
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(downloadContent);
            
            if (string.IsNullOrEmpty(InworldEditorUtil.UserDataPath) || uwr.result != UnityWebRequest.Result.Success)
            {
                InworldAI.LogError($"Failed to download model: {charFullName} with {uwr.url}");
                charRef.characterAssets.avatarProgress = 1;
                return;
            }
            // YAN: Currently we only download .glb files.
            if (!Directory.Exists($"{InworldEditorUtil.UserDataPath}/{InworldEditor.AvatarPath}"))
            {
                Directory.CreateDirectory($"{InworldEditorUtil.UserDataPath}/{InworldEditor.AvatarPath}");
            }
            string newAssetPath = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.AvatarPath}/{charRef.CharacterFileName}.glb";
            File.WriteAllBytes(newAssetPath, uwr.downloadHandler.data);
            FileInfo currentProgress = new FileInfo(newAssetPath);
            charRef.characterAssets.avatarProgress = (float)currentProgress.Length / uwr.downloadHandler.data.Length;
            if (charRef.characterAssets.avatarProgress > 0.95f)
                AssetDatabase.Refresh();
        }
        void _OnCharThumbnailDownloaded(string charFullName, AsyncOperation downloadContent)
        {
            InworldCharacterData charRef = CurrentWorkspace?.characters.FirstOrDefault(c => c.brainName == charFullName);
            if (charRef == null)
                return;
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(downloadContent);
            
            if (string.IsNullOrEmpty(InworldEditorUtil.UserDataPath) || uwr.result != UnityWebRequest.Result.Success)
            {
                InworldAI.LogError($"Failed to download Thumbnail: {charFullName} with {uwr.url}");
                charRef.characterAssets.thumbnailProgress = 1;
                return;
            }
            if (!Directory.Exists($"{InworldEditorUtil.UserDataPath}/{InworldEditor.ThumbnailPath}"))
            {
                Directory.CreateDirectory($"{InworldEditorUtil.UserDataPath}/{InworldEditor.ThumbnailPath}");
            }
            string newAssetPath = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.ThumbnailPath}/{charRef.CharacterFileName}.png";
            File.WriteAllBytes(newAssetPath, uwr.downloadHandler.data);
            FileInfo currentProgress = new FileInfo(newAssetPath);
            charRef.characterAssets.thumbnailProgress = (float)currentProgress.Length / uwr.downloadHandler.data.Length;
            if (charRef.characterAssets.avatarProgress > 0.95f)
                AssetDatabase.Refresh();
        }
    }
}
#endif
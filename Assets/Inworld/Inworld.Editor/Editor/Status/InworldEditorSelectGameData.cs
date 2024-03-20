/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using Inworld.Editors.Graph;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using Inworld.Entities;

namespace Inworld.Editors
{
    // YAN: At this moment, the ws data has already filled.
    public class InworldEditorSelectGameData : IEditorState
    {
        const string k_DefaultWorkspace = "--- SELECT WORKSPACE ---";
        const string k_DefaultScene = "--- SELECT SCENE ---";
        const string k_DefaultGraph = "--- SELECG GRAPH ---";
        const string k_DefaultKey = "--- SELECT KEY---";
        const string k_DataMissing = "Some data is missing.\nPlease make sure you have at least one scene and one key/secret in your workspace";
        
        string m_CurrentWorkspace = "--- SELECT WORKSPACE ---";
        string m_CurrentScene = "--- SELECT SCENE ---";
        string m_CurrentGraph = "--- SELECT GRAPH ---";
        string m_CurrentKey = "--- SELECT KEY---";
        
        bool m_DisplayDataMissing = false;
        bool m_StartDownload = false;
        SelectingDataType m_CurrentSelecting = SelectingDataType.None;
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
            _DrawToggles();
            _DrawDataDropDown();
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
            if (m_CurrentWorkspace != k_DefaultWorkspace && !string.IsNullOrEmpty(m_CurrentWorkspace))
            {
                if (GUILayout.Button("Refresh", InworldEditor.Instance.BtnStyle))
                {
                    _SelectWorkspace(m_CurrentWorkspace);
                }
            }
            if (m_CurrentSelecting == SelectingDataType.Graphs && !string.IsNullOrEmpty(m_CurrentGraph) && m_CurrentGraph != k_DefaultGraph)
            {
                if (GUILayout.Button("Open Graph", InworldEditor.Instance.BtnStyle))
                {
                    _LoadCurrentGraph();
                }
            }

            if (m_CurrentKey != k_DefaultKey && !string.IsNullOrEmpty(m_CurrentKey) && m_CurrentScene != k_DefaultScene && !string.IsNullOrEmpty(m_CurrentScene))
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Next", InworldEditor.Instance.BtnStyle))
                {
                    _SaveCurrentSettings();
                    _DownloadRelatedAssets();
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
            m_CurrentScene = k_DefaultScene;
            m_CurrentWorkspace = k_DefaultWorkspace;
            m_CurrentSelecting = SelectingDataType.None;
            if (InworldAI.User.Workspace.Count != 1)
                return;
            _SelectWorkspace(InworldAI.User.Workspace[0].displayName);
        }
        public void ProcessData(string sceneName)
        {
            m_CurrentScene = InworldAI.User.GetSceneByFullName(sceneName)?.displayName;
            _SaveCurrentSettings();
            _DownloadRelatedAssets();
        }
        InworldSceneData _GetCurrentScene()
        {
            InworldWorkspaceData ws = InworldAI.User.GetWorkspaceByDisplayName(m_CurrentWorkspace);
            return ws?.scenes.FirstOrDefault(scene => scene.displayName == m_CurrentScene);
        }
        /// <summary>
        /// Triggers when other general update logic has been finished.
        /// </summary>
        public void PostUpdate()
        {
            if (!m_StartDownload)
                return;
            InworldSceneData sceneData = _GetCurrentScene();
            if (sceneData == null)
            {
                return;
            }
            EditorUtility.DisplayProgressBar("Inworld", "Downloading Assets", sceneData.Progress);
            if (sceneData.Progress > 0.95f)
            {
                InworldEditor.Instance.Status = EditorStatus.SelectCharacter;
            }
        }

        string CurrentGraph
        {
            get => m_CurrentGraph;
            set
            {
                if (m_CurrentGraph == value)
                    return;
                m_CurrentGraph = value;
                _LoadCurrentGraph();
            }
        }
        void _LoadCurrentGraph()
        {
            InworldWorkspaceData ws = InworldAI.User.GetWorkspaceByDisplayName(m_CurrentWorkspace);
            if (ws == null)
                return;
            if (!string.IsNullOrEmpty(m_CurrentGraph) && m_CurrentGraph != k_DefaultGraph)
            {
                InworldGraphData graphData = ws.graphs.FirstOrDefault(graph => graph.displayName == m_CurrentGraph);
                if (graphData == null)
                {
                    Debug.LogError($"Graph NULL with {m_CurrentGraph} {ws.graphs.Count}!");
                }
                InworldGraph.OpenGraphWindow(m_CurrentGraph, graphData);
            }
        }
        void _SaveCurrentSettings()
        {
            InworldGameData gameData = _CreateGameDataAssets();
            InworldController controller = Object.FindObjectOfType<InworldController>();
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
            InworldSceneData sceneData = _GetCurrentScene();
            if (sceneData == null)
                return;
            // Download Thumbnails and put under User name's folder.
            m_StartDownload = true;
            foreach (CharacterReference charRef in sceneData.characterReferences)
            {
                if (charRef.characterOverloads.Count != 1)
                    continue;
                
                string thumbURL = charRef.characterOverloads[0].defaultCharacterAssets.ThumbnailURL;
                string thumbFileName = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.ThumbnailPath}/{charRef.CharacterFileName}.png";
                string modelURL = charRef.characterOverloads[0].defaultCharacterAssets.rpmModelUri;
                string modelFileName = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.AvatarPath}/{charRef.CharacterFileName}.glb";
                if (File.Exists(thumbFileName))
                    charRef.characterOverloads[0].defaultCharacterAssets.thumbnailProgress = 1;
                else if (!string.IsNullOrEmpty(thumbURL))
                    InworldEditorUtil.DownloadCharacterAsset(charRef.character, thumbURL, _OnCharThumbnailDownloaded);
                if (File.Exists(modelFileName))
                    charRef.characterOverloads[0].defaultCharacterAssets.avatarProgress = 1;
                else if (!string.IsNullOrEmpty(modelURL))
                    InworldEditorUtil.DownloadCharacterAsset(charRef.character, modelURL, _OnCharModelDownloaded);
            }
            // Meanwhile, showcasing progress bar.
        }

        InworldGameData _CreateGameDataAssets()
        {
            // Create a new SO.
            InworldGameData gameData = ScriptableObject.CreateInstance<InworldGameData>();
            InworldWorkspaceData ws = InworldAI.User.GetWorkspaceByDisplayName(m_CurrentWorkspace);
            if (ws != null)
            {
                InworldSceneData sceneData = ws.scenes.FirstOrDefault(scene => scene.displayName == m_CurrentScene);
                InworldKeySecret keySecret = ws.keySecrets.FirstOrDefault(key => key.key == m_CurrentKey);
                gameData.SetData(ws.name, sceneData, keySecret);
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
            string newAssetPath = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.GameDataPath}/{gameData.GameDataFileName}.asset";
            AssetDatabase.CreateAsset(gameData, newAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return gameData;
        }
        void _DrawWorkspaceDropDown()
        {
            EditorGUILayout.LabelField("Choose Workspace:", InworldEditor.Instance.TitleStyle);
            List<string> wsList = InworldAI.User.Workspace.Select(ws => ws.displayName).ToList();
            InworldEditorUtil.DrawDropDown(m_CurrentWorkspace, wsList, _SelectWorkspace);
        }
        void _DrawSceneDropDown()
        {
            if (m_CurrentWorkspace == k_DefaultWorkspace || string.IsNullOrEmpty(m_CurrentWorkspace))
                return;
            InworldWorkspaceData ws = InworldAI.User.GetWorkspaceByDisplayName(m_CurrentWorkspace);
            if (ws == null)
                return;
            List<string> sceneList = ws.scenes.Select(scene => scene.displayName).ToList();
            EditorGUILayout.LabelField("Choose Scene:", InworldEditor.Instance.TitleStyle);
            InworldEditorUtil.DrawDropDown(m_CurrentScene, sceneList, _SelectScenes);
            m_CurrentScene = sceneList.Count == 1 ? sceneList[0] : m_CurrentScene;
        }
        void _DrawGraphDropDown()
        {
            if (m_CurrentWorkspace == k_DefaultWorkspace || string.IsNullOrEmpty(m_CurrentWorkspace))
                return;
            InworldWorkspaceData ws = InworldAI.User.GetWorkspaceByDisplayName(m_CurrentWorkspace);
            if (ws == null)
                return;
            List<string> graphList = ws.graphs.Select(graph => graph.displayName).ToList();
            if (graphList.Count == 0)
                return;
            CurrentGraph = graphList.Count == 1 ? graphList[0] : m_CurrentGraph;;
        }

        void _DrawToggles()
        {
            if (m_CurrentWorkspace == k_DefaultWorkspace || string.IsNullOrEmpty(m_CurrentWorkspace))
                return;
            if (m_CurrentKey == k_DefaultKey || string.IsNullOrEmpty(m_CurrentKey))
                return;
            InworldWorkspaceData ws = InworldAI.User.GetWorkspaceByDisplayName(m_CurrentWorkspace);
            if (ws == null)
                return;
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Choose Data Type:", InworldEditor.Instance.TitleStyle);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Scenes"))
            {
                m_CurrentSelecting = SelectingDataType.Scenes;
            }
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Graphs"))
            {
                m_CurrentSelecting = SelectingDataType.Graphs;
            }
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Characters"))
            {
                m_CurrentSelecting = SelectingDataType.Characters;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndChangeCheck();
        }
        void _DrawDataDropDown()
        {
            EditorGUILayout.Space(10);
            switch (m_CurrentSelecting)
            {
                case SelectingDataType.None:
                    break;
                case SelectingDataType.Characters:
                    _SaveCurrentSettings();
                    InworldEditor.Instance.Status = EditorStatus.SelectCharacter;
                    break;
                case SelectingDataType.Scenes:
                    _DrawSceneDropDown();
                    break;
                case SelectingDataType.Graphs:
                    _DrawGraphDropDown();
                    break;
            }
        }
        void _DrawKeyDropDown()
        {
            if (m_CurrentWorkspace == k_DefaultWorkspace || string.IsNullOrEmpty(m_CurrentWorkspace))
                return;
            InworldWorkspaceData ws = InworldAI.User.GetWorkspaceByDisplayName(m_CurrentWorkspace);
            if (ws == null)
                return;
            List<string> keyList = ws.keySecrets.Where(key => key.state == "ACTIVE").Select(key => key.key).ToList();
            EditorGUILayout.LabelField("Choose API Key:", InworldEditor.Instance.TitleStyle);
            InworldEditorUtil.DrawDropDown(m_CurrentKey, keyList, _SelectKeys);
            m_CurrentKey = keyList.Count == 1 ? keyList[0] : m_CurrentKey;
        }
        void _ListCharacters()
        {
            string wsFullName = InworldAI.User.GetWorkspaceFullName(m_CurrentWorkspace);
            if (string.IsNullOrEmpty(wsFullName))
                return;
            InworldEditorUtil.SendWebGetRequest(InworldEditor.ListCharactersURL(wsFullName), true, _ListCharacterCompleted);
        }
        void _ListKeys()
        {
            string wsFullName = InworldAI.User.GetWorkspaceFullName(m_CurrentWorkspace);
            if (string.IsNullOrEmpty(wsFullName))
                return;
            InworldEditorUtil.SendWebGetRequest(InworldEditor.ListKeyURL(wsFullName), true, _ListKeyCompleted);
        }
        void _ListCommonKnowledges()
        {
            string wsFullName = InworldAI.User.GetWorkspaceFullName(m_CurrentWorkspace);
            if (string.IsNullOrEmpty(wsFullName))
                return;
            InworldEditorUtil.SendWebGetRequest(InworldEditor.ListCommonKnowledgeURL(wsFullName), true, _ListCommonKnowledgeCompleted);
        }

        void _ListScenes()
        {
            string wsFullName = InworldAI.User.GetWorkspaceFullName(m_CurrentWorkspace);
            if (string.IsNullOrEmpty(wsFullName))
                return;
            InworldEditorUtil.SendWebGetRequest(InworldEditor.ListScenesURL(wsFullName), true, _ListSceneCompleted);
        }
        void _ListGraphs()
        {
            string wsFullName = InworldAI.User.GetWorkspaceFullName(m_CurrentWorkspace);
            if (string.IsNullOrEmpty(wsFullName))
                return;
            InworldEditorUtil.SendWebGetRequest(InworldEditor.ListGraphsURL(wsFullName), true, _ListGraphCompleted);
        }
        void _ListCharacterCompleted(AsyncOperation obj)
        {
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(obj);
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                InworldEditor.Instance.Error = $"List Characters Failed: {InworldEditor.GetError(uwr.error)}";
                EditorUtility.ClearProgressBar();
                return;
            }

            ListCharacterResponse resp = JsonUtility.FromJson<ListCharacterResponse>(uwr.downloadHandler.text);
            if (resp.characters.Count == 0)
            {
                m_DisplayDataMissing = true;
            }
            InworldWorkspaceData ws = InworldAI.User.GetWorkspaceByDisplayName(m_CurrentWorkspace);
            if (ws.characters == null)
                ws.characters = new List<InworldCharacterData>();
            ws.characters.Clear();
            ws.characters.AddRange(resp.ToCharacterData()); 
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
            
            ListSceneResponse resp = JsonUtility.FromJson<ListSceneResponse>(uwr.downloadHandler.text);
            InworldWorkspaceData ws = InworldAI.User.GetWorkspaceByDisplayName(m_CurrentWorkspace);
            if (ws.scenes == null)
                ws.scenes = new List<InworldSceneData>();
            ws.scenes.Clear();
            ws.scenes.AddRange(resp.scenes); 
        }
        void _ListGraphCompleted(AsyncOperation obj)
        {
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(obj);
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                InworldEditor.Instance.Error = $"List Graph Failed: {InworldEditor.GetError(uwr.error)}";
                EditorUtility.ClearProgressBar();
                return;
            }
            ListGraphResponse resp = JsonUtility.FromJson<ListGraphResponse>(uwr.downloadHandler.text);
            File.WriteAllText("aaa.json", uwr.downloadHandler.text);
            Debug.Log("YAN: Write to json completed!");
            // YAN: Add default name for display as currently it's missing
            foreach (InworldGraphData graph in resp.graphs.Where(graph => string.IsNullOrEmpty(graph.displayName)))
            {
                graph.displayName = graph.name.Split('/')[^1];
            }
            InworldWorkspaceData ws = InworldAI.User.GetWorkspaceByDisplayName(m_CurrentWorkspace);
            if (ws.graphs == null)
                ws.graphs = new List<InworldGraphData>();
            ws.graphs.Clear();
            ws.graphs.AddRange(resp.graphs);
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
            InworldWorkspaceData ws = InworldAI.User.GetWorkspaceByDisplayName(m_CurrentWorkspace);
            if (ws.keySecrets == null)
                ws.keySecrets = new List<InworldKeySecret>();
            ws.keySecrets.Clear();
            ws.keySecrets.AddRange(resp.apiKeys); 
        }
        void _ListCommonKnowledgeCompleted(AsyncOperation obj)
        {
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(obj);
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                InworldEditor.Instance.Error = $"List Knowledge Failed: {InworldEditor.GetError(uwr.error)}";
                EditorUtility.ClearProgressBar();
                return;
            }
            ListCommonKnowledgeResponse resp = JsonUtility.FromJson<ListCommonKnowledgeResponse>(uwr.downloadHandler.text);
            if (resp.commonKnowledge.Count == 0)
                m_DisplayDataMissing = true;
            InworldWorkspaceData ws = InworldAI.User.GetWorkspaceByDisplayName(m_CurrentWorkspace);
            if (ws.commonKnowledges == null)
                ws.commonKnowledges = new List<InworldCommonKnowledge>();
            ws.commonKnowledges.Clear();
            ws.commonKnowledges.AddRange(resp.commonKnowledge); 
        }
        void _SelectWorkspace(string workspaceDisplayName)
        {
            m_CurrentWorkspace = workspaceDisplayName;
            m_CurrentScene = k_DefaultScene;
            m_CurrentKey = k_DefaultKey; // YAN: Reset data.
            m_DisplayDataMissing = false;
            m_StartDownload = false;
            _ListCharacters();
            _ListCommonKnowledges();
            _ListScenes();
            _ListGraphs();
            _ListKeys();
        }

        void _SelectGraph(string graphDisplayName) => m_CurrentGraph = graphDisplayName;
        void _SelectScenes(string sceneDisplayName) => m_CurrentScene = sceneDisplayName;
        void _SelectKeys(string keyDisplayName) => m_CurrentKey = keyDisplayName;
        // Download Avatars and put under User name's folder.
        void _OnCharModelDownloaded(string charFullName, AsyncOperation downloadContent)
        {
            CharacterReference charRef = _GetCurrentScene()?.characterReferences.FirstOrDefault(c => c.character == charFullName);
            if (charRef == null || charRef.characterOverloads.Count != 1)
                return;
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(downloadContent);
            
            if (string.IsNullOrEmpty(InworldEditorUtil.UserDataPath) || uwr.result != UnityWebRequest.Result.Success)
            {
                InworldAI.LogError($"Failed to download model: {charFullName} with {uwr.url}");
                charRef.characterOverloads[0].defaultCharacterAssets.avatarProgress = 1;
                return;
            }
            // YAN: Currently we only download .glb files.
            if (!Directory.Exists($"{InworldEditorUtil.UserDataPath}/{InworldEditor.AvatarPath}"))
            {
                Directory.CreateDirectory($"{InworldEditorUtil.UserDataPath}/{InworldEditor.AvatarPath}");
            }
            string newAssetPath = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.AvatarPath}/{charRef.CharacterFileName}.glb";
            File.WriteAllBytes(newAssetPath, uwr.downloadHandler.data);
            AssetDatabase.Refresh();
            charRef.characterOverloads[0].defaultCharacterAssets.avatarProgress = 1;
        }
        void _OnCharThumbnailDownloaded(string charFullName, AsyncOperation downloadContent)
        {
            CharacterReference charRef = _GetCurrentScene()?.characterReferences.FirstOrDefault(c => c.character == charFullName);
            if (charRef == null || charRef.characterOverloads.Count != 1)
                return;
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(downloadContent);
            
            if (string.IsNullOrEmpty(InworldEditorUtil.UserDataPath) || uwr.result != UnityWebRequest.Result.Success)
            {
                InworldAI.LogError($"Failed to download Thumbnail: {charFullName} with {uwr.url}");
                charRef.characterOverloads[0].defaultCharacterAssets.thumbnailProgress = 1;
                return;
            }
            if (!Directory.Exists($"{InworldEditorUtil.UserDataPath}/{InworldEditor.ThumbnailPath}"))
            {
                Directory.CreateDirectory($"{InworldEditorUtil.UserDataPath}/{InworldEditor.ThumbnailPath}");
            }
            string newAssetPath = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.ThumbnailPath}/{charRef.CharacterFileName}.png";
            File.WriteAllBytes(newAssetPath, uwr.downloadHandler.data);
            charRef.characterOverloads[0].defaultCharacterAssets.thumbnailProgress = 1;
        }
    }
}
#endif
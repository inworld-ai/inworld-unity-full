/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

using Inworld.Data;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Inworld.Editors
{
    public class InworldEditorSelectCharacter: IEditorState
    {
        const string k_DefaultScene = "All Characters";
        const string k_DataMissing = "All Characters are not supported in the 2D mode.\nPlease make sure you have selected at least one scene with characters";
        string m_CurrentSceneName = "All Characters";
        List<string> m_SceneNames;
        InworldWorkspaceData m_CurrentWorkspace;
        InworldGameData m_CurrentGameData;
        bool m_StartDownload = false;
        Vector2 m_ScrollPosition;

        InworldSceneData CurrentScene => m_CurrentWorkspace?.scenes.FirstOrDefault(scene => scene.displayName == m_CurrentSceneName);
        
        /// <summary>
        /// Triggers when open editor window.
        /// </summary>
        public void OnOpenWindow()
        {
            if (!InworldController.Instance || !InworldController.Instance.GameData)
            {
                InworldEditor.Instance.Status = EditorStatus.SelectGameData; // YAN: Fall back.
            }
            else
                _InitDataSelection();
        }
        /// <summary>
        /// Triggers when drawing the title of the editor panel page.
        /// </summary>
        public void DrawTitle()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Please select characters and drag to the scene.", InworldEditor.Instance.TitleStyle);
            EditorGUILayout.Space();
        }
        /// <summary>
        /// Triggers when drawing the content of the editor panel page.
        /// </summary>
        public void DrawContent()
        {
            if (m_CurrentWorkspace == null || !m_CurrentGameData)
                return;
            _DrawSceneSelectionDropDown();
            _DrawCharacterSelection();
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
                InworldEditor.Instance.Status = EditorStatus.SelectGameData;
            }
            if (GUILayout.Button("Refresh", InworldEditor.Instance.BtnStyle))
            {
                _DownloadRelatedAssets();
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
            m_StartDownload = false;
            EditorUtility.ClearProgressBar();
            _InitDataSelection();
        }
        /// <summary>
        /// Triggers when other general update logic has been finished.
        /// </summary>
        public void PostUpdate()
        {
            if (!m_StartDownload || !InworldController.Instance.GameData)
                return;
            InworldGameData gameData = InworldController.Instance.GameData;
            EditorUtility.DisplayProgressBar("Inworld", "Downloading Assets", gameData.Progress);
            if (gameData.Progress > 0.95f)
            {
                m_StartDownload = false;
                EditorUtility.ClearProgressBar();
                _CreatePrefabVariants();
            }
        }
        public void OnClose()
        {
            
        }
        void _InitDataSelection()
        {
            m_SceneNames = new List<string>
            {
                k_DefaultScene
            };
            if (!InworldController.Instance)
                return;
            m_CurrentGameData = InworldController.Instance.GameData;
            if (InworldAI.User && InworldAI.User.Workspace != null && InworldAI.User.Workspace.Count != 0)
                m_CurrentWorkspace = InworldAI.User.Workspace.FirstOrDefault(ws => ws.name == m_CurrentGameData.workspaceFullName);
            m_CurrentWorkspace?.scenes.ForEach(s => m_SceneNames.Add(s.displayName));
        }
        void _CreatePrefabVariants()
        {
            // 1. Get the character prefab for character in current workspace. (Default or Specific)
            if (m_CurrentWorkspace == null)
                return;
            foreach (InworldCharacterData charRef in m_CurrentWorkspace.characters)
            {
                GameObject downloadedModel = _GetModel(charRef);
                _CreateVariant(charRef, downloadedModel);
            }
            // 2. Save the prefab variant as the new data.
        }
        void _DrawSceneSelectionDropDown()
        {
            if (m_CurrentWorkspace == null)
                return;
            EditorGUILayout.LabelField("Choose Scenes:", InworldEditor.Instance.TitleStyle);
            InworldEditorUtil.DrawDropDown(m_CurrentSceneName, m_SceneNames, _SelectScenes);
            m_CurrentSceneName = m_SceneNames.Count == 1 ? m_SceneNames[0] : m_CurrentSceneName;
        }

        void _DrawCharacterSelection()
        {
            EditorGUILayout.LabelField("Choose GameMode:", InworldEditor.Instance.TitleStyle);
            GUILayout.BeginHorizontal();
            InworldEditor.Is3D = EditorGUILayout.Toggle("3D Game", InworldEditor.Is3D);
            InworldEditor.Is3D = !EditorGUILayout.Toggle("2D Game", !InworldEditor.Is3D);
            GUILayout.EndHorizontal();
            if (InworldEditor.Is3D)
            {
                // 1. Get the character prefab for character in current scene. (Default or Specific)
                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
                EditorGUILayout.BeginHorizontal();
                if (m_CurrentSceneName == k_DefaultScene)
                    _ListAllCharacters();
                else
                    _ListCharactersInScene(m_CurrentSceneName);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndScrollView();
            }
            else
            {
                if (m_CurrentSceneName == k_DefaultScene)
                    EditorGUILayout.LabelField(k_DataMissing, InworldEditor.Instance.TitleStyle);
                else
                    InworldController.Client.EnableGroupChat = false;
            }
            if (GUILayout.Button("Add PlayerController to Scene", GUILayout.ExpandWidth(true)))
            {
                InworldEditorUtil.AddPlayerController(InworldEditor.Is3D ? InworldEditor.PlayerController : InworldEditor.PlayerController2D);
            }
        }
        void _ListCharactersInScene(string currentSceneName)
        {
            InworldSceneData sceneData = m_CurrentWorkspace.scenes.FirstOrDefault(s => s.displayName == currentSceneName);
            if (sceneData == null)
                return;
            List<InworldCharacterData> charDataList = sceneData.GetCharacterDataByReference(m_CurrentWorkspace);
            foreach (InworldCharacterData charData in charDataList.Where
                (charData => GUILayout.Button(charData.description.givenName, InworldEditor.Instance.BtnCharStyle(_GetTexture2D(charData)))))
            {
                Selection.activeObject = _GetPrefab(charData);
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
        }
        void _ListAllCharacters()
        {
            foreach (InworldCharacterData charData in m_CurrentWorkspace.characters.Where
                (charData => GUILayout.Button(charData.description.givenName, InworldEditor.Instance.BtnCharStyle(_GetTexture2D(charData)))))
            {
                Selection.activeObject = _GetPrefab(charData);
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
        }
        void _SelectScenes(string sceneDisplayName)
        {
            m_CurrentSceneName = sceneDisplayName;
            m_CurrentGameData.sceneFullName = CurrentScene?.name ?? "";
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        void _DownloadRelatedAssets()
        {
            if (!InworldController.Instance || !m_CurrentGameData)
                return;
            Debug.Log("Download Related Assets...");
            InworldWorkspaceData wsData = InworldAI.User.Workspace.FirstOrDefault(ws => ws.name == m_CurrentGameData.workspaceFullName);
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
        GameObject _GetModel(InworldCharacterData charRef)
        {
            AssetDatabase.Refresh();
            string filePath = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.AvatarPath}/{charRef.CharacterFileName}.glb";
            return !File.Exists(filePath) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
        }
        Texture2D _GetTexture2D(InworldCharacterData charRef)
        {
            string filePath = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.ThumbnailPath}/{charRef.CharacterFileName}.png";
            if (!File.Exists(filePath))
                return InworldAI.DefaultThumbnail;
            byte[] imgBytes = File.ReadAllBytes(filePath);
            Texture2D loadedTexture = new Texture2D(0,0); 
            loadedTexture.LoadImage(imgBytes);
            return loadedTexture;
        }
        GameObject _GetPrefab(InworldCharacterData charRef)
        {
            string filePath = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.PrefabPath}/{charRef.CharacterFileName}.prefab";
            if (File.Exists(filePath))
                return AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
            InworldAI.LogError($"Cannot find {charRef.CharacterFileName}.prefab");
            return null;
        }
                // Download Avatars and put under User name's folder.
        void _OnCharModelDownloaded(string charFullName, AsyncOperation downloadContent)
        {
            InworldCharacterData charRef = InworldController.Instance.GameData.characters.FirstOrDefault(c => c.brainName == charFullName);
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
            AssetDatabase.Refresh();
            charRef.characterAssets.avatarProgress = 1;
        }
        void _OnCharThumbnailDownloaded(string charFullName, AsyncOperation downloadContent)
        {
            InworldCharacterData charRef = InworldController.Instance.GameData.characters.FirstOrDefault(c => c.brainName == charFullName);
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
            charRef.characterAssets.thumbnailProgress = 1;
        }
    }
}
#endif
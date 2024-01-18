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
using Inworld.Sample;
using Inworld.Entities;

namespace Inworld.Editors
{
    public class InworldEditorSelectCharacter: IEditorState
    {
        bool m_StartDownload = false;
        Vector2 m_ScrollPosition;
        
        /// <summary>
        /// Triggers when open editor window.
        /// </summary>
        public void OnOpenWindow()
        {
            if (!InworldController.Instance || !InworldController.Instance.GameData)
            {
                InworldEditor.Instance.Status = EditorStatus.SelectGameData; // YAN: Fall back.
            }
        }
        /// <summary>
        /// Triggers when drawing the title of the editor panel page.
        /// </summary>
        public void DrawTitle()
        {
            EditorGUILayout.Space();
            if (InworldEditor.Is3D)
                EditorGUILayout.LabelField("Please select characters and drag to the scene.", InworldEditor.Instance.TitleStyle);
            else
            {
                EditorGUILayout.LabelField("Done!", InworldEditor.Instance.TitleStyle);
                EditorGUILayout.LabelField("You can close the tab now.", InworldEditor.Instance.TitleStyle);
            }
            EditorGUILayout.Space();
        }
        /// <summary>
        /// Triggers when drawing the content of the editor panel page.
        /// </summary>
        public void DrawContent()
        {
            if (!InworldEditor.Is3D || !InworldController.Instance || !InworldController.Instance.GameData)
                return;
            // 1. Get the character prefab for character in current scene. (Default or Specific)
            InworldSceneData sceneData = InworldAI.User.GetSceneByFullName(InworldController.Instance.GameData.sceneFullName);
            if (sceneData == null)
                return;
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
            EditorGUILayout.BeginHorizontal();
            foreach (CharacterReference charRef in sceneData.characterReferences.Where(charRef => GUILayout.Button(charRef.characterOverloads[0].defaultCharacterDescription.givenName, InworldEditor.Instance.BtnCharStyle(_GetTexture2D(charRef)))))
            {
                Selection.activeObject = _GetPrefab(charRef);
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Add PlayerController to Scene", GUILayout.ExpandWidth(true)))
            {
                Camera mainCamera = Camera.main;
                if (mainCamera)
                {
                    if (EditorUtility.DisplayDialog("Note", "Adding player controller will delete current main camera. Continue?", "OK", "Cancel"))
                    {
                        Undo.DestroyObjectImmediate(mainCamera.gameObject);
                    }
                }
                if (!Object.FindObjectOfType<PlayerController>())
                    Object.Instantiate(InworldEditor.PlayerController);
            }
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
            _CreatePrefabVariants();
        }
        /// <summary>
        /// Triggers when other general update logic has been finished.
        /// </summary>
        public void PostUpdate()
        {
            if (!m_StartDownload || !InworldController.Instance.GameData)
                return;
            var sceneData = InworldController.Instance.GameData;
            EditorUtility.DisplayProgressBar("Inworld", "Downloading Assets", sceneData.Progress);
            if (sceneData.Progress > 0.95f)
            {
                m_StartDownload = false;
                EditorUtility.ClearProgressBar();
                _CreatePrefabVariants();
            }
        }
        void _CreatePrefabVariants()
        {
            // 1. Get the character prefab for character in current scene. (Default or Specific)
            InworldSceneData sceneData = InworldAI.User.GetSceneByFullName(InworldController.Instance.GameData.sceneFullName);
            if (sceneData == null)
                return;
            foreach (CharacterReference charRef in sceneData.characterReferences)
            {
                GameObject downloadedModel = _GetModel(charRef);
                _CreateVariant(charRef, downloadedModel);
            }
            // 2. Save the prefab variant as the new data.
        }
        void _DownloadRelatedAssets()
        {
            if (!InworldController.Instance.GameData)
                return;
            // Download Thumbnails and put under User name's folder.
            m_StartDownload = true;
            InworldGameData sceneData = InworldController.Instance.GameData;
            foreach (InworldCharacterData character in sceneData.characters)
            {
                string thumbURL = character.characterAssets.ThumbnailURL;
                string modelURL = character.characterAssets.rpmModelUri;
                if (!string.IsNullOrEmpty(thumbURL))
                    InworldEditorUtil.DownloadCharacterAsset(character.brainName, thumbURL, _OnCharThumbnailDownloaded);
                if (!string.IsNullOrEmpty(modelURL))
                    InworldEditorUtil.DownloadCharacterAsset(character.brainName, modelURL, _OnCharModelDownloaded);
            }
            // Meanwhile, showcasing progress bar.
        }
        static void _CreateVariant(CharacterReference charRef, GameObject customModel)
        { 
            // Use Current Model
            InworldCharacter avatar = customModel ?
                Object.Instantiate(InworldEditor.Instance.RPMPrefab) :
                Object.Instantiate(InworldEditor.Instance.InnequinPrefab);

            InworldCharacter iwChar = avatar.GetComponent<InworldCharacter>();
            iwChar.Data = new InworldCharacterData(charRef);
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
        GameObject _GetModel(CharacterReference charRef)
        {
            AssetDatabase.Refresh();
            string filePath = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.AvatarPath}/{charRef.CharacterFileName}.glb";
            return !File.Exists(filePath) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
        }
        Texture2D _GetTexture2D(CharacterReference charRef)
        {
            string filePath = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.ThumbnailPath}/{charRef.CharacterFileName}.png";
            if (!File.Exists(filePath))
                return InworldAI.DefaultThumbnail;
            byte[] imgBytes = File.ReadAllBytes(filePath);
            Texture2D loadedTexture = new Texture2D(0,0); 
            loadedTexture.LoadImage(imgBytes);
            return loadedTexture;
        }
        GameObject _GetPrefab(CharacterReference charRef)
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
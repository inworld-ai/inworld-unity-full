/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using Inworld.Editor.States;
using Inworld.Studio;
using Inworld.Util;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Inworld.Editor
{
    /// <summary>
    ///     Inworld Editor has 3 parts.
    ///     This part is for file supports.
    ///     One otherparts are for UI rendering, and Connecting Inworld Studio Server, and process data send or receive.
    /// </summary>
    public partial class InworldEditor : EditorWindow
    {
        #region Private Variables.
        const string k_ResourcePath = "Assets/Inworld.AI/Resources";
        readonly Dictionary<InworldEditorStatus, EditorState> m_States = new Dictionary<InworldEditorStatus, EditorState>();
        readonly Dictionary<InworldWorkspaceData, WorkspaceFetchingProgress> m_Progresses = new Dictionary<InworldWorkspaceData, WorkspaceFetchingProgress>();
        #endregion

        #region Private Properties Functions
        static internal string TokenForExchange { get; set; }
        static internal float SecToReconnect { get; set; } = 0.5f;
        static internal float CurrentCountDown { get; set; }
        static internal bool IsTokenValid
        {
            get
            {
                if (string.IsNullOrEmpty(TokenForExchange))
                    return false;
                string[] columns = TokenForExchange.Split(':');
                if (columns.Length < 2)
                    return false;
                InworldAI.Log($"IDToken: {columns[0]}");
                InworldAI.Log($"RefreshToken: {columns[1]}");
                InworldAI.User.RefreshTokens(columns[0], columns[1]);
                return true;
            }
        }
        internal void Init()
        {
            m_Studio = new InworldStudio(this);
            if (m_States.Count != 0)
                return;
            _InitStates();
        }
        void _DrawBanner()
        {
            rootVisualElement.Add(InworldAI.UI.Header.Instantiate());
        }
        static internal void _SaveCurrentSettings()
        {
            _SaveWSData(InworldAI.Game.currentWorkspace);
            foreach (string brain in InworldAI.Game.currentScene.characters.Where(brain => InworldAI.User.Characters.ContainsKey(brain)))
            {
                _SaveCharData(InworldAI.User.Characters[brain]);
            }
            _SaveKey(InworldAI.Game.currentKey);
            _SaveIwSceneData(InworldAI.Game.currentScene);
        }
        static void _SaveWSData(InworldWorkspaceData wsToSave)
        {
            string fileLocation = _GetFileLocation(InworldAI.Settings.WorkspaceDataPath);
            if (wsToSave.index == -1)
            {
                InworldWorkspaceData[] objects = Resources.LoadAll<InworldWorkspaceData>($"{InworldAI.User.Name}/{InworldAI.Settings.WorkspaceDataPath}");
                InworldWorkspaceData existWS = objects.FirstOrDefault(ws => ws.fullName == wsToSave.fullName);
                if (!existWS) // Need to save a new one.
                {
                    int nIndex = objects.Count(ws => ws.title == wsToSave.title);
                    string path = nIndex == 0 ? $"{fileLocation}/{wsToSave.title}.asset" : $"{fileLocation}/{wsToSave.title}_{nIndex}.asset";
                    wsToSave.index = nIndex;
                    AssetDatabase.CreateAsset(wsToSave, path);
                }
                else
                {
                    wsToSave.index = existWS.index;
                }
            }
            EditorUtility.SetDirty(wsToSave);
            AssetDatabase.SaveAssets();
        }
        static void _SaveCharData(InworldCharacterData charToSave)
        {
            string fileLocation = _GetFileLocation(InworldAI.Settings.CharacterDataPath);
            if (charToSave.index == -1)
            {
                InworldCharacterData[] objects = Resources.LoadAll<InworldCharacterData>($"{InworldAI.User.Name}/{InworldAI.Settings.CharacterDataPath}");
                InworldCharacterData existChar = objects.FirstOrDefault(charData => charData.brain == charToSave.brain);
                if (!existChar) // Need to save a new one.
                {
                    int nIndex = objects.Count(charData => charData.characterName == charToSave.characterName);
                    string path = nIndex == 0 ?
                        $"{fileLocation}/{charToSave.characterName}.asset" :
                        $"{fileLocation}/{charToSave.characterName}_{nIndex}.asset";
                    charToSave.index = nIndex;
                    AssetDatabase.CreateAsset(charToSave, path);
                }
                else
                {
                    charToSave.index = existChar.index;
                    existChar.CopyFrom(charToSave);
                }
            }
            EditorUtility.SetDirty(charToSave);
            AssetDatabase.SaveAssets();
        }
        static void _SaveIwSceneData(InworldSceneData sceneToSave)
        {
            string fileLocation = _GetFileLocation(InworldAI.Settings.InworldSceneDataPath);
            if (sceneToSave.index == -1)
            {
                InworldSceneData[] objects = Resources.LoadAll<InworldSceneData>($"{InworldAI.User.Name}/{InworldAI.Settings.InworldSceneDataPath}");
                InworldSceneData existScene = objects.FirstOrDefault(scene => scene.fullName == sceneToSave.fullName);
                if (!existScene) // Need to find a new one.
                {
                    int nIndex = objects.Count(scene => scene.ShortName == sceneToSave.ShortName);
                    string path = nIndex == 0 ?
                        $"{fileLocation}/{sceneToSave.ShortName}.asset" :
                        $"{fileLocation}/{sceneToSave.ShortName}_{nIndex}.asset";
                    sceneToSave.index = nIndex;
                    AssetDatabase.CreateAsset(sceneToSave, path);
                }
                else
                {
                    sceneToSave.index = existScene.index;
                }
            }
            EditorUtility.SetDirty(sceneToSave);
            AssetDatabase.SaveAssets();
        }
        static void _SaveKey(InworldKeySecret asset)
        {
            string fileLocation = _GetFileLocation(InworldAI.Settings.KeySecretDataPath);
            string oldPath = AssetDatabase.GetAssetPath(asset);

            if (string.IsNullOrEmpty(oldPath))
            {
                string path = $"{fileLocation}/{asset.ShortName}.asset";
                if (!AssetDatabase.Contains(asset))
                    AssetDatabase.CreateAsset(asset, path);
            }
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }
        InworldCharacterData _GetCharData(string mrID, out int index)
        {
            InworldCharacterData[] results = Resources.LoadAll<InworldCharacterData>($"{InworldAI.User.Name}/{InworldAI.Settings.CharacterDataPath}");
            foreach (InworldCharacterData characterData in results)
            {
                if (characterData.brain != mrID)
                    continue;
                index = characterData.index;
                return characterData;
            }
            index = -1;
            return null;
        }
        InworldSceneData _GetIwSceneData(string mrID, out int index)
        {
            InworldSceneData[] results = Resources.LoadAll<InworldSceneData>($"{InworldAI.User.Name}/{InworldAI.Settings.InworldSceneDataPath}");
            foreach (InworldSceneData sceneData in results)
            {
                if (sceneData.fullName != mrID)
                    continue;
                index = sceneData.index;
                return sceneData;
            }
            index = -1;
            return null;
        }
        InworldWorkspaceData _GetWSData(string mrID, out int index)
        {
            InworldWorkspaceData[] results = Resources.LoadAll<InworldWorkspaceData>($"{InworldAI.User.Name}/{InworldAI.Settings.WorkspaceDataPath}"); 
            foreach (InworldWorkspaceData wsData in results)
            {
                if (wsData.fullName != mrID)
                    continue;
                index = wsData.index;
                return wsData;
            }
            index = -1;
            return null;
        }
        InworldKeySecret _GetKey(string dataPath, string assetName)
        {
            string fileLocation = _GetFileLocation(dataPath);

            InworldKeySecret result = AssetDatabase.LoadAssetAtPath<InworldKeySecret>($"{fileLocation}/{assetName}.asset");
            if (result)
                return result;
            string defaultFileLocation = _GetDefaultFileLocation(dataPath);
            result = AssetDatabase.LoadAssetAtPath<InworldKeySecret>($"{defaultFileLocation}/{assetName}.asset");
            return result; // Yan: Nullable.
        }
        string _GetDefaultFileLocation(string dataPath)
        {
            string fileLocation = $"{k_ResourcePath}/{dataPath}";
            if (!AssetDatabase.IsValidFolder($"{k_ResourcePath}/{dataPath}"))
                AssetDatabase.CreateFolder(k_ResourcePath, dataPath);
            return fileLocation;
        }
        static string _GetFileLocation(string dataPath)
        {
            string fileLocation = $"{k_ResourcePath}/{InworldAI.User.Name}/{dataPath}";
            if (!AssetDatabase.IsValidFolder($"{k_ResourcePath}/{InworldAI.User.Name}"))
                AssetDatabase.CreateFolder(k_ResourcePath, InworldAI.User.Name);
            if (!AssetDatabase.IsValidFolder(fileLocation))
                AssetDatabase.CreateFolder($"{k_ResourcePath}/{InworldAI.User.Name}", dataPath);
            return fileLocation;
        }
        void LoadDefaultAvatar(InworldCharacterData charData)
        {
            const string defaultName = "Inworld.AI/Resources/Default";
            if (!File.Exists($"{Application.dataPath}/{defaultName}"))
            {
                InworldAI.LogError($"Cannot find Default Assets! Please check {defaultName} exists.");
                return;
            }
            AssetDatabase.CopyAsset($"Assets/{defaultName}", charData.LocalAvatarFileName);
        }
        void LoadDefaultThumbnail(InworldCharacterData charData)
        {
            string assetPath = AssetDatabase.GetAssetPath(InworldAI.Settings.DefaultThumbnail);
            AssetDatabase.CopyAsset(assetPath, charData.LocalThumbnailFileName);
        }
        static void _LoadController()
        {
            InworldController controller = FindObjectOfType<InworldController>();
            if (!controller)
                controller = PrefabUtility.InstantiatePrefab(InworldAI.ControllerPrefab) as InworldController;
            if (!controller)
                return;
            Inworld.Runtime.InitInworld initInworld = controller.GetComponent<Inworld.Runtime.InitInworld>();
            if (initInworld)
            {
                initInworld.EditorLoadData();
            }
            controller.transform.position = Vector3.zero;
        }
        static void _SaveData()
        {
            if (InworldController.Instance)
            {
                foreach (InworldCharacter iwChar in InworldController.Instance.GetComponentsInChildren<InworldCharacter>())
                {
                    iwChar.Data.EditorSaveData();
                }
                EditorUtility.SetDirty(InworldController.Instance);
                Runtime.InitInworld initInworld = InworldController.Instance.GetComponent<Inworld.Runtime.InitInworld>();
                if (initInworld)
                    EditorUtility.SetDirty(initInworld);
            }
            EditorUtility.SetDirty(InworldAI.User);
            EditorUtility.SetDirty(InworldAI.Game);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        static internal void SetupInworldController()
        {
            if (!IsDataValid)
                return;
            if (Status != InworldEditorStatus.Default)
                _SaveCurrentSettings();
            _LoadController();
            _SaveData();
        }
        #endregion

        #region Properties
        /// <summary>
        ///     Get Instance of the InworldEditor.
        ///     It'll create a Inworld Studio Panel if the panel hasn't opened.
        /// </summary>
        public static InworldEditor Instance => GetWindow<InworldEditor>("Inworld Studio");

        /// <summary>
        ///     Get the current data fetching progress of all the workspaces.
        /// </summary>
        public static Dictionary<InworldWorkspaceData, WorkspaceFetchingProgress> Progress => Instance.m_Progresses;
        /// <summary>
        ///     Get the Current Workspace's data fetching progress.
        /// </summary>
        public static float CurrentProgress { get; private set; }

        /// <summary>
        ///     Check if all the Data in the current data in the Inworld Game Setting is true.
        /// </summary>
        public static bool IsDataValid
        {
            get
            {
                if (!InworldAI.Game.currentWorkspace)
                    return false;
                if (!InworldAI.Game.currentKey)
                    return false;
                if (!InworldAI.Game.currentScene)
                    return false;
                if (!InworldAI.Game.currentWorkspace.scenes.FirstOrDefault(sceneData => sceneData.fullName == InworldAI.Game.currentScene.fullName))
                    return false;
                if (!InworldAI.Game.currentWorkspace.integrations.FirstOrDefault(keySecret => keySecret.key == InworldAI.Game.currentKey.key))
                    return false;
                return InworldAI.Game.currentScene.characters.Count > 0;
            }
        }
        #endregion

        #region Function
        /// <summary>
        ///     Open Inworld Studio Panel
        ///     It will detect and pop import window if you dont have TMP imported.
        /// </summary>
        public void ShowPanel()
        {
            if (!File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"))
                TMP_PackageUtilities.ImportProjectResourcesMenu();
            titleContent = new GUIContent("Inworld Studio");
            minSize = InworldAI.UI.MinSize;
            Show();
        }
        /// <summary>
        ///     Set a gameobject in your scene with InworldCharacterData.
        ///     If the InworldCharacterData belongs to InworldAI.Game.currentScene,
        ///     the data and its related component (animation, player detecting, etc) would be added to the gameobject.
        ///     All the characters that has Inworld Character Data but not in the current Inworld Scene would be deleted.
        /// </summary>
        /// <param name="avatar">That gameObject you want to bind InworldCharacter</param>
        /// <param name="selectedCharacter">
        ///     The InworldCharacterData to add to the gameobject.
        ///     NOTE: That InworldCharacterData should be in InworldAI.Game.CurrentScene.
        /// </param>
        public static void SetupInworldCharacter(GameObject avatar, InworldCharacterData selectedCharacter)
        {
            SetupInworldController();

            InworldCharacter character = PrefabUtility.InstantiatePrefab(InworldAI.CharacterPrefab, avatar.transform) as InworldCharacter;
            if (character)
            {
                character.transform.SetParent(InworldController.Instance.transform);
                character.LoadCharacter(selectedCharacter, avatar);
                character.transform.name = selectedCharacter.characterName;
            }
            InworldCharacter[] charList = InworldController.Instance.transform.GetComponentsInChildren<InworldCharacter>();
            foreach (InworldCharacter iwChar in charList)
            {
                if (!InworldController.CurrentScene.characters.Contains(iwChar.BrainName))
                    DestroyImmediate(iwChar.gameObject);
            }
            _SaveData();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        /// <summary>
        ///     Add a Player controller into the current scene.
        ///     NOTE:
        ///     1. Player controller is mandatory for the Inworld Characters to communicate.
        ///     2. If you call this function, the Main Camera in your current scene would be deleted.
        ///     If you have your own player control, to let Inworld Characters to communicate,
        ///     Please the `InworldController.Player` to your customized controller object.
        /// </summary>
        public void LoadPlayerController()
        {
            _LoadController();
            GameObject mainCamera;
            if (Camera.main)
            {
                mainCamera = Camera.main.gameObject;
                DestroyImmediate(mainCamera);
            }
            mainCamera = PrefabUtility.InstantiatePrefab(InworldAI.PlayerControllerPrefab) as GameObject;
            InworldController.Player = mainCamera;
            EditorUtility.SetDirty(InworldController.Instance);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        #endregion
    }
}
#endif

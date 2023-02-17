/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/

#if UNITY_EDITOR
using Inworld.Model.Sample;
using Inworld.Util;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Linq;
using UnityEngine;
#if UNITY_IPHONE
using System.IO;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
#endif

namespace Inworld.Editor
{
    /// <summary>
    ///     This class would be called when package is imported, or Unity Editor is opened.
    /// </summary>
    [InitializeOnLoad]
    public class InitInworld : IPreprocessBuildWithReport
    {
        static InitInworld()
        {
            AssetDatabase.importPackageCompleted += packName =>
            {
                string userName = CloudProjectSettings.userName;
                InworldAI.User.OrganizationID = CloudProjectSettings.organizationId;
                InworldAI.User.Name = !string.IsNullOrEmpty(userName) && userName.Split('@').Length > 1 ? userName.Split('@')[0] : userName;
                _AddDebugMacro();
            };
            EditorApplication.playModeStateChanged += _LogPlayModeState;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.wantsToQuit += _ClearStatus;
            SceneView.duringSceneGui += OnSceneGUIChanged;
        }
        public int callbackOrder { get; }

        #region Call backs
        static void OnSceneGUIChanged(SceneView view)
        {
            if (InworldAI.IsDebugMode)
                _DrawGizmos();
            if (Event.current.type != EventType.DragExited)
                return;
            if (_WillSetupInworldCharacter)
                _SetupInworldCharacter(Selection.activeGameObject);
        }
        static void OnHierarchyChanged()
        {
            if (_WillSetupInworldCharacter)
                _SetupInworldCharacter(Selection.activeGameObject);
        }

        // YAN: Inworld Log will not be displayed in release,
        //      unless “Development Build”, or "Is Verbose Log" is checked.
        public void OnPreprocessBuild(BuildReport report)
        {
            if (Debug.isDebugBuild || InworldAI.IsDebugMode)
                return;
            _RemoveDebugMacro();
        }
        #if UNITY_IPHONE
        /// <summary>
        /// Handle libgrpc project settings.
        /// For the details. Please visit https://github.com/Cysharp/MagicOnion#ios-build-with-grpc
        /// </summary>
        /// <param name="target"></param>
        /// <param name="path"></param>
        [PostProcessBuild(1)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            string projectPath = PBXProject.GetPBXProjectPath(path);
            PBXProject project = new PBXProject();
            project.ReadFromString(File.ReadAllText(projectPath));
            string targetGuid = project.GetUnityFrameworkTargetGuid(); 

            // libz.tbd for grpc ios build
            project.AddFrameworkToProject(targetGuid, "libz.tbd", false);

            // libgrpc_csharp_ext missing bitcode. as BITCODE exand binary size to 250MB.
            project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");

            File.WriteAllText(projectPath, project.WriteToString());
        }
        #endif
        #endregion

        #region Private Properties & Functions
        static bool _WillSetupInworldCharacter
        {
            get
            {
                if (!InworldAI.Settings.AutoGenerateCharacter)
                    return false;
                if (EditorUtility.IsPersistent(Selection.activeGameObject))
                    return false;
                if (!InworldAI.Game.currentScene)
                    return false;
                GameObject avatar = Selection.activeGameObject;
                if (!avatar)
                    return false;
                return !avatar.GetComponent<InworldCharacter>();
            }
        }
        static void _AddDebugMacro()
        {
            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            string strSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if (!strSymbols.Contains("INWORLD_DEBUG"))
                strSymbols = string.IsNullOrEmpty(strSymbols) ? "INWORLD_DEBUG" : strSymbols + ";INWORLD_DEBUG";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, strSymbols);
        }
        static void _RemoveDebugMacro()
        {
            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            string strSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            strSymbols = strSymbols.Replace(";INWORLD_DEBUG", "").Replace("INWORLD_DEBUG", "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, strSymbols);
        }
        static void _DrawGizmos()
        {
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(20, 20, 350, 80));
            GUIStyle gizmosStyle = new GUIStyle
            {
                fontSize = 10
            };
            if (InworldAI.Game.currentScene)
            {
                GUILayout.Label($"Current InworldScene: <size=15><color=red><b>{InworldAI.Game.currentScene.ShortName}</b></color></size>", gizmosStyle);
                GUILayout.Label($"If you drag any InworldCharacters that are not in\n<b>{InworldAI.Game.currentScene.ShortName}</b>, they will be deleted!", gizmosStyle);
            }
            else
            {
                GUILayout.Label("No InworldScene has found. Please set in InworldStudio Panel", gizmosStyle);
            }
            GUILayout.EndArea();
            Handles.EndGUI();
        }
        static void _SetupInworldCharacter(GameObject avatar)
        {
            InworldCharacterData selectedCharacter = InworldAI.User.Characters.Values.FirstOrDefault
                (charData => charData.FileName == avatar.transform.name);
            if (selectedCharacter)
                InworldEditor.SetupInworldCharacter(avatar, selectedCharacter);
            else if (avatar.transform.name == "Default" && InworldAI.Game.currentCharacter != null)
                InworldEditor.SetupInworldCharacter(avatar, InworldAI.Game.currentCharacter);
            else
            {
                InworldCharacterData[] charList = Resources.LoadAll<InworldCharacterData>("Characters");
                selectedCharacter = charList.FirstOrDefault(charData => charData.avatar.transform.name == avatar.transform.name);
                if (selectedCharacter)
                    InworldEditor.SetupInworldCharacter(avatar, selectedCharacter);
            }

        }
        static void _LogPlayModeState(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    if (EditorWindow.HasOpenInstances<InworldEditor>())
                    {
                        PlayerPrefs.SetInt("OPEN_INWORLD_STUDIO", 1);
                    }
                    if (InworldAI.IsDebugMode)
                        _AddDebugMacro();
                    else
                        _RemoveDebugMacro();
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    if (PlayerPrefs.GetInt("OPEN_INWORLD_STUDIO") == 1)
                    {
                        PlayerPrefs.SetInt("OPEN_INWORLD_STUDIO", 0);
                        InworldEditor.Instance.Init();
                    }
                    _AddDebugMacro();
                    break;
            }
        }
        static bool _ClearStatus()
        {
            InworldEditor.Instance.Close();
            return true;
        }
        #endregion
    }
    /// <summary>
    ///     This class defines Editor integrations.
    ///     Such as Top Menu, Right click menu, Editor > Project Settings, Preference, etc.
    /// </summary>
    public static class InworldAISettingsProvider
    {
        /// <summary>
        ///     For the menu in "Edit > Project Settings > Inworld.AI"
        /// </summary>
        [SettingsProvider]
        static SettingsProvider CreateInworldProjectSettingsProvider()
        {
            return new SettingsProvider("Project/Inworld.AI", SettingsScope.Project)
            {
                guiHandler = searchContext =>
                {
                    UnityEditor.Editor.CreateEditor(InworldAI.User).OnInspectorGUI();
                }
            };
        }
        /// <summary>
        ///     For the menu in "Edit > Preference > Inworld.AI"
        /// </summary>
        [SettingsProvider]
        static SettingsProvider CreateInworldUserSettingsProvider()
        {
            return new SettingsProvider("Preferences/Inworld.AI", SettingsScope.User)
            {
                guiHandler = searchContext =>
                {
                    UnityEditor.Editor.CreateEditor(InworldAI.Settings).OnInspectorGUI();
                }
            };
        }
        /// <summary>
        ///     For the options on top menu bar "Inworld".
        /// </summary>

        #region Top Menu
        [MenuItem("Inworld/Studio Panel", false, 0)]
        static void TopMenuConnectStudio() => InworldEditor.Instance.ShowPanel();

        [MenuItem("Inworld/Global Settings", false, 1)]
        static void TopMenuShowPanel() => Selection.SetActiveObjectWithContext(InworldAI.Instance, InworldAI.Instance);
        
        [MenuItem("Inworld/Change User Name", false, 1)]
        static void TopMenuUserPanel() => Selection.SetActiveObjectWithContext(InworldAI.User, InworldAI.User);
        #endregion

        /// <summary>
        ///     For right click the project window.
        /// </summary>

        #region Asset Menu
        [MenuItem("Assets/Inworld Studio Panel", false, 0)]
        static void ConnectStudio() => InworldEditor.Instance.ShowPanel();

        [MenuItem("Assets/Inworld Settings", false, 1)]
        static void ShowPanel() => Selection.SetActiveObjectWithContext(InworldAI.Instance, InworldAI.Instance);

        #endregion

        #region Scene Menu
        [MenuItem("GameObject/Inworld/Add/Inworld Controller", false, 2)]
        static void AddInworldController()
        {
            if (InworldController.Instance)
                return;
            Object.Instantiate(InworldAI.ControllerPrefab.gameObject);
            InworldController.Instance.transform.name = "Inworld Controller";
            InworldController.CurrentScene = InworldAI.Game.currentScene;
        }
        [MenuItem("GameObject/Inworld/Add/Inworld Player", false, 1)]
        static void AddInworldPlayer()
        {
            AddInworldController();
            GameObject mainCamera;
            if (Camera.main)
            {
                mainCamera = Camera.main.gameObject;
                Object.DestroyImmediate(mainCamera);
            }
            mainCamera = PrefabUtility.InstantiatePrefab(InworldAI.PlayerControllerPrefab) as GameObject;
            InworldController.Player = mainCamera;
        }
        [MenuItem("GameObject/Inworld/Add/Inworld Character", true, 3)]
        static bool CheckInworldController()
        {
            return InworldController.Instance;
        }
        [MenuItem("GameObject/Inworld/Add/Inworld Character", false, 3)]
        static void AttachInworldCharacter()
        {
            if (!InworldController.Instance)
            {
                // YAN: Need to instantiate that first.
                InworldLog.LogError("Cannot Find InworldController!");
                return;
            }
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject)
            {
                if (selectedObject.GetComponent<InworldController>())
                {
                    InworldLog.LogError("Cannot Add InworldChatacters on InworldController");
                    return;
                }
                InworldCharacter selectedChar = selectedObject.GetComponent<InworldCharacter>();
                if (!selectedChar)
                {
                    selectedChar = Object.Instantiate(InworldAI.CharacterPrefab, selectedObject.transform);
                    Transform instantiateTransform = selectedChar.transform;
                    instantiateTransform.SetParent(selectedObject.transform.parent);
                    selectedObject.transform.SetParent(instantiateTransform);
                    instantiateTransform.name = selectedObject.transform.name;
                }
                if (!InworldController.CurrentScene.characters.Contains(selectedChar.BrainName))
                {
                    selectedChar.LoadCharacter
                    (
                        InworldAI.Game.currentWorkspace.characters.FirstOrDefault
                            (data => InworldController.CurrentScene.characters.Contains(data.brain)),
                        selectedObject
                    );
                }
            }
            else
            {
                InworldLog.LogError("Cannot Add InworldChatacters based on Empty!");
            }

        }
        #endregion
    }

    /// <summary>
    ///     Add a back button to navigate back to InworldAI Global settings.
    /// </summary>
    public class InworldInspector : UnityEditor.Editor
    {
        GUIStyle m_BtnStyle;
        GUIStyle BtnStyle
        {
            get
            {
                if (m_BtnStyle != null)
                    return m_BtnStyle;
                m_BtnStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 12,
                    fixedWidth = 100,
                    margin = new RectOffset(10, 10, 0, 0)
                };
                return m_BtnStyle;
            }
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space(40);
            if (GUILayout.Button("Back", BtnStyle))
            {
                Selection.SetActiveObjectWithContext(InworldAI.Instance, InworldAI.Instance);
            }
        }
    }
    [CustomEditor(typeof(InworldGameSettings))] public class InworldGameSettingInspector : InworldInspector {}
    [CustomEditor(typeof(GLTFAvatarLoader))] public class InworldAvatarLoaderInspector : InworldInspector {}
}
#endif

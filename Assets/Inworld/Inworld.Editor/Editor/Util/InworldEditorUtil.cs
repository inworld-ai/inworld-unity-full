/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.Networking;


namespace Inworld.Editors
{
    /// <summary>
    ///     This class would be called when package is imported, or Unity Editor is opened.
    /// </summary>
    [InitializeOnLoad]
    public class InworldEditorUtil : IPreprocessBuildWithReport
    {
        /// <summary>
        /// Interface property. 
        /// </summary>
        public int callbackOrder { get; }
        
        /// <summary>
        /// Get the path for UserData.
        /// </summary>
        public static string UserDataPath => Path.GetDirectoryName(AssetDatabase.GetAssetPath(InworldAI.User));
        
        static InworldEditorUtil()
        {
            AssetDatabase.importPackageCompleted += packageName =>
            {
                if (!packageName.StartsWith("InworldExtraAssets"))
                    return;
                _AddDebugMacro();
            };
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.quitting += OnEditorQuitting;
        }
        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                case PlayModeStateChange.EnteredEditMode:
                    if (InworldAI.IsDebugMode)
                        _AddDebugMacro();
                    else
                        _RemoveDebugMacro();
                    break;
            }
        }
        /// <summary>
        /// Remove all the Inworld logs. those log will not be printed out in the runtime.
        /// Needs to be public to be called outside Unity.
        /// </summary>
        /// <param name="report"></param>
        public void OnPreprocessBuild(BuildReport report)
        {
            if (Debug.isDebugBuild || InworldAI.IsDebugMode)
                return;
            _RemoveDebugMacro();
        }
        static void OnEditorQuitting()
        {
            InworldEditor.Instance.SaveData();
        }
        

#region Top Menu
        [MenuItem("Inworld/Inworld Studio Panel", false, 0)]
        static void TopMenuConnectStudio() => InworldStudioPanel.Instance.ShowPanel();

        [MenuItem("Inworld/Inworld Settings", false, 1)]
        static void TopMenuShowPanel() => InworldAIEditor.Instance.ShowPanel();

        [MenuItem("Inworld/User Settings", false, 1)]
        static void TopMenuUserPanel() => _OpenUserPanel();
        
        [MenuItem("Inworld/Editor Settings", false, 1)]
        static void TopMenuEditorPanel() => Selection.SetActiveObjectWithContext(InworldEditor.Instance, InworldEditor.Instance);
#endregion


#region Asset Menu
        [MenuItem("Assets/Inworld/Studio Panel", false, 0)]
        static void ConnectStudio() => InworldStudioPanel.Instance.ShowPanel();

        [MenuItem("Assets/Inworld/Default Settings", false, 1)]
        static void ShowPanel() => InworldAIEditor.Instance.ShowPanel();

        [MenuItem("Assets/Inworld/User Settings", false, 1)]
        static void UserPanel() => _OpenUserPanel();
        
        [MenuItem("Assets/Inworld/Editor Settings", false, 1)]
        static void EditorPanel() => Selection.SetActiveObjectWithContext(InworldEditor.Instance, InworldEditor.Instance);
#endregion

#region Hierarchy Menu
        [MenuItem("GameObject/Inworld/Upgrade Material", false, 0)]
        static void UpgradeMaterial() => InworldRenderPipelineConverter.UpgradeMaterial();
#endregion
        
        /// <summary>
        /// Editor based send web request.
        /// </summary>
        /// <param name="url">the url to send web request</param>
        /// <param name="withToken">check if with token. Token should be stored in InworldEditor.Token.</param>
        /// <param name="callback">the callback function after web request finished or failed.</param>
        public static void SendWebGetRequest(string url, bool withToken, Action<AsyncOperation> callback)
        {
            UnityWebRequest uwr = new UnityWebRequest(url, "GET");
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.timeout = 60;
            if (withToken)
            {
                if (string.IsNullOrEmpty(InworldEditor.Token))
                {
                    InworldEditor.Instance.Error = InworldEditor.k_TokenErrorInstruction;
                    return;
                }
                uwr.SetRequestHeader("Authorization", InworldEditor.Token);
            }
            UnityWebRequestAsyncOperation updateRequest = uwr.SendWebRequest();
            updateRequest.completed += callback;
        }
        /// <summary>
        /// Download assets and binding to the character's CharacterAsset
        /// </summary>
        /// <param name="charFullName">the full name of the InworldCharacter</param>
        /// <param name="url">the url of the assets to download</param>
        /// <param name="callback">the callback function once finished</param>
        public static void DownloadCharacterAsset(string charFullName, string url, Action<string, AsyncOperation> callback)
        {
            SendWebGetRequest(url, false, op => callback(charFullName, op));
        }
        /// <summary>
        /// Get the actual UnityWebRequest in call back.
        /// </summary>
        /// <param name="op">the ayncoperation in sending web request in the call back.</param>
        /// <returns></returns>
        public static UnityWebRequest GetResponse(AsyncOperation op) => op is UnityWebRequestAsyncOperation webTask ? webTask.webRequest : null;
        /// <summary>
        /// Render a drop down field in Editor GUI.
        /// </summary>
        /// <param name="currentItem">the selected item</param>
        /// <param name="values">the list of the selections</param>
        /// <param name="callback">the callback function once selected</param>
        public static void DrawDropDown(string currentItem, List<string> values, Action<string> callback)
        {
            if (!EditorGUILayout.DropdownButton(new GUIContent(currentItem), FocusType.Passive, InworldEditor.Instance.DropDownStyle))
                return;
            GenericMenu menu = new GenericMenu();

            foreach (string value in values)
            {
                menu.AddItem(new GUIContent(value), false, () => callback(value));
            }
            menu.ShowAsContext();
        }
        static void _OpenUserPanel()
        {
            if (!Directory.Exists(InworldEditor.UserDataPath))
            {
                if (EditorUtility.DisplayDialog("Loading User Data failed", "Cannot find User Data. Please login first.", "OK", "Cancel"))
                    ConnectStudio();
            }
            else
                Selection.SetActiveObjectWithContext(InworldAI.User, InworldAI.User);
        }
        static void _SetDefaultUserName()
        {
            string userName = CloudProjectSettings.userName;
            InworldAI.User.Name = !string.IsNullOrEmpty(userName) && userName.Split('@').Length > 1 ? userName.Split('@')[0] : userName;
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
    }
}
#endif
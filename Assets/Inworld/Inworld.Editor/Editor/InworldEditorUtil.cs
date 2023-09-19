#if UNITY_EDITOR
using Inworld.Util;
using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.Networking;


namespace Inworld.AI.Editor
{
    /// <summary>
    ///     This class would be called when package is imported, or Unity Editor is opened.
    /// </summary>
    [InitializeOnLoad]
    public class InworldEditorUtil : IPreprocessBuildWithReport
    {
        const string k_VersionCheckURL = "https://api.github.com/repos/inworld-ai/inworld-unity-sdk/releases";
        const string k_ReleaseURL = "https://github.com/inworld-ai/inworld-unity-sdk/releases";
        
        public int callbackOrder { get; }
        public void OnPreprocessBuild(BuildReport report)
        {
            if (Debug.isDebugBuild || InworldAI.IsDebugMode)
                return;
            _RemoveDebugMacro();
        }
        static InworldEditorUtil()
        {
            AssetDatabase.importPackageCompleted += packName =>
            {
                string userName = CloudProjectSettings.userName;
                InworldAI.User.Name = !string.IsNullOrEmpty(userName) && userName.Split('@').Length > 1 ? userName.Split('@')[0] : userName;
                _AddDebugMacro();
            };
            _CheckUpdates();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.quitting += OnEditorQuitting;
        }
        static void OnEditorQuitting()
        {
            InworldEditor.Instance.SaveData();
        }
        static void _CheckUpdates()
        {
            if (string.IsNullOrEmpty(InworldAI.ImportedTime))
                InworldAI.ImportedTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            SendWebGetRequest(k_VersionCheckURL, false, OnUpdateRequestComplete);
        }
        public static void SendWebGetRequest(string url, bool withToken, Action<AsyncOperation> callback)
        {
            UnityWebRequest uwr = new UnityWebRequest(url, "GET");
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.timeout = 60;
            if (withToken)
            {
                if (string.IsNullOrEmpty(InworldEditor.Token))
                {
                    InworldEditor.Instance.Error = $"Login Expired. Please login again";
                    return;
                }
                uwr.SetRequestHeader("Authorization", InworldEditor.Token);
            }
            UnityWebRequestAsyncOperation updateRequest = uwr.SendWebRequest();
            updateRequest.completed += callback;
        }
        public static void DownloadCharacterAsset(string charFullName, string url, Action<string, AsyncOperation> callback)
        {
            SendWebGetRequest(url, false, op => callback(charFullName, op));
        }
        public static string UserDataPath => Path.GetDirectoryName(AssetDatabase.GetAssetPath(InworldAI.User));

        public static UnityWebRequest GetResponse(AsyncOperation op) => op is UnityWebRequestAsyncOperation webTask ? webTask.webRequest : null;
        static void OnUpdateRequestComplete(AsyncOperation obj)
        {
            UnityWebRequest uwr = GetResponse(obj);
            string jsonStr = "{ \"package\": " + uwr.downloadHandler.text + "}";
            ReleaseData date = JsonUtility.FromJson<ReleaseData>(jsonStr);
            if (date.package == null || date.package.Length <= 0)
                return;
            string publishedDate = date.package[0].published_at;
            DateTime currentVersion = DateTime.ParseExact(publishedDate, "yyyy-MM-ddTHH:mm:ssZ", null, System.Globalization.DateTimeStyles.RoundtripKind);
            DateTime importedTime = DateTime.ParseExact(InworldAI.ImportedTime, "yyyy-MM-ddTHH:mm:ssZ", null, System.Globalization.DateTimeStyles.RoundtripKind);
            if (importedTime < currentVersion) 
            {
                InworldAI.LogWarning($"Your Inworld SDK is outdated. Please fetch the newest from Asset Store or {k_ReleaseURL}");
            }
            else
            {
                InworldAI.Version = date.package[0]?.tag_name;
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
        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    if (InworldAI.IsDebugMode)
                        _AddDebugMacro();
                    else
                        _RemoveDebugMacro();
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    _AddDebugMacro();
                    break;
            }
        }
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
        public static void UpgradeProtocol<T>() where T : InworldClient
        {
            if (!InworldController.Instance)
                return;
            InworldClient currClient = InworldController.Instance.GetComponent<InworldClient>();
            if (!currClient)
                return;
            
            T newClient = InworldController.Instance.gameObject.AddComponent<T>();
            newClient.CopyFrom(currClient);

            UnityEngine.Object.DestroyImmediate(currClient);
        }
        /// <summary>
        ///     For right click the project window.
        /// </summary>
#region Top Menu
        [MenuItem("Inworld/Inworld Studio Panel", false, 0)]
        static void TopMenuConnectStudio() => InworldStudioPanel.Instance.ShowPanel();

        [MenuItem("Inworld/Inworld Settings", false, 1)]
        static void TopMenuShowPanel() => Selection.SetActiveObjectWithContext(InworldAI.Instance, InworldAI.Instance);
        
        [MenuItem("Inworld/User Settings", false, 1)]
        static void TopMenuUserPanel() => Selection.SetActiveObjectWithContext(InworldAI.User, InworldAI.User);
        
        [MenuItem("Inworld/Editor Settings", false, 1)]
        static void TopMenuEditorPanel() => Selection.SetActiveObjectWithContext(InworldEditor.Instance, InworldEditor.Instance);
                
        [MenuItem("Inworld/Switch Protocol/Web socket")]
        public static void SwitchToWebSocket() => UpgradeProtocol<InworldWebSocketClient>();
#endregion

        /// <summary>
        ///     For right click the project window.
        /// </summary>
#region Asset Menu
        [MenuItem("Assets/Inworld/Studio Panel", false, 0)]
        static void ConnectStudio() => InworldStudioPanel.Instance.ShowPanel();

        [MenuItem("Assets/Inworld/Default Settings", false, 1)]
        static void ShowPanel() => Selection.SetActiveObjectWithContext(InworldAI.Instance, InworldAI.Instance);
        
        [MenuItem("Assets/Inworld/User Settings", false, 1)]
        static void UserPanel() => Selection.SetActiveObjectWithContext(InworldAI.User, InworldAI.User);
        
        [MenuItem("Assets/Inworld/Editor Settings", false, 1)]
        static void EditorPanel() => Selection.SetActiveObjectWithContext(InworldEditor.Instance, InworldEditor.Instance);
#endregion

    }
}
#endif
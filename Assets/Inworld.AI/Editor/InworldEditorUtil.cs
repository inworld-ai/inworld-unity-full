#if !UNITY_WEBGL
using Inworld.Util;
using System;
using UnityEditor;
using UnityEngine;
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
        }
        static void _CheckUpdates()
        {
            if (string.IsNullOrEmpty(InworldAI.ImportedTime))
                InworldAI.ImportedTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            UnityWebRequest uwr = new UnityWebRequest(k_VersionCheckURL, "GET");
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.timeout = 60;
            UnityWebRequestAsyncOperation updateRequest = uwr.SendWebRequest();
            updateRequest.completed += OnUpdateRequestComplete;
        }
        static UnityWebRequest _GetResponse(AsyncOperation op)
        {
            return op is not UnityWebRequestAsyncOperation webTask ? null : webTask.webRequest;
        }
        static void OnUpdateRequestComplete(AsyncOperation obj)
        {
            UnityWebRequest uwr = _GetResponse(obj);
            string jsonStr = "{ \"package\": " + uwr.downloadHandler.text + "}";
            ReleaseData date = JsonUtility.FromJson<ReleaseData>(jsonStr);
            if (date.package.Length <= 0)
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
                InworldAI.Version = date.package[0].tag_name;
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
        public static void UpgradeProtocol<T>() where T : InworldClient
        {
            if (!InworldController.Instance)
                return;
            InworldClient currClient = InworldController.Instance.GetComponent<InworldClient>();
            if (!currClient)
                return;
            InworldServerConfig currServer = currClient.Server;
            string apiKey = currClient.APIKey;
            string apiSecret = currClient.APISecret;
            string customToken = currClient.CustomToken;
            T newClient = InworldController.Instance.gameObject.AddComponent<T>();
            newClient.Server = currServer;
            newClient.APISecret = apiSecret;
            newClient.APIKey = apiKey;
            newClient.CustomToken = customToken;
            InworldController.Client = newClient;
            UnityEngine.Object.DestroyImmediate(currClient);
        }
        [MenuItem("Inworld/Switch Protocol/Web socket")]
        public static void SwitchToWebSocket() => UpgradeProtocol<InworldWebSocketClient>();
    }
}
#endif
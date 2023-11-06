/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.Networking;
using Inworld.Entities;
using UnityEngine.Rendering;

namespace Inworld.Editors
{
    /// <summary>
    ///     This class would be called when package is imported, or Unity Editor is opened.
    /// </summary>
    [InitializeOnLoad]
    public class InworldEditorUtil : IPreprocessBuildWithReport
    {
        static readonly int s_LegacyBaseMap = Shader.PropertyToID("_MainTex");
        static readonly int s_LegacyNormalMap = Shader.PropertyToID("_BumpMap");
        static readonly int s_MetallicMap = Shader.PropertyToID("_MetallicGlossMap");
        static readonly int s_Smoothness = Shader.PropertyToID("_Smoothness");
        
        static readonly int s_URPBaseMap = Shader.PropertyToID("_BaseMap");
        static readonly int s_URPNormalMap = Shader.PropertyToID("_BumpMap");
        
        static readonly int s_HDRPBaseMap = Shader.PropertyToID("_BaseColorMap");
        static readonly int s_HDRPNormalMap = Shader.PropertyToID("_NormalMap");
        
        const string k_VersionCheckURL = "https://api.github.com/repos/inworld-ai/inworld-unity-sdk/releases";
        const string k_UpdateTitle = "Notice";
        const string k_UpdateContent = "Detected you're using Inworld SDK v2, it's not compatible with current Inworld package. We recommend you delete the whole Inworld.AI folder and then import.";
        const string k_LegacyPackage = "Inworld.AI";
        public int callbackOrder { get; }
        
        public static string UserDataPath => Path.GetDirectoryName(AssetDatabase.GetAssetPath(InworldAI.User));
        
        /// <summary>
        /// Remove all the Inworld logs. those log will not be printed out in the runtime.
        /// </summary>
        /// <param name="report"></param>
        public void OnPreprocessBuild(BuildReport report)
        {
            if (Debug.isDebugBuild || InworldAI.IsDebugMode)
                return;
            _RemoveDebugMacro();
        }
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
        /// <summary>
        /// Switch protocol: (Websocket or NDK)
        /// </summary>
        /// <typeparam name="T">the InworldClient which uses the target protocol.</typeparam>
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

        static InworldEditorUtil()
        {
            AssetDatabase.importPackageStarted += name =>
            {
                string[] allAssetPaths = AssetDatabase.GetSubFolders("Assets");
                if (!allAssetPaths.Any(assetPath => assetPath.Contains(k_LegacyPackage)))
                    return;
                EditorUtility.DisplayDialog(k_UpdateTitle, k_UpdateContent, "OK");
            };

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
            if (string.IsNullOrEmpty(InworldAI.Version))
                SendWebGetRequest(k_VersionCheckURL, false, OnUpdateRequestComplete);
        }

        static void OnUpdateRequestComplete(AsyncOperation obj)
        {
            UnityWebRequest uwr = GetResponse(obj);
            string jsonStr = "{ \"package\": " + uwr.downloadHandler.text + "}";
            ReleaseData date = JsonUtility.FromJson<ReleaseData>(jsonStr);
            if (date.package == null || date.package.Length <= 0)
                return;
            InworldAI.Version = date.package[0]?.tag_name;
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

#region Hierarchy Menu
        [MenuItem("GameObject/Inworld/Upgrade Material", false, 0)]
        static void UpgradeMaterial()
        {
            if (!GraphicsSettings.currentRenderPipeline)
            {
                InworldAI.LogError("Current Rendering pipeline is not URP or HDRP!");
                return;
            }
            InworldAI.Log($"Updating material for {Selection.activeGameObject.name}");
            Renderer[] renderers = Selection.activeGameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Material material = renderer.sharedMaterial;
                Material newMat = new Material(GraphicsSettings.currentRenderPipeline.defaultMaterial);
                if (material)
                {
                    Texture2D baseMap = material.GetTexture(s_LegacyBaseMap) as Texture2D;
                    Texture2D normalMap = material.GetTexture(s_LegacyNormalMap) as Texture2D;
                    Texture2D metallicMap = material.GetTexture(s_MetallicMap) as Texture2D;
                    newMat.SetTexture(s_URPBaseMap, baseMap);
                    newMat.SetTexture(s_HDRPBaseMap, baseMap);
                    newMat.SetTexture(s_URPNormalMap, normalMap);
                    newMat.SetTexture(s_HDRPNormalMap, normalMap);
                    newMat.SetTexture(s_MetallicMap, metallicMap);
                    newMat.SetFloat(s_Smoothness, 0.15f); // YAN: GLTF's smoothness = 1 - mainTex.g * _Roughness.
                }
                renderer.material = newMat;
            }
            InworldAI.Log($"{Selection.activeGameObject.name} Updating material completed!");
        }
#endregion
    }
}
#endif
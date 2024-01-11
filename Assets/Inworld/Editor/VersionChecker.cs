/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using Inworld.Entities;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Inworld
{
    public static class VersionChecker
    {
        const string k_VersionCheckURL = "https://api.github.com/repos/inworld-ai/inworld-unity-sdk/releases";
        const string k_VersionQuestFailed = "Fetching Inworld SDK Version Failed. Please check your network.";
        const string k_UpdateTitle = "Notice";
        const string k_UpdateContent = "Detected you're using Inworld SDK v2, it's not compatible with current Inworld package. We recommend you delete the whole Inworld.AI folder and then import.";
        const string k_LegacyPackage = "Inworld.AI";

        public static bool IsLegacyPackage => AssetDatabase.GetSubFolders("Assets").Any(assetPath => assetPath.Contains(k_LegacyPackage));

        public static void NoticeLegacyPackage()
        {
            EditorUtility.DisplayDialog(k_UpdateTitle, k_UpdateContent, "OK");
        }
        public static void CheckVersionUpdates()
        {
            Debug.Log("Check Version Updates...");
            if (string.IsNullOrEmpty(InworldAI.Version))
                SendWebGetRequest(k_VersionCheckURL, OnUpdateRequestComplete);
        }
        static void SendWebGetRequest(string url, Action<AsyncOperation> callback)
        {
            UnityWebRequest uwr = new UnityWebRequest(url, "GET");
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.timeout = 60;
            UnityWebRequestAsyncOperation updateRequest = uwr.SendWebRequest();
            updateRequest.completed += callback;
        }
        static UnityWebRequest GetResponse(AsyncOperation op) => op is UnityWebRequestAsyncOperation webTask ? webTask.webRequest : null;
        static void OnUpdateRequestComplete(AsyncOperation obj)
        {
            UnityWebRequest uwr = GetResponse(obj);
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                InworldAI.LogError(k_VersionQuestFailed);
                return;
            }
            string jsonStr = "{ \"package\": " + uwr.downloadHandler.text + "}";
            ReleaseData date = JsonUtility.FromJson<ReleaseData>(jsonStr);
            if (date.package == null || date.package.Length <= 0)
                return;
            InworldAI.Version = date.package[0]?.tag_name;
            Debug.Log("Check Version Completed.");
        }
    }
}
#endif
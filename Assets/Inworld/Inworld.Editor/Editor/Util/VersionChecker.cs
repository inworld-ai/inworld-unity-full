/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using Inworld.Entities;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Inworld.Editors
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
            InworldAI.Log("Check Version Updates...");
            if (string.IsNullOrEmpty(InworldAI.Version))
                InworldEditorUtil.SendWebGetRequest(k_VersionCheckURL, false, OnUpdateRequestComplete);
        }
        static void OnUpdateRequestComplete(AsyncOperation obj)
        {
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(obj);
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
            InworldAI.Log("Check Version Completed.");
        }
    }
}
#endif
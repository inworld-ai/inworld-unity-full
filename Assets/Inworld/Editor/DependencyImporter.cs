/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;


namespace Inworld
{
    [InitializeOnLoad]
    public class DependencyImporter : AssetPostprocessor
    {
        const string k_Pathv2 = "Assets/Inworld.AI";
        const string k_Pathv3 = "Assets/Inworld/Inworld.AI"; // YAN: These 2 folders are incompatible.
        const string k_UpgradeTitle = "Legacy Inworld found";
        const string k_UpgradeContent = "Unable to upgrade. Please delete the folder Assets/Inworld, and reimport this package";
        const string k_DependencyPackages = "https://github.com/inworld-ai/inworld-unity.git#yj3.2";
        static DependencyImporter()
        {
            AssetDatabase.importPackageCompleted += async packageName =>
            {
                await InstallDependencies();
            };
        }
        

        public static async Task InstallDependencies()
        {
            if (Directory.Exists(k_Pathv2) || Directory.Exists(k_Pathv3))
            {
                if (EditorUtility.DisplayDialog(k_UpgradeTitle, k_UpgradeContent, "OK"))
                    return;
            } 
            Debug.Log("Import Dependency Packages...");
            await _AddPackage(k_DependencyPackages);
        }


        static async Task _AddPackage(string packageFullName)
        {
            ListRequest listRequest = Client.List();

            while (!listRequest.IsCompleted)
            {
                await Task.Yield();
            }
            if (listRequest.Status != StatusCode.Success)
            {
                Debug.LogError(listRequest.Error.ToString());
                return;
            }
            if (listRequest.Result.Any(x => x.name == packageFullName))
            {
                Debug.Log($"{packageFullName} Found.");
                return;
            }

            AddRequest addRequest = Client.Add(packageFullName);
            while (!addRequest.IsCompleted)
            {
                await Task.Yield();
            }

            if (addRequest.Status != StatusCode.Success)
            {
                Debug.LogError($"Failed to add {packageFullName}.");
                return;
            }
            Debug.Log($"Import {packageFullName} Completed");
            if (!Directory.Exists("Assets/Inworld/Inworld.Assets") && File.Exists("Assets/Inworld/InworldExtraAssets.unitypackage"))
                AssetDatabase.ImportPackage("Assets/Inworld/InworldExtraAssets.unitypackage", false);
        }
    }
}
#endif

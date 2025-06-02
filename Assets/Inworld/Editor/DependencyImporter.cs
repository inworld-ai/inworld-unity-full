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
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Inworld
{
    [InitializeOnLoad]
    public class DependencyImporter : AssetPostprocessor
    {
        const string k_PkgName = "InworldAI.Full";
        const string k_ExtraAssets = "InworldExtraAssets";
        const string k_InworldPath = "Assets/Inworld";
        const string k_UpgradeTitle = "Legacy Inworld found";
        const string k_UpgradeContent = "Unable to upgrade. Please delete the folder Assets/Inworld, and reimport this package";

        static readonly string[] s_DependentPackages = {
            "com.unity.nuget.newtonsoft-json",
            "com.unity.cloud.gltfast"
        };
        
        static DependencyImporter()
        {
            AssetDatabase.importPackageCompleted += name =>
            {
                if (name.Contains(k_PkgName))
                    InstallDependencies();
                else if (name == k_ExtraAssets)
                    _InstallTMP();
            };
        }
        
        [MenuItem("Inworld/Install Dependencies/SDK")]
        public static async void InstallDependencies()
        {
            if (Directory.Exists($"Assets/Inworld.AI"))
            {
                if (EditorUtility.DisplayDialog(k_UpgradeTitle, k_UpgradeContent, "OK"))
                    return;
            } 
            Debug.Log("Import Dependency Packages...");
            await _AddPackage(); 
        }

        static async Task _AddUnityPackage(string package)
        {
            ListRequest listRequest = UnityEditor.PackageManager.Client.List();

            while (!listRequest.IsCompleted)
            {
                await Task.Yield();
            }
            if (listRequest.Status != StatusCode.Success)
            {
                Debug.LogError(listRequest.Error.ToString());
                return;
            }
            if (listRequest.Result.Any(x => x.name == package))
            {
                Debug.Log($"{package} Found.");
                return;
            }
            AddRequest addRequest = UnityEditor.PackageManager.Client.Add(package);
            while (!addRequest.IsCompleted)
            {
                await Task.Yield();
            }
            if (addRequest.Status != StatusCode.Success)
            {
                Debug.LogError($"Failed to add {package}.");
                return;
            }
            Debug.Log($"Import {package} Completed");
        }
        
        static async Task _AddPackage()
        {
            foreach (string dependentPackage in s_DependentPackages)
            {
                await _AddUnityPackage(dependentPackage);
            }
            if (!Directory.Exists($"{k_InworldPath}/Inworld.Assets") 
                && File.Exists($"{k_InworldPath}/{k_ExtraAssets}.unitypackage"))
                AssetDatabase.ImportPackage($"{k_InworldPath}/InworldExtraAssets.unitypackage", false);
        }
        
        static void _InstallTMP()
        {
            if (File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"))
                return;
            string packageFullPath = TMP_EditorUtility.packageFullPath;
            AssetDatabase.ImportPackage(packageFullPath + "/Package Resources/TMP Essential Resources.unitypackage", false);
        }
    }
}
#endif

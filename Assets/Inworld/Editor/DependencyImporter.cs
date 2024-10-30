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
        const string k_Pathv2 = "Assets/Inworld.AI";
        const string k_Pathv3 = "Assets/Inworld/Inworld.AI"; // YAN: These 2 folders are incompatible.
        const string k_UpgradeTitle = "Legacy Inworld found";
        const string k_UpgradeContent = "Unable to upgrade. Please delete the folder Assets/Inworld, and reimport this package";
        const string k_DependencyPackage = "com.inworld.unity.core";
        const string k_InworldAssetsPath = "Assets/Inworld/Inworld.Assets";
        const string k_ExtraPackagePath = "Assets/Inworld/InworldExtraAssets.unitypackage";
        
        static DependencyImporter()
        {
            AssetDatabase.importPackageCompleted += _ =>
            {
                InstallDependencies();
            };
        }
        
        [MenuItem("Inworld/Export Package/Install Dependencies")]
        public static async void InstallDependencies()
        {
            if (Directory.Exists(k_Pathv2) || Directory.Exists(k_Pathv3))
            {
                if (EditorUtility.DisplayDialog(k_UpgradeTitle, k_UpgradeContent, "OK"))
                    return;
            } 
            Debug.Log("Import Dependency Packages...");
            await _AddPackage(); 
        }

        static string _GetTgzFileName()
        {
            string searchDirectory = Path.Combine(Application.dataPath, "Inworld");
            string[] tgzFiles = Directory.GetFiles(searchDirectory, "*.tgz", SearchOption.TopDirectoryOnly);
            return tgzFiles.Length > 0 ? $"file:{tgzFiles[0]}" : "";
        }
        static async Task _AddUnityPackage(string package, string detail = "")
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
            string pkgToLoad = string.IsNullOrEmpty(detail) ? package : detail;
            AddRequest addRequest = UnityEditor.PackageManager.Client.Add(pkgToLoad);
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
            await _AddUnityPackage(k_DependencyPackage, _GetTgzFileName());
            if (!Directory.Exists(k_InworldAssetsPath) && File.Exists(k_ExtraPackagePath))
                AssetDatabase.ImportPackage(k_ExtraPackagePath, false);
            if (System.Type.GetType("TMPro.TextMeshPro, Unity.TextMeshPro") != null)
                return;
            string packageFullPath = TMP_EditorUtility.packageFullPath;
            AssetDatabase.ImportPackage(packageFullPath + "/Package Resources/TMP Essential Resources.unitypackage", false);
        }
    }
}
#endif

/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;


namespace Inworld
{
    public class DependencyImporter : AssetPostprocessor
    {
        // YAN: Add other dependencies here.
        static readonly string[] s_DependencyPackages = 
        {
            "com.unity.cloud.gltfast"
        };
        public static async Task InstallDependencies()
        {
            Debug.Log("Import Dependency Packages...");
            foreach (string dependency in s_DependencyPackages)
            {
                await _AddPackage(dependency);
            }
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
        }
    }
}
#endif

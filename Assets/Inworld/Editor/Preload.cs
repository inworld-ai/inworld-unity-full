/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Inworld
{
    [InitializeOnLoad]
    public class Preload : MonoBehaviour
    {
        static Preload()
        {
            AssetDatabase.importPackageCompleted += async packageName =>
            {
                if (!packageName.StartsWith("InworldAI.Full"))
                    return;
                await DependencyImporter.InstallDependencies();
                VersionChecker.CheckVersionUpdates();
                if (VersionChecker.IsLegacyPackage)
                    VersionChecker.NoticeLegacyPackage();
                TMP_PackageResourceImporter.ImportResources(true, false, false);
                AssetDatabase.ImportPackage("Assets/Inworld/InworldExtraAssets.unitypackage", false);
                _SetDefaultUserName();
#if UNITY_EDITOR && VSP
                if (!string.IsNullOrEmpty(InworldAI.User.Account))
                    VSAttribution.SendAttributionEvent("Login Studio", InworldAI.k_CompanyName, InworldAI.User.Account);     
#endif
            };
        }
        static void _SetDefaultUserName()
        {
            string userName = CloudProjectSettings.userName;
            InworldAI.User.Name = !string.IsNullOrEmpty(userName) && userName.Split('@').Length > 1 ? userName.Split('@')[0] : userName;
        }
    }
}
#endif
/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System.IO;
using UnityEditor;

namespace Inworld
{
	/// <summary>
	///     This file would be called by commands, for auto-generate Unity packages.
	/// </summary>
	public static class UnityPackageExporter
    {
        // Path to export to.
        const string k_ExportPath = "Build";
        // The name of the unitypackage to output.
        const string k_FullPackageName = "InworldAI.Full";
        const string k_LitePackageName = "InworldAI.Lite";

        // The path to the package under the `Assets/` folder.
        const string k_FullPackagePath = "Assets/Inworld";
        const string k_LitePackagePath = "Assets/Inworld/Inworld.AI";
        const string k_ExtraPackagePath = "Assets/Inworld/InworldExtraAssets.unitypackage";

        /// <summary>
        ///     Call it via outside command line to export package.
        /// </summary>
        [MenuItem("Inworld/Export Package/Full")]
        public static void ExportFull()
        {
            ExportExtraAssets();
            string[] assetPaths =
            {
                "Assets/Inworld/Inworld.AI", 
                "Assets/Inworld/Editor",
                k_ExtraPackagePath
            }; 
            AssetDatabase.ExportPackage(assetPaths, $"{k_FullPackagePath}/{k_FullPackageName}.unitypackage", ExportPackageOptions.Recurse);
        }
        [MenuItem("Inworld/Export Package/Lite")]
        public static void ExportLite() => AssetDatabase.ExportPackage(k_LitePackagePath, $"{k_FullPackagePath}/{k_LitePackageName}.unitypackage", ExportPackageOptions.Recurse);
        
        [MenuItem("Inworld/Export Package/Extra Assets")]
        public static void ExportExtraAssets()
        {
            string[] assetPaths =
            {
                "Assets/Inworld/Inworld.Assets", 
                "Assets/Inworld/Inworld.Editor",
                "Assets/Inworld/Inworld.NDK",
                "Assets/Inworld/Inworld.Samples.Innequin",
                "Assets/Inworld/Inworld.Samples.RPM"
            }; 
            AssetDatabase.ExportPackage(assetPaths, k_ExtraPackagePath, ExportPackageOptions.Recurse); 
        }
    }
}

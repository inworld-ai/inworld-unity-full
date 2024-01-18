/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Inworld
{
	/// <summary>
	///     This file would be called by commands, for auto-generate Unity packages.
	/// </summary>
	public static class UnityPackageExporter
    {
        // The name of the unitypackage to output.
        const string k_FullPackageName = "InworldAI.Full";

        // The path to the package under the `Assets/` folder.
        const string k_FullPackagePath = "Assets/Inworld";
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
                "Assets/Inworld/Editor",
                k_ExtraPackagePath
            }; 
            AssetDatabase.ExportPackage(assetPaths, $"{k_FullPackagePath}/{k_FullPackageName}.unitypackage", ExportPackageOptions.Recurse);
        }
        
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

        [MenuItem("Inworld/Build Test")]
        public static void BuildTestScene()
        {
            string[] scenes = { "Assets/Inworld/Inworld.AI/Scenes/Sample2D.unity"};
            BuildTarget[] platforms =
            {
                BuildTarget.Android, BuildTarget.iOS, BuildTarget.StandaloneWindows64, BuildTarget.WebGL, BuildTarget.StandaloneOSX
            };
            foreach (BuildTarget platform in platforms)
            {
                __BuildTestOnPlatform(scenes, platform);
            }
        }

        static void __BuildTestOnPlatform(string[] scenes, BuildTarget targetPlatform)
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = $"Builds/{targetPlatform}/InworldTest", // YAN: As a build test, we don't care the extension name
                target = targetPlatform,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            switch (summary.result)
            {
                case BuildResult.Succeeded:
                    Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
                    break;
                case BuildResult.Failed:
                    Debug.LogError("Build failed");
                    break;
            }
        }
    }
}

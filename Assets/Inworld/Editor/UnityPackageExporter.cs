/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;
using System.Collections.Generic;
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

        static readonly string Eol = Environment.NewLine;
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
 
        public static void BuildTestScene()
        {
            string[] scenes = { "Assets/Inworld/Inworld.Samples.RPM/Scenes/SampleBasic.unity"};

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = $"{EditorUserBuildSettings.activeBuildTarget}/BuildTest", // YAN: As a build test, we don't care the extension name
                target = EditorUserBuildSettings.activeBuildTarget,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            switch (summary.result)
            {
                case BuildResult.Succeeded:
                    Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
                    EditorApplication.Exit(0);
                    break;
                case BuildResult.Failed:
                    Debug.LogError("Build failed");
                    EditorApplication.Exit(101);
                    break;
                case BuildResult.Cancelled:
                    Console.WriteLine("Build cancelled!");
                    EditorApplication.Exit(102);
                    break;
                case BuildResult.Unknown:
                default:
                    Console.WriteLine("Build result is unknown!");
                    EditorApplication.Exit(103);
                    break;
            }
        }
        
    }
}

/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
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
        const string k_TestScenePath = "Assets/Inworld/Inworld.Samples/Scenes/InnequinBasic.unity";
        const string k_TestSceneWebGL = "Assets/Inworld/Inworld.Samples/Scenes/InnequinWebGL.unity";
        const string k_TestSceneMobile = "Assets/Inworld/Inworld.Samples/Scenes/InnequinMobile.unity";


        /// <summary>
        ///     Call it via outside command line to export package.
        /// </summary>
        [MenuItem("Inworld/Export Package/SDK", false, 102)]
        public static void ExportFull()
        {
            string[] assetPaths =
            {
                k_FullPackagePath
            }; 
            AssetDatabase.ExportPackage(assetPaths, $"{k_FullPackagePath}/{k_FullPackageName}.unitypackage", ExportPackageOptions.Recurse);
        }


        public static void BuildTestScene()
        {
            string sceneToBuild = "";
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                case BuildTarget.iOS:
                    sceneToBuild = k_TestSceneMobile;
                    break;
                case BuildTarget.WebGL:
                    sceneToBuild = k_TestSceneWebGL;
                    break;
                default:
                    sceneToBuild = k_TestScenePath;
                    break;
            }
            string[] scenes = { sceneToBuild };

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = $"{EditorUserBuildSettings.activeBuildTarget}/BuildTest", 
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

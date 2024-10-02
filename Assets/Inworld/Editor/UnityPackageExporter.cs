/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
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
        const string k_CorePackagePath = "../inworld-unity-core";
        const string k_FullPackagePath = "Assets/Inworld";
        const string k_ExtraPackagePath = "Assets/Inworld/InworldExtraAssets.unitypackage";
        const string k_TestScenePath = "Assets/Inworld/Inworld.Samples.RPM/Scenes/SampleBasic.unity";

        [MenuItem("Inworld/Export Package/Core")]
        public static async void ExportCore()
        {
            PackRequest req = UnityEditor.PackageManager.Client.Pack(k_CorePackagePath, k_FullPackagePath);
            while (!req.IsCompleted)
            {
                await Task.Yield();
            }
            switch (req.Status)
            {
                case StatusCode.Success:
                    Debug.Log($"Core Package Exported to {req.Result.tarballPath}!");
                    break;
                case StatusCode.Failure:
                    Debug.LogError(req.Error.message);
                    break;
            }
        }
        static string _GetTgzFileName()
        {
            string searchDirectory = Path.Combine(Application.dataPath, "Inworld");
            string[] tgzFiles = Directory.GetFiles(searchDirectory, "*.tgz", SearchOption.TopDirectoryOnly);
            return tgzFiles.Length > 0 ? $"{k_FullPackagePath}/{Path.GetFileName(tgzFiles[0])}" : "";
        }
        /// <summary>
        ///     Call it via outside command line to export package.
        /// </summary>
        [MenuItem("Inworld/Export Package/Full")]
        public static void ExportFull()
        {
            string corePath = _GetTgzFileName();
            if (string.IsNullOrEmpty(corePath))
            {
                Debug.LogError("Please extract core package first!");
                return;
            }
            ExportExtraAssets();
            string[] assetPaths =
            {
                corePath,
                $"{k_FullPackagePath}/Editor",
                k_ExtraPackagePath
            }; 
            AssetDatabase.ExportPackage(assetPaths, $"{k_FullPackagePath}/{k_FullPackageName}.unitypackage", ExportPackageOptions.Recurse);
        }
        /// <summary>
        ///     Call it via outside command line to export package.
        /// </summary>
        [MenuItem("Inworld/Export Package/Unity Asset Store")]
        public static void ExportUAS()
        {
            string corePath = _GetTgzFileName();
            if (string.IsNullOrEmpty(corePath))
            {
                Debug.LogError("Please extract core package first!");
                return;
            }
            string[] assetPaths =
            {
                k_FullPackagePath
            }; 
            AssetDatabase.ExportPackage(assetPaths, $"{k_FullPackagePath}/{k_FullPackageName}.unitypackage", ExportPackageOptions.Recurse);
        }
        [MenuItem("Inworld/Export Package/Extra Assets")]
        public static void ExportExtraAssets()
        {
            string[] assetPaths =
            {
                $"{k_FullPackagePath}/Inworld.Assets", 
                $"{k_FullPackagePath}/Inworld.Editor",
                $"{k_FullPackagePath}/Inworld.Native",
                $"{k_FullPackagePath}/Inworld.Samples.Innequin",
                $"{k_FullPackagePath}/Inworld.Samples.RPM"
            }; 
            AssetDatabase.ExportPackage(assetPaths, k_ExtraPackagePath, ExportPackageOptions.Recurse); 
        }
 
        public static void BuildTestScene()
        {
            string[] scenes = { k_TestScenePath };

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

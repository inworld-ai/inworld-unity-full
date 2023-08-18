/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using System.IO;
using UnityEditor;
namespace ExportPackage.Editor
{
	/// <summary>
	///     This file would be called by commands, for auto-generate Unity packages.
	/// </summary>
	public static class UnityPackageExporter
    {
        // The name of the unitypackage to output.
        const string k_PackageName = "ai.inworld.runtime-sdk-lite";
        const string k_NDKPackageName = "ai.inworld.runtime-sdk";
        const string k_samplePackageName = "ai.inworld.runtime-sdk-innequin";
        const string k_RPMSamplePackageName = "ai.inworld.runtime-sdk-samples";
        const string k_completePackageName = "ai.inworld.runtime-sdk-complete";


        // The path to the package under the `Assets/` folder.
        static string[] __packagePath = {"Assets/Inworld/Inworld.AI"};
        static string[] __NDKPackagePath = {"Assets/Inworld/Inworld.AI", "Assets/Inworld/Inworld.NDK", "Assets/Inworld/Inworld.Assets"};
        static string[] __samplePackagePath = {"Assets/Inworld/Inworld.AI", "Assets/Inworld/Inworld.Assets", "Assets/Inworld/Inworld.Samples.Innequin"};
        static string[] __RPMSamplesPackagePath = {"Assets/Inworld/Inworld.AI", "Assets/Inworld/Inworld.NDK", "Assets/Inworld/Inworld.Assets", "Assets/Inworld/Inworld.Samples.RPM"};
        static string[] __completePackagePath = {"Assets/Inworld"};

        // Path to export to.
        const string k_ExportPath = "Build";

        /// <summary>
        ///     Call it via outside command line to export package.
        /// </summary>
        public static void Export()
        {
            ExportPackage($"{k_ExportPath}/{k_PackageName}.unitypackage", __packagePath);
        }

        public static void ExportNDK()
        {
            ExportPackage($"{k_ExportPath}/{k_NDKPackageName}.unitypackage", __NDKPackagePath);
        }
        
        public static void ExportSample()
        {
            ExportPackage($"{k_ExportPath}/{k_samplePackageName}.unitypackage", __samplePackagePath);
        }

        public static void ExportRPMSamples()
        {
            ExportPackage($"{k_ExportPath}/{k_RPMSamplePackageName}.unitypackage", __RPMSamplesPackagePath);
        }
        
        public static void ExportCompletePackage()
        {
            ExportPackage($"{k_ExportPath}/{k_completePackageName}.unitypackage", __completePackagePath);
        }
        
        /// <summary>
        ///     Export package to target path.
        /// </summary>
        /// <param name="exportPath">target path to export.</param>
        /// <returns>string of the output full path</returns>
        public static string ExportPackage(string exportPath, string[] includePaths)
        {
            // Ensure export path.
            DirectoryInfo dir = new FileInfo(exportPath).Directory;
            if (dir != null && !dir.Exists)
            {
                dir.Create();
            }

            // Export
            AssetDatabase.ExportPackage
            (
                includePaths,
                exportPath,
                ExportPackageOptions.Recurse
            );
            return Path.GetFullPath(exportPath);
        }
    }
}

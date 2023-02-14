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
        const string k_PackageName = "ai.inworld.runtime-sdk";

        // The path to the package under the `Assets/` folder.
        const string k_PackagePath = "Inworld.AI";

        // Path to export to.
        const string k_ExportPath = "Build";

        /// <summary>
        ///     Call it via outside command line to export package.
        /// </summary>
        public static void Export()
        {
            ExportPackage($"{k_ExportPath}/{k_PackageName}.unitypackage");
        }
        /// <summary>
        ///     Export package to target path.
        /// </summary>
        /// <param name="exportPath">target path to export.</param>
        /// <returns>string of the output full path</returns>
        public static string ExportPackage(string exportPath)
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
                $"Assets/{k_PackagePath}",
                exportPath,
                ExportPackageOptions.Recurse
            );
            return Path.GetFullPath(exportPath);
        }
    }
}

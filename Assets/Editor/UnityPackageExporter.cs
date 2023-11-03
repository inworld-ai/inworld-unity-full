/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System.IO;
using UnityEditor;

namespace Inworld.Editors
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
        const string k_FullPackagePath = "Assets/Inworld/";
        const string k_LitePackagePath = "Assets/Inworld/Inworld.AI/";

        /// <summary>
        ///     Call it via outside command line to export package.
        /// </summary>
        public static void ExportFull() => ExportPackage($"{k_ExportPath}/{k_FullPackageName}.unitypackage", k_FullPackagePath);
        
        public static void ExportLite() => ExportPackage($"{k_ExportPath}/{k_LitePackageName}.unitypackage", k_LitePackagePath);
       
        /// <summary>
        ///     Export package to target path.
        /// </summary>
        /// <param name="exportPath">target path to export.</param>
        /// <param name="includePath">target path to include.</param>
        /// <returns>string of the output full path</returns>
        public static string ExportPackage(string exportPath, string includePath)
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
                includePath,
                exportPath,
                ExportPackageOptions.Recurse
            );
            return Path.GetFullPath(exportPath);
        }
    }
}

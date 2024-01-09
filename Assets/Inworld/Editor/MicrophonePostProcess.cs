/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Inworld
{
	public class MicrophonePostProcess
    {
        const string k_NativeJSPath = "Inworld/Editor/Native";
        
		[PostProcessBuild(1)]
		public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
		{
			if (target != BuildTarget.WebGL) 
				return;
			
			string indexPath = $"{pathToBuiltProject}/index.html";

			if (!File.Exists(indexPath))
			{
				Debug.LogError("Cannot load index file.");
				return;
			}
			string indexData = File.ReadAllText(indexPath);

			string dependencies = "<script src='./InworldMicrophone.js'></script>";

			if (!indexData.Contains(dependencies))
			{
				indexData = indexData.Insert(indexData.IndexOf("</head>"), $"\n{dependencies}\n");
				File.WriteAllText(indexPath, indexData);
			}
			File.Copy($"{Application.dataPath}/{k_NativeJSPath}/InworldMicrophone.txt", $"{pathToBuiltProject}/InworldMicrophone.js", true);
			File.Copy($"{Application.dataPath}/{k_NativeJSPath}/AudioResampler.txt", $"{pathToBuiltProject}/AudioResampler.js", true);
		}
	}
}

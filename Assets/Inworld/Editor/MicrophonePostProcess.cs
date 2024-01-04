using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
namespace FrostweepGames.MicrophonePro
{
	public sealed class MicrophonePostProcess
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

/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

#if !UNITY_WEBGL && UNITY_EDITOR
using Inworld.Audio.VAD;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

// TODO(Yan): Enable Sentis instead once future Sentis supported.
// Currently, put the onnx inside the StreamingAssets folder, to pass the C++ runtime build.

namespace Inworld.Native
{
    public class VADBuildProcessor : IPreprocessBuildWithReport
    {
        const string k_SourceFilePath = "Inworld/Inworld.Native/VAD/Plugins";
        const string k_TargetFileName =  "silero_vad.onnx";
        const string k_TargetMismatchTitle = "Error AudioManager detected.";
        const string k_TargetMismatchMobile = "AudioManager with AEC or VAD is not supported in your platform.\nPlease use AudioManagerMobile instead.";
        const string k_TargetMismatchWebGL = "AudioManager is not supported in your platform.\nPlease use AudioManagerWebGL instead.";
    
        [InitializeOnLoadMethod]
        static void OnProjectLoadedInEditor()
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler(BuildPlayerHandler);
        }

        static void BuildPlayerHandler(BuildPlayerOptions options)
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }
            string sourceName = $"{Application.dataPath}/{k_SourceFilePath}/{k_TargetFileName}";
            string targetName = $"{Application.streamingAssetsPath}/{k_TargetFileName}";

            if (!File.Exists(targetName))
            {
                if (File.Exists(sourceName))
                {
                    File.Copy(sourceName, targetName, true);
                    Debug.Log($"Copied {sourceName} to {targetName}");
                }
                else
                {
                    Debug.LogError($"Source file not found: {targetName}");
                }
            }
            BuildPipeline.BuildPlayer(options);
        }
        public int callbackOrder { get; }
        public void OnPreprocessBuild(BuildReport report)
        {
            switch (report.summary.platform)
            {
                case BuildTarget.Android:
                case BuildTarget.iOS:
                    if (InworldController.Instance && 
                        (InworldController.Audio.GetModule<AudioEchoFilter>() || InworldController.Audio.GetModule<VoiceActivityDetector>()))
                    {
                        EditorUtility.DisplayDialog(k_TargetMismatchTitle, k_TargetMismatchMobile, "OK");
                    }
                    break;
            }
        }
        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            Debug.Log($"Switch from {previousTarget} to {newTarget}");
        }
    }
}
#endif

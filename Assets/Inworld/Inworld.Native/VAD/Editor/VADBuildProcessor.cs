/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

#if !UNITY_WEBGL && UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

// TODO(Yan): Enable Sentis instead once future Sentis supported.
// Currently, put the onnx inside the StreamingAssets folder, to pass the C++ runtime build.

namespace Inworld.Native
{
    public class VADBuildProcessor
    {
        const string k_SourceFilePath = "Inworld/Inworld.Native/VAD/Plugins";
        const string k_TargetFileName =  "silero_vad.onnx";
    
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
    }
}
#endif

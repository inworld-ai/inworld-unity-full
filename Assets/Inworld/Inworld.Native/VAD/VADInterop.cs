/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System.Runtime.InteropServices;
namespace Inworld.Inworld.Native.VAD
{
    public class VADInterop
    {
        const string DLL_NAME = "inworld-ndk-vad";
        
        /// <summary>
        /// Initialize the VAD plugin with the prebuilt onnx file.
        /// We cannot use Sentis for now, because Sentis does not support IF in model.
        /// 
        /// TODO(Yan): Replace with Sentis directly when it's supported.
        /// </summary>
        /// <param name="model">the onnx model to load.</param>
        [DllImport(DLL_NAME)]
        public static extern void VAD_Initialize(string model);

        /// <summary>
        /// Free the VAD Handle
        /// </summary>
        [DllImport(DLL_NAME)]
        public static extern void VAD_Terminate();

        /// <summary>
        /// Sample the microphone data.
        /// </summary>
        /// <param name="audioData">the float array of the audio data we're sending.</param>
        /// <param name="size">the size of the audio data.</param>
        /// <returns></returns>
        [DllImport(DLL_NAME)]
        public static extern float VAD_Process(float[] audioData, int size);
        
        /// <summary>
        /// Clear the history input and reset the state.
        /// </summary>
        [DllImport(DLL_NAME)]
        public static extern void VAD_ResetState();
    }
}

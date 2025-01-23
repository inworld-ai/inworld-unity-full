/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Inworld.Audio.VAD
{
    public class VoiceActivityDetector : PlayerVoiceDetector
    {
        //TODO(Yan): Replace directly with Sentis when it supports IF condition.
        const string k_SourceFilePath = "Inworld/Inworld.Native/VAD/Plugins";
        const string k_TargetFileName =  "silero_vad.onnx";
        bool m_Initialized; 
        
        public bool IsAvailable =>  Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor;
        protected override void OnEnable()
        {
            gameObject.SetActive(IsAvailable);
            if (!gameObject.activeSelf)
                return;
#if UNITY_EDITOR
            VADInterop.VAD_Initialize($"{Application.dataPath}/{k_SourceFilePath}/{k_TargetFileName}");
#else
            VADInterop.VAD_Initialize($"{Application.streamingAssetsPath}/{k_TargetFileName}");
#endif
            m_Initialized = true;
            base.OnEnable();
        }
        protected void OnDestroy()
        {
            if (m_Initialized)
                VADInterop.VAD_Terminate();
        }
        protected override bool DetectPlayerSpeaking()
        {
            CircularBuffer<short> buffer = ShortBufferToSend;
            if (buffer == null || m_CurrPosition == ShortBufferToSend.currPos)
                return false;
            List<short> data = buffer.GetRange(m_CurrPosition, buffer.currPos);
            if (data.Count == 0)
                return false;
            float[] processedWave = WavUtility.ConvertInt16ArrayToFloatArray(data.ToArray());
            float vadResult = VADInterop.VAD_Process(processedWave, processedWave.Length);
            return vadResult * 30 > m_PlayerVolumeThreashold;
        }
    }
}

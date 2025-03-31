/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Microsoft.ML.OnnxRuntime.Unity;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace Inworld.Audio.VAD
{
    public class VoiceActivityDetector : PlayerVoiceDetector
    {
        //TODO(Yan): Replace directly with Sentis when it supports IF condition.
        [SerializeField] OrtAsset m_Model;

        LocalVAD m_VADLocal;
        bool m_Initialized; 
        
        /// <summary>
        ///     Check Available, will add mac support in the next update.
        ///     For mobile device such as Android/iOS they naturally supported via hardware.
        /// </summary>
        public bool IsAvailable => Application.platform == RuntimePlatform.WindowsPlayer 
                                   || Application.platform == RuntimePlatform.WindowsEditor
                                   || Application.platform == RuntimePlatform.OSXEditor
                                   || Application.platform == RuntimePlatform.OSXPlayer;
        protected override void OnEnable()
        {
            m_VADLocal = new LocalVAD(m_Model, m_PlayerVolumeThreashold);
            m_Initialized = true;
            base.OnEnable();
        }
        protected void OnDestroy()
        {
            if (m_Initialized)
                m_VADLocal.Dispose();
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
            float vadResult = m_VADLocal.Process(new List<float>(processedWave)) * 30;
            return vadResult > m_PlayerVolumeThreashold;
        }
    }
}

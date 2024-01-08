/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inworld.AEC
{
    public class InworldAECAudioCapture : AudioCapture
    {
        const int k_NumSamples = 160;
        IntPtr m_AECHandle;
        protected float[] m_OutputBuffer;

        
        /// <summary>
        /// A flag for this component is using AEC (in this class always True)
        /// </summary>
        public override bool EnableAEC => true;

        /// <summary>
        ///     Check Available, will add mac support in the next update.
        ///     For mobile device such as Android/iOS they naturally supported via hardware.
        /// </summary>
        public bool IsAvailable => Application.platform == RuntimePlatform.WindowsPlayer;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_AECHandle == IntPtr.Zero)
                return;
            AECInterop.WebRtcAec3_Free(m_AECHandle);
            m_AECHandle = IntPtr.Zero;
        }
        // YAN: Currently if you'd like to use AEC. The Audio Setting for output has to be 16000 sample rate, and mono.
        //      We'll add resampling features in the next update.
        protected override void Init()
        {
            if (IsAvailable)
            {
                AudioConfiguration audioSetting = AudioSettings.GetConfiguration();
                audioSetting.speakerMode = AudioSpeakerMode.Mono;
                audioSetting.sampleRate = k_SampleRate;
                AudioSettings.Reset(audioSetting);
                m_AECHandle = AECInterop.WebRtcAec3_Create(k_SampleRate);
            }
            else
                m_SamplingMode = MicSampleMode.TURN_BASED;
            base.Init();
        }

        protected override byte[] Output(int nSize)
        {
            short[] inputBuffer = WavUtility.ConvertAudioClipDataToInt16Array(m_InputBuffer, nSize * m_Recording.channels);
              
            m_OutputBuffer = new float[nSize];
            AudioListener.GetOutputData(m_OutputBuffer, 0);

            short[] outputBuffer = WavUtility.ConvertAudioClipDataToInt16Array(m_OutputBuffer, nSize * m_Recording.channels);
            return FilterAudio(inputBuffer, outputBuffer, m_AECHandle);
        }
        protected byte[] FilterAudio(short[] inputData, short[] outputData, IntPtr aecHandle)
        {
            List<short> filterBuffer = new List<short>();
            if (outputData == null || outputData.Length == 0 || !IsAvailable ||IsPlayerTurn)
            {
                filterBuffer.AddRange(inputData);
            }
            else
            {
                for (int i = 0; i < inputData.Length * m_Recording.channels; i += k_NumSamples)
                {
                    short[] inputTmp = new short[k_NumSamples];
                    short[] outputTmp = new short[k_NumSamples];
                    short[] filterTmp = new short[k_NumSamples];
                    Array.Copy(inputData, i, inputTmp, 0, k_NumSamples);
                    Array.Copy(outputData, i, outputTmp, 0, k_NumSamples);
                    AECInterop.WebRtcAec3_BufferFarend(m_AECHandle, outputTmp);
                    AECInterop.WebRtcAec3_Process(m_AECHandle, inputTmp, filterTmp);
                    filterBuffer.AddRange(filterTmp);
                }
            }
            byte[] byteArray = new byte[filterBuffer.Count * 2]; // Each short is 2 bytes
            Buffer.BlockCopy(filterBuffer.ToArray(), 0, byteArray, 0, filterBuffer.Count * 2);
            return byteArray;
        }
        
    }
}


/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inworld.AEC
{
    public class InworldAECAudioCapture : AudioCapture
    {
        const int k_NumSamples = 160;
        IntPtr m_AECHandle;

        float[] m_CharacterBuffer;
        List<short> m_CurrentPlayingWavData = new List<short>();
        Dictionary<string, InworldInteraction> m_SoundEnv = new Dictionary<string, InworldInteraction>();
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_AECHandle == IntPtr.Zero)
                return;
            AECInterop.WebRtcAec3_Free(m_AECHandle);
            m_AECHandle = IntPtr.Zero;
        }
        protected override void Init()
        {
            m_AECHandle = AECInterop.WebRtcAec3_Create(k_SampleRate);
            m_CurrentPlayingWavData = new List<short>();
            base.Init();
        }

        protected override byte[] Output(int nSize)
        {
            short[] inputBuffer = WavUtility.ConvertAudioClipDataToInt16Array(m_InputBuffer, nSize * m_Recording.channels);
            m_CurrentPlayingWavData.Clear();
            foreach (InworldInteraction interaction in m_SoundEnv.Values)
            {
                _Mix(interaction.GetCurrentAudioFragment());
            }
            return FilterAudio(inputBuffer, m_CurrentPlayingWavData.ToArray(), m_AECHandle);
        }
        void _Mix(short[] currAudio)
        {
            if (currAudio == null || currAudio.Length == 0)
                return;
            for (int i = 0; i < currAudio.Length; i++)
            {
                if (i < m_CurrentPlayingWavData.Count)
                    m_CurrentPlayingWavData[i] += currAudio[i];
                else
                    m_CurrentPlayingWavData.Add(currAudio[i]);
            }
        }
        public byte[] FilterAudio(short[] inputData, short[] outputData, IntPtr aecHandle)
        {
            short[] filteredAudio = new short[inputData.Length]; // Create a new array for filtered audio
            if (outputData == null || outputData.Length == 0 || outputData.Average(x => Mathf.Abs(x)) == 0)
            {
                filteredAudio = inputData;
            }
            else
            {
                int maxSamples = Math.Min(inputData.Length, outputData.Length) / k_NumSamples * k_NumSamples;
                for (int i = 0; i < maxSamples; i += k_NumSamples)
                {
                    AECInterop.WebRtcAec3_BufferFarend(aecHandle, outputData.Skip(i).Take(k_NumSamples).ToArray());
                    AECInterop.WebRtcAec3_Process(aecHandle, inputData.Skip(i).Take(k_NumSamples).ToArray(), filteredAudio.Skip(i).ToArray());
                }
            }
            byte[] byteArray = new byte[filteredAudio.Length * 2]; // Each short is 2 bytes
            Buffer.BlockCopy(filteredAudio, 0, byteArray, 0, filteredAudio.Length * 2);
            return byteArray;
        }
        public override bool EnableAEC => true;
        public override void RegisterLiveSession(string dataAgentId, InworldInteraction interaction)
        {
            m_SoundEnv[dataAgentId] = interaction;
        }
    }
}


/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

#if !UNITY_WEBGL
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inworld.Audio
{
    /// <summary>
    /// The base Sampling module for collecting InputBuffer.
    /// </summary>
    public class AudioCollectModule : InworldAudioModule, ICollectAudioHandler
    {
        [SerializeField] protected bool m_AutoReconnect = true;
        
        protected int m_LastPosition;
        protected int m_CurrPosition;

        public virtual int OnCollectAudio()
        {
            string deviceName = Audio.DeviceName;
            if (m_AutoReconnect && !Audio.IsMicRecording)
                Audio.StartMicrophone();
            AudioClip recClip = Audio.RecordingClip;
            if (!recClip)
                return -1;
            m_CurrPosition = Microphone.GetPosition(deviceName);
            if (m_CurrPosition < m_LastPosition)
                m_CurrPosition = recClip.samples;
            if (m_CurrPosition <= m_LastPosition)
                return -1;
            int nSize = m_CurrPosition - m_LastPosition;
            float[] rawInput = new float[nSize];
            if (!Audio.RecordingClip.GetData(rawInput, m_LastPosition))
                return -1;
            List<short> input = new List<short>();
            foreach (float sample in rawInput)
            {
                float clampedSample = Mathf.Clamp(sample, -1, 1);
                input.Add(Convert.ToInt16(clampedSample * short.MaxValue));
            }
            Audio.InputBuffer.Enqueue(new List<short>(input));

            m_LastPosition = m_CurrPosition % recClip.samples;
            return nSize;
        }
        
        public void ResetPointer() => m_LastPosition = m_CurrPosition = 0;
    }
}
#endif
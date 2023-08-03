/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace Inworld
{
    /// <summary>
    /// YAN: This is a global Audio Capture controller.
    ///      For each separate InworldCharacter, we use class AudioInteraction to handle audio clips.
    /// </summary>
    public class AudioCapture : MonoBehaviour
    {
        // ReSharper disable all InconsistentNaming
        public UnityEvent OnRecordingStart;
        public UnityEvent OnRecordingEnd;
        /// <summary>
        /// Signifies if microphone is capturing audio.
        /// </summary>
        public bool IsCapturing { get; set; }
        /// <summary>
        /// Signifies if user is speaking based on audio amplitud and threshold.
        /// </summary>
        public bool IsSpeaking { get; set; }
        [SerializeField] float  m_UserSpeechThreshold = 0.01f;
        [SerializeField] int m_AudioRate = 16000;
        [SerializeField] int m_BufferSeconds = 1;
        
        readonly List<string> m_AudioToPush = new List<string>();
        // Size of audioclip used to collect information, need to be big enough to keep up with collect. 
        int m_BufferSize;
        const int k_SizeofInt16 = sizeof(short);
        byte[] m_ByteBuffer;
        float[] m_FloatBuffer;
        AudioClip m_Recording;
        float m_CDCounter;
        // Last known position in AudioClip buffer.
        int m_LastPosition;

        #if !UNITY_WEBGL
        public void StartRecording()
        {
            m_LastPosition = Microphone.GetPosition(null);
            m_AudioToPush.Clear();
            IsCapturing = true;
            OnRecordingStart.Invoke();
        }
        public void StopRecording(bool needPush = false)
        {
            Microphone.End(null);
            if (needPush)
            {
                foreach (string audioData in m_AudioToPush)
                {
                    InworldController.Instance.SendAudio(audioData);
                }
            }
            m_AudioToPush.Clear();
            IsCapturing = false;
            OnRecordingEnd.Invoke();
        }
        void Awake()
        {
            m_BufferSize = m_BufferSeconds * m_AudioRate;
            m_ByteBuffer = new byte[m_BufferSize * 1 * k_SizeofInt16];
            m_FloatBuffer = new float[m_BufferSize * 1];
        }

        void Start()
        {
            m_Recording = Microphone.Start(null, true, m_BufferSeconds, m_AudioRate);
        }

        void Update()
        {
            if (!IsCapturing)
                return;
            if (!Microphone.IsRecording(null))
                StartRecording();
            if (m_CDCounter <= 0)
            {
                m_CDCounter = 0.1f;
                _Collect();
            }
            m_CDCounter -= Time.deltaTime;
        }
        void _Collect()
        {
            int nPosition = Microphone.GetPosition(null);
            if (nPosition < m_LastPosition)
                nPosition = m_BufferSize;
            if (nPosition <= m_LastPosition)
                return;
            int nSize = nPosition - m_LastPosition;
            if (!m_Recording.GetData(m_FloatBuffer, m_LastPosition))
                return;
            m_LastPosition = nPosition % m_BufferSize;
            WavUtility.ConvertAudioClipDataToInt16ByteArray(m_FloatBuffer, nSize * m_Recording.channels, m_ByteBuffer);
            int nWavCount = nSize * m_Recording.channels * k_SizeofInt16;
            byte[] output = new byte[nWavCount];
            Buffer.BlockCopy(m_ByteBuffer, 0, output, 0, nWavCount);
            InworldController.Instance.SendAudio(Convert.ToBase64String(output));
            // Check if player is speaking based on audio amplitude
            float amplitude = CalculateAmplitude(m_FloatBuffer);
            IsSpeaking = amplitude > m_UserSpeechThreshold;
        }

        // Helper method to calculate the amplitude of audio data
        float CalculateAmplitude(float[] audioData)
        {
            float sum = audioData.Sum(t => Mathf.Abs(t));
            return sum / audioData.Length;
        }

        void OnDestroy()
        {
            StopRecording();
        }
        public void PushAudio()
        {
            foreach (string audioData in m_AudioToPush)
            {
                InworldController.Instance.SendAudio(audioData);
            }
        }
        #endif

    }
}

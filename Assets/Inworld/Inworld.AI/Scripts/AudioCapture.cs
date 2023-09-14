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
        [SerializeField] float  m_UserSpeechThreshold = 0.01f;
        [SerializeField] protected int m_AudioRate = 16000;
        [SerializeField] int m_BufferSeconds = 1;
        [SerializeField] string m_DeviceName;
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
        /// <summary>
        /// Get/Set Audio Input Device Name for recording.
        /// </summary>
        public string DeviceName
        {
            get => m_DeviceName;
            set => m_DeviceName = value;
        }

        readonly List<string> m_AudioToPush = new List<string>();
        // Size of audioclip used to collect information, need to be big enough to keep up with collect. 
        protected int m_BufferSize;
        protected const int k_SizeofInt16 = sizeof(short);
        protected const int k_SizeofInt32 = sizeof(int);
        protected byte[] m_ByteBuffer;
        protected float[] m_InputBuffer;
        protected float[] m_OutputBuffer;
        protected AudioClip m_Recording;
        float m_CDCounter;
        // Last known position in AudioClip buffer.
        int m_LastPosition;

        public void StartRecording()
        {
#if !UNITY_WEBGL
            m_LastPosition = Microphone.GetPosition(null);
            m_AudioToPush.Clear();
            IsCapturing = true;
            if (!Microphone.IsRecording(m_DeviceName))
                m_Recording = Microphone.Start(m_DeviceName, true, m_BufferSeconds, m_AudioRate);
#endif
            OnRecordingStart.Invoke();
        }
        public void StopRecording(bool needPush = false)
        {
#if !UNITY_WEBGL
            Microphone.End(m_DeviceName);
            if (needPush)
            {
                foreach (string audioData in m_AudioToPush)
                {
                    InworldController.Instance.SendAudio(audioData);
                }
            }
#endif
            m_AudioToPush.Clear();
            IsCapturing = false;
            OnRecordingEnd.Invoke();
        }
        void Awake()
        {
            Init();
        }

#if !UNITY_WEBGL
        void Update()
        {
            if (!IsCapturing)
                return;
            if (!Microphone.IsRecording(m_DeviceName))
                StartRecording();
            if (m_CDCounter <= 0)
            {
                m_CDCounter = 0.1f;
                _Collect();
            }
            m_CDCounter -= Time.deltaTime;
        }
#endif
        void _Collect()
        {
            int nPosition = Microphone.GetPosition(m_DeviceName);
            if (nPosition < m_LastPosition)
                nPosition = m_BufferSize;
            if (nPosition <= m_LastPosition)
                return;
            int nSize = nPosition - m_LastPosition;
            if (!m_Recording.GetData(m_InputBuffer, m_LastPosition))
                return;
            m_LastPosition = nPosition % m_BufferSize;
            byte[] output = Output(nSize * m_Recording.channels);
            InworldController.Instance.SendAudio(Convert.ToBase64String(output));
            // Check if player is speaking based on audio amplitude
            float amplitude = CalculateAmplitude(m_InputBuffer);
            IsSpeaking = amplitude > m_UserSpeechThreshold;
        }

        protected virtual void Init()
        {
            m_BufferSize = m_BufferSeconds * m_AudioRate;
            m_ByteBuffer = new byte[m_BufferSize * 1 * k_SizeofInt16];
            m_InputBuffer = new float[m_BufferSize * 1];
            m_OutputBuffer = new float[m_BufferSeconds * 1];
        }
        protected virtual byte[] Output(int nSize)
        {
            WavUtility.ConvertAudioClipDataToInt16ByteArray(m_InputBuffer, nSize * m_Recording.channels, m_ByteBuffer);
            int nWavCount = nSize * m_Recording.channels * k_SizeofInt16;
            byte[] output = new byte[nWavCount];
            Buffer.BlockCopy(m_ByteBuffer, 0, output, 0, nWavCount);
            return output;
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

        public virtual void SamplePlayingWavData(float[] data, int channels)
        {

        }
    }
}

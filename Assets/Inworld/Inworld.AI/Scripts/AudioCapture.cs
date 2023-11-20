/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Interactions;
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
        [SerializeField] protected bool m_AutoPush = true;
        [SerializeField] protected float  m_UserSpeechThreshold = 0.01f;
        [SerializeField] protected int m_BufferSeconds = 1;
        [SerializeField] protected string m_DeviceName;

        public UnityEvent OnRecordingStart;
        public UnityEvent OnRecordingEnd;
        
        protected const int k_SizeofInt16 = sizeof(short);
        protected const int k_SampleRate = 16000;
        protected AudioClip m_Recording;
        protected bool m_IsPlayerSpeaking;
        protected bool m_IsCapturing;
        protected float m_CDCounter;
        // Last known position in AudioClip buffer.
        protected int m_LastPosition;
        // Size of audioclip used to collect information, need to be big enough to keep up with collect. 
        protected int m_BufferSize;
        protected readonly List<string> m_AudioToPush = new List<string>();
        protected byte[] m_ByteBuffer;
        protected float[] m_InputBuffer;

        /// <summary>
        /// Signifies if audio is currently blocked from being captured.
        /// </summary>
        public bool IsBlocked { get; set; }
        /// <summary>
        /// Signifies if microphone is capturing audio.
        /// </summary>
        public bool IsCapturing => m_IsCapturing;
        /// <summary>
        /// Signifies if audio should be pushed to server automatically as it is captured.
        /// </summary>
        public bool AutoPush
        {
            get => m_AutoPush;
            set => m_AutoPush = value;
        }
        /// <summary>
        /// Signifies if user is speaking based on audio amplitude and threshold.
        /// </summary>
        public bool IsPlayerSpeaking => m_IsPlayerSpeaking;
        /// <summary>
        /// Get Audio Input Device Name for recording.
        /// </summary>
        public string DeviceName => m_DeviceName;
        /// <summary>
        /// Get if aec is enabled. The parent class by default is false.
        /// </summary>
        public virtual bool EnableAEC => false;

#region Public Functions
        /// <summary>
        /// Change the device of microphone input.
        /// </summary>
        /// <param name="deviceName">the device name to input.</param>
        public void ChangeInputDevice(string deviceName)
        {
#if !UNITY_WEBGL
            if (deviceName == m_DeviceName)
                return;
            
            if (Microphone.IsRecording(m_DeviceName))
                StopMicrophone(m_DeviceName);

            m_DeviceName = deviceName;
            StartMicrophone(m_DeviceName);
#endif
        }
        /// <summary>
        /// Unity's official microphone module starts recording, will trigger OnRecordingStart event.
        /// </summary>
        public void StartRecording()
        {
#if !UNITY_WEBGL
            if (m_IsCapturing)
                return;
            m_LastPosition = Microphone.GetPosition(m_DeviceName);
            m_IsCapturing = true;
#endif
            OnRecordingStart.Invoke();
        }
        /// <summary>
        /// Unity's official microphone module stops recording, will trigger OnRecordingEnd event.
        /// </summary>
        public void StopRecording()
        {
#if !UNITY_WEBGL
            if (!m_IsCapturing)
                return;
            m_AudioToPush.Clear();
            m_IsCapturing = false;
#endif
            OnRecordingEnd.Invoke();
        }
        /// <summary>
        /// Manually push the audio wave data to server.
        /// </summary>
        public void PushAudio()
        {
#if !UNITY_WEBGL
            foreach (string audioData in m_AudioToPush)
            {
                InworldController.Instance.SendAudio(audioData);
            }
            m_AudioToPush.Clear();
#endif
        }
        /// <summary>
        /// Virtual function for sampling environment audios for echo cancellation.
        /// Would be implemented in the child class.
        /// </summary>
        /// <param name="data">the current environment audio.</param>
        /// <param name="channels">the channels of the environment audio.</param>
        public virtual void SamplePlayingWavData(float[] data, int channels)
        {

        }
#endregion

#region MonoBehaviour Functions
        protected virtual void Awake()
        {
            Init();
        }
        
        protected virtual void OnEnable()
        {
            StartMicrophone(m_DeviceName);
        }

        protected virtual void OnDisable()
        {
            StopRecording();
            StopMicrophone(m_DeviceName);
        }
#if !UNITY_WEBGL
        protected void Update()
        {
            if (!m_IsCapturing || IsBlocked)
                return;
            if (!Microphone.IsRecording(m_DeviceName))
                StartMicrophone(m_DeviceName);
            if (m_CDCounter <= 0)
            {
                m_CDCounter = 0.1f;
                Collect();
            }
            m_CDCounter -= Time.unscaledDeltaTime;
        }
#endif
        protected virtual void OnDestroy()
        {
            StopRecording();
            StopMicrophone(m_DeviceName);
        }
#endregion

#region Protected Functions
        protected virtual void Init()
        {
            m_BufferSize = m_BufferSeconds * k_SampleRate;
            m_ByteBuffer = new byte[m_BufferSize * 1 * k_SizeofInt16];
            m_InputBuffer = new float[m_BufferSize * 1];
        }
        protected virtual void Collect()
        {
#if !UNITY_WEBGL
            int nSize = GetAudioData();
            if (nSize <= 0)
                return;
            byte[] output = Output(nSize * m_Recording.channels);
            string audioData = Convert.ToBase64String(output);
            if(m_AutoPush)
                InworldController.Instance.SendAudio(audioData);
            else
                m_AudioToPush.Add(audioData);
            // Check if player is speaking based on audio amplitude
            float amplitude = CalculateAmplitude(m_InputBuffer);
            m_IsPlayerSpeaking = amplitude > m_UserSpeechThreshold;
#endif            
        }
        protected int GetAudioData()
        {
#if UNITY_WEBGL
            return -1;
#else
            int nPosition = Microphone.GetPosition(m_DeviceName);
            if (nPosition < m_LastPosition)
                nPosition = m_BufferSize;
            if (nPosition <= m_LastPosition)
                return -1;
            int nSize = nPosition - m_LastPosition;
            if (!m_Recording.GetData(m_InputBuffer, m_LastPosition))
                return -1;
            m_LastPosition = nPosition % m_BufferSize;
            return nSize;
#endif
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
        protected float CalculateAmplitude(float[] audioData)
        {
            float sum = audioData.Sum(t => Mathf.Abs(t));
            return sum / audioData.Length;
        }
        protected void StartMicrophone(string deviceName)
        {
#if !UNITY_WEBGL
            m_Recording = Microphone.Start(deviceName, true, m_BufferSeconds, k_SampleRate);
#endif
        }
        protected void StopMicrophone(string deviceName)
        {
#if !UNITY_WEBGL
            Microphone.End(deviceName);
#endif
        }
#endregion
    }
}

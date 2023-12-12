/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;


namespace Inworld
{
    /// <summary>
    /// YAN: This is a global Audio Capture controller.
    ///      For each separate InworldCharacter, we use class AudioInteraction to handle audio clips.
    /// </summary>
    public class AudioCapture : MonoBehaviour
    {
        [SerializeField] protected MicSampleMode m_SamplingMode;
        [Range(1, 2)][SerializeField] protected float m_PlayerVolumeThreshold = 2f;
        [SerializeField] protected int m_BufferSeconds = 1;
        [SerializeField] protected string m_DeviceName;

        public UnityEvent OnRecordingStart;
        public UnityEvent OnRecordingEnd;

        public UnityEvent OnPlayerStartSpeaking;
        public UnityEvent OnPlayerStopSpeaking;
        protected MicSampleMode m_LastSampleMode;
        protected const int k_SizeofInt16 = sizeof(short);
        protected const int k_SampleRate = 16000;
        protected AudioClip m_Recording;
        protected IEnumerator m_AudioCoroutine;
        protected bool m_IsPlayerSpeaking;
        protected bool m_IsCapturing;
        protected float m_BackgroundNoise;
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
            get => m_SamplingMode != MicSampleMode.NO_MIC && m_SamplingMode != MicSampleMode.PUSH_TO_TALK;
            set
            {
                if (value)
                {
                    if (m_SamplingMode == MicSampleMode.PUSH_TO_TALK)
                        m_SamplingMode = m_LastSampleMode;
                }
                else
                {
                    if (m_SamplingMode != MicSampleMode.PUSH_TO_TALK)
                        m_LastSampleMode = m_SamplingMode;
                    m_SamplingMode = MicSampleMode.PUSH_TO_TALK;
                }
            }
        }
        /// <summary>
        /// A flag to check if player is allowed to speak and without filtering
        /// </summary>
        public bool IsPlayerTurn => 
            m_SamplingMode == MicSampleMode.NO_FILTER || 
            m_SamplingMode == MicSampleMode.PUSH_TO_TALK ||
            m_SamplingMode == MicSampleMode.TURN_BASED && !InworldController.CharacterHandler.IsAnyCharacterSpeaking;

        /// <summary>
        /// A flag to check if audio is available to send to server.
        ///     (Either Enable AEC or it's Player's turn to speak)
        /// </summary>
        public bool IsAudioAvailable => m_SamplingMode == MicSampleMode.AEC || IsPlayerTurn;

        /// <summary>
        /// Signifies if user is speaking based on audio amplitude and threshold.
        /// </summary>
        public bool IsPlayerSpeaking
        {
            get => m_IsPlayerSpeaking;
            set
            {
                if (m_IsPlayerSpeaking == value)
                    return;
                m_IsPlayerSpeaking = value;
                if (m_IsPlayerSpeaking)
                    OnPlayerStartSpeaking.Invoke();
                else
                    OnPlayerStopSpeaking.Invoke();
            }
        }
        /// <summary>
        /// Get the background noises, including music.
        /// </summary>
        public float BackgroundNoise => m_BackgroundNoise;
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
        ///     Aync to Calculate the background noise (including bg music, etc)
        ///     Please call it whenever audio environment changed in your game.
        /// </summary>
        /// <returns></returns>
        public IEnumerator Calibrate()
        {
#if !UNITY_WEBGL
            if (!Microphone.IsRecording(m_DeviceName))
                StartMicrophone(m_DeviceName);
            while (m_BackgroundNoise == 0)
            {
                GetAudioData();
                yield return new WaitForSeconds(0.1f);
                m_BackgroundNoise = CalculateAmplitude();
            }
#endif
            yield break;
        }
#endregion

#region MonoBehaviour Functions
        protected virtual void Awake()
        {
            Init();
        }
        
        protected virtual void OnEnable()
        {
            m_AudioCoroutine = AudioCoroutine();
            StartCoroutine(m_AudioCoroutine);
            
        }
        protected virtual IEnumerator AudioCoroutine()
        {
#if !UNITY_WEBGL
            while (true)
            {
                yield return Calibrate();
                if (!m_IsCapturing || IsBlocked)
                {
                    yield return null;
                    continue;
                }
                yield return Collect();
            }
#endif
        }

        protected virtual void OnDisable()
        {
            StopCoroutine(m_AudioCoroutine);
            StopRecording();
            StopMicrophone(m_DeviceName);
        }

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

        protected virtual IEnumerator Collect()
        {
#if !UNITY_WEBGL
            if (m_SamplingMode == MicSampleMode.NO_MIC)
                yield break;
            if (m_SamplingMode != MicSampleMode.PUSH_TO_TALK && m_BackgroundNoise == 0)
                yield break;
            int nSize = GetAudioData();
            if (nSize <= 0)
                yield break;
            IsPlayerSpeaking = CalculateAmplitude() > m_BackgroundNoise * m_PlayerVolumeThreshold;
            byte[] output = Output(nSize * m_Recording.channels);
            string audioData = Convert.ToBase64String(output);
            if(AutoPush)
                InworldController.Instance.SendAudio(audioData);
            else
                m_AudioToPush.Add(audioData);
            yield return new WaitForSeconds(0.1f);
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
        protected float CalculateAmplitude()
        {
            float fAvg = 0;
            int nCount = 0;
            foreach (float t in m_InputBuffer)
            {
                float tmp = Mathf.Abs(t);
                if (tmp == 0)
                    continue;
                fAvg += tmp;
                nCount++;
            }
            return nCount == 0 ? 0 : fAvg / nCount;
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

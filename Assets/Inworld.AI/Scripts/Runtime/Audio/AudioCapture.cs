/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Google.Protobuf;
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
        public UnityEvent OnRecordingStart;
        public UnityEvent OnRecordingEnd;

        [SerializeField] protected int m_AudioRate = 16000;
        [SerializeField] protected int m_BufferSeconds = 1;
        [SerializeField] protected bool m_AutoPush = true;
        protected readonly List<ByteString> m_AudioToPush = new List<ByteString>();
        // Size of audioclip used to collect information, need to be big enough to keep up with collect. 
        protected int m_BufferSize;
        protected const int k_SizeofInt16 = sizeof(short);
        protected bool m_IsCapturing;
        protected byte[] m_ByteBuffer;
        protected float[] m_FloatBuffer;
        protected AudioClip m_Recording;
        protected float m_CDCounter;
        // Last known position in AudioClip buffer.
        int m_LastPosition;
        protected string m_CurrentDevice = null;
        /// <summary>
        ///     Whether audio is currently blocked from being captured.
        /// </summary>
        public bool IsBlocked { get; set; }
        /// <summary>
        ///     Whether audio is currently being captured.
        /// </summary>
        public bool IsCapturing => m_IsCapturing;
        /// <summary>
        ///     Wether to audio should be pushed to server automatically as it is captured.
        /// </summary>
        public bool AutoPush
        {
	        get => m_AutoPush;
	        set => m_AutoPush = value;
        }
        public void SwitchAudioInputDevice(string deviceName)
        {
	        if(m_CurrentDevice == deviceName)
		        return;

	        if (Microphone.IsRecording(m_CurrentDevice))
		        Microphone.End(m_CurrentDevice);

	        m_CurrentDevice = deviceName;
	        m_Recording = Microphone.Start(m_CurrentDevice, true, m_BufferSeconds, m_AudioRate);
        }
        internal void StartRecording()
        {
	        if (m_IsCapturing)
		        return;
            m_LastPosition = Microphone.GetPosition(m_CurrentDevice);
            m_AudioToPush.Clear();
            m_IsCapturing = true;
            OnRecordingStart.Invoke();
        }
        internal void StopRecording(bool pushAudio = false)
        {
            if (!m_IsCapturing)
	            return;
            if (pushAudio)
	            PushAudio();
            else
	            m_AudioToPush.Clear();
            m_IsCapturing = false;
            OnRecordingEnd.Invoke();
        }
        protected virtual void Awake()
        {
            m_BufferSize = m_BufferSeconds * m_AudioRate;
            m_ByteBuffer = new byte[m_BufferSize * 1 * k_SizeofInt16];
            m_FloatBuffer = new float[m_BufferSize * 1];
        }
        protected virtual void OnEnable()
        {
	        m_Recording = Microphone.Start(m_CurrentDevice, true, m_BufferSeconds, m_AudioRate);
        }

        protected virtual void OnDisable()
        {
	        Microphone.End(m_CurrentDevice);
        }
        protected virtual void Start()
        {
            m_Recording = Microphone.Start(m_CurrentDevice, true, m_BufferSeconds, m_AudioRate);
        }
        protected virtual void Update()
        {
            if (!m_IsCapturing || IsBlocked)
                return;
            if (!Microphone.IsRecording(m_CurrentDevice))
	            m_Recording = Microphone.Start(m_CurrentDevice, true, m_BufferSeconds, m_AudioRate);
            if (m_CDCounter <= 0)
            {
                m_CDCounter = 0.1f;
                Collect();
            }
            m_CDCounter -= Time.unscaledDeltaTime;
        }
        void OnDestroy()
        {
	        Microphone.End(m_CurrentDevice);
        }
        protected virtual int GetAudioData()
        {
            int nPosition = Microphone.GetPosition(m_CurrentDevice);
            if (nPosition < m_LastPosition)
                nPosition = m_BufferSize;
            if (nPosition <= m_LastPosition)
                return -1;
            int nSize = nPosition - m_LastPosition;
            if (!m_Recording.GetData(m_FloatBuffer, m_LastPosition))
                return -1;
            m_LastPosition = nPosition % m_BufferSize;
            return nSize;
        }
        protected virtual void Collect()
        {
            int nSize = GetAudioData(); // YAN: Out to m_FloatBuffer.
            if (nSize < 0)
                return;
            WavUtility.ConvertAudioClipDataToInt16ByteArray(m_FloatBuffer, nSize * m_Recording.channels, m_ByteBuffer);
            ByteString audioData = ByteString.CopyFrom(m_ByteBuffer, 0, nSize * m_Recording.channels * k_SizeofInt16);
            if (m_AutoPush)
                InworldController.Instance.SendAudio(audioData);
            else
                m_AudioToPush.Add(audioData);
        }
        internal void PushAudio() 
        {
            foreach (ByteString audioData in m_AudioToPush)
            {
                InworldController.Instance.SendAudio(audioData);
            }
            m_AudioToPush.Clear();
        }
    }
}

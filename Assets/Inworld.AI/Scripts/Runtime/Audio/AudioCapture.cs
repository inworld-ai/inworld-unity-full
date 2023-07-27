/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Google.Protobuf;
using Inworld.Util;
using System;
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
        public bool IsCapturing { get; set; }
        [SerializeField] protected int m_AudioRate = 16000;
        [SerializeField] protected int m_BufferSeconds = 1;
        
        readonly List<ByteString> m_AudioToPush = new List<ByteString>();
        // Size of audioclip used to collect information, need to be big enough to keep up with collect. 
        int m_BufferSize;
        const int k_SizeofInt16 = sizeof(short);
        byte[] m_ByteBuffer;
        protected float[] m_FloatBuffer;
        protected AudioClip m_Recording;
        float m_CDCounter;
        // Last known position in AudioClip buffer.
        int m_LastPosition;
        bool m_AutoPush;
        protected string m_CurrentDevice = null;

        public void StartRecording(bool autoPush = true)
        {
            if (!Microphone.IsRecording(m_CurrentDevice))
                m_Recording = Microphone.Start(m_CurrentDevice, true, m_BufferSeconds, m_AudioRate);
            m_LastPosition = Microphone.GetPosition(null);
            m_AudioToPush.Clear();
            IsCapturing = true;
            m_AutoPush = autoPush;
            OnRecordingStart.Invoke();
        }
        public void StopRecording()
        {
            Microphone.End(null);
            m_AudioToPush.Clear();
            IsCapturing = false;
            m_AutoPush = true;
            OnRecordingEnd.Invoke();
        }
        protected virtual void Awake()
        {
            m_BufferSize = m_BufferSeconds * m_AudioRate;
            m_ByteBuffer = new byte[m_BufferSize * 1 * k_SizeofInt16];
            m_FloatBuffer = new float[m_BufferSize * 1];
        }
        protected virtual void Start()
        {
            m_Recording = Microphone.Start(m_CurrentDevice, true, m_BufferSeconds, m_AudioRate);
        }
        void Update()
        {
            if (!IsCapturing)
                return;
            if (!Microphone.IsRecording(m_CurrentDevice))
                StartRecording();
            if (m_CDCounter <= 0)
            {
                m_CDCounter = 0.1f;
                Collect();
            }
            m_CDCounter -= Time.deltaTime;
        }
        void OnDestroy()
        {
            StopRecording();
        }
        protected int GetAudioData()
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
        public void PushAudio() 
        {
            foreach (ByteString audioData in m_AudioToPush)
            {
                InworldController.Instance.SendAudio(audioData);
            }
        }
    }
}

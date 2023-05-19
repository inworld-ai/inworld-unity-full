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
        [SerializeField] int m_AudioRate = 16000;
        [SerializeField] int m_BufferSeconds = 1;
        
        readonly List<ByteString> m_AudioToPush = new List<ByteString>();
        // Size of audioclip used to collect information, need to be big enough to keep up with collect. 
        int m_BufferSize;
        const int k_SizeofInt16 = sizeof(short);
        byte[] m_ByteBuffer;
        float[] m_FloatBuffer;
        AudioClip m_Recording;
        float m_CDCounter;
        // Last known position in AudioClip buffer.
        int m_LastPosition;
        bool m_AutoPush;

        public void StartRecording(bool autoPush = true)
        {
            if (!Microphone.IsRecording(null))
                m_Recording = Microphone.Start(null, true, m_BufferSeconds, m_AudioRate);
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
        void OnDestroy()
        {
            StopRecording();
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

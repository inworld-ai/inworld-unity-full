/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Inworld.Audio.AEC
{
    public class AcousticEchoCanceler : InworldAudioModule, ICollectAudioHandler, IProcessAudioHandler
    {
        [SerializeField] bool m_AutoReconnect;
        const int k_SampleRate = 16000;
        const int k_NumSamples = 160;
        IntPtr m_AECHandle;
        ConcurrentQueue<short> m_InputBuffer = new ConcurrentQueue<short>();
        ConcurrentQueue<short> m_OutputBuffer = new ConcurrentQueue<short>();
        int m_OutputSampleRate;
        int m_OutputChannels;
        AECProbe m_FarendProbe;
        AECProbe m_NearendProbe;
        bool m_IsAudioDebugging;
        InputAction m_DumpAudioAction;
        List<short> m_DebugInput = new List<short>();
        List<short> m_DebugOutput = new List<short>();
        List<short> m_DebugFilter = new List<short>();
        /// <summary>
        ///     Check Available, will add mac support in the next update.
        ///     For mobile device such as Android/iOS they naturally supported via hardware.
        /// </summary>
        public bool IsAvailable => Application.platform == RuntimePlatform.WindowsPlayer 
                                   || Application.platform == RuntimePlatform.WindowsEditor
                                   || Application.platform == RuntimePlatform.OSXEditor
                                   || Application.platform == RuntimePlatform.OSXPlayer;
        public CircularBuffer<short> ProcessedBuffer { get; set; }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void OnEnable()
        {
            AudioConfiguration audioSetting = AudioSettings.GetConfiguration();
            m_OutputSampleRate = audioSetting.sampleRate;
            m_OutputChannels = audioSetting.speakerMode == AudioSpeakerMode.Stereo ? 2 : 1;
            ProcessedBuffer = new CircularBuffer<short>(k_SampleRate);
            enabled = IsAvailable 
                      && InitProbe<AudioListener>(ref m_FarendProbe, SignalEnd.FarEnd) 
                      && InitProbe<InworldAudioCapture>(ref m_NearendProbe, SignalEnd.NearEnd);
            if (enabled)
            {
                m_AECHandle = AECInterop.WebRtcAec3_Create(k_SampleRate);
                m_DumpAudioAction = InworldAI.InputActions["DumpAudio"];
            }
                
            else
            {
                //Fallback to turn based.
            }
        }
        void Update()
        {
            m_IsAudioDebugging = m_DumpAudioAction.IsPressed();
        }
        void OnDestroy()
        {
            if (!IsAvailable)
                return;
            AECInterop.WebRtcAec3_Free(m_AECHandle);
            m_AECHandle = IntPtr.Zero;
        }
        bool InitProbe<T>(ref AECProbe probe, SignalEnd end) where T : Behaviour
        {
            if (probe)
                return probe;
            T listener = FindFirstObjectByType<T>();
            if (!listener)
            {
                InworldAI.LogError($"Cannot Find {typeof(T).Name}");
                return false;
            }
            probe = listener.gameObject.GetComponent<AECProbe>();
            if (!probe)
                probe = listener.gameObject.AddComponent<AECProbe>();
            probe.end = end;
            return probe;
        }
        public void GetOutputData(SignalEnd end, float[] data, int channels)
        {
            if (end == SignalEnd.FarEnd)
                WavUtility.ConvertAudioClipDataToInt16Array(ref m_OutputBuffer, data, m_OutputSampleRate, channels);
            else
                WavUtility.ConvertAudioClipDataToInt16Array(ref m_InputBuffer, data, m_OutputSampleRate, channels);
        }
        public bool OnPreProcessAudio()
        {
            return true;
        }

        public bool OnPostProcessAudio()
        {
            while (m_InputBuffer.Count >= k_NumSamples && m_OutputBuffer.Count >= k_NumSamples)
            {
                short[] inputData = new short[k_NumSamples];
                short[] outputData = new short[k_NumSamples];
                for (int i = 0; i < k_NumSamples; i++)
                {
                    m_InputBuffer.TryDequeue(out inputData[i]);
                    m_OutputBuffer.TryDequeue(out outputData[i]);
                }
                FilterAudio(inputData, outputData);
            }
            return true;
        }
        public int OnCollectAudio()
        {
            string deviceName = Audio.DeviceName;
            if (m_AutoReconnect && !Audio.IsMicRecording)
                Audio.StartMicrophone();
            return 1;
        }
        public void ResetPointer()
        {
            
        }
        void FilterAudio(short[] inputData, short[] outputData)
        {
            short[] filterTmp = new short[k_NumSamples];
            
            if (m_AECHandle == IntPtr.Zero)
            {
                m_AECHandle = AECInterop.WebRtcAec3_Create(k_SampleRate);
            }
            AECInterop.WebRtcAec3_BufferFarend(m_AECHandle, outputData);
            AECInterop.WebRtcAec3_Process(m_AECHandle, inputData, filterTmp);

            Audio.ProcessedWaveData.Enqueue(filterTmp.ToList());

            if (!m_IsAudioDebugging)
                return;
            m_DebugInput.AddRange(inputData);
            m_DebugFilter.AddRange(filterTmp);
            if (outputData != null)
                m_DebugOutput.AddRange(outputData);
        }
    }
}


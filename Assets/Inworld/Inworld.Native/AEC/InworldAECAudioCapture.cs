/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Entities;
using Inworld.Audio.VAD;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

namespace Inworld.Audio.AEC
{
    public class InworldAECAudioCapture : AudioCapture
    {
        const int k_NumSamples = 160;
        //TODO(Yan): Replace directly with Sentis when it supports IF condition.
        const string k_SourceFilePath = "Inworld/Inworld.Native/VAD/Plugins";
        const string k_TargetFileName =  "silero_vad.onnx";
        
        AECProbe m_Probe;
        bool m_IsAudioDebugging = false;
        IntPtr m_AECHandle;
        protected ConcurrentQueue<short> m_OutputBuffer = new ConcurrentQueue<short>();
        protected InputAction m_DumpAudioAction;
        protected float m_AECTimer = 5;
        protected bool m_AudioFilterStarted;
        [Range(0, 10000)][SerializeField] protected int m_OutputBufferOffset = 4800;
        [Range(1, 10)][SerializeField] float m_AECResetCountDown = 5f;
        
#region Debug Dump Audio
        List<short> m_DebugOutput = new List<short>();
        List<short> m_DebugInput = new List<short>();
        List<short> m_DebugFilter = new List<short>();
#endregion


        /// <summary>
        /// Get the AECProbe.
        /// Will create one and attach to AudioListener if not existed.
        /// </summary>
        public AECProbe Probe
        {
            get
            {
                if (!IsAvailable)
                    return null;
                if (m_Probe)
                    return m_Probe;
                AudioListener listener = FindFirstObjectByType<AudioListener>();
                if (!listener)
                {
                    InworldAI.LogError("Cannot Find Audio Listener!");
                    return m_Probe;
                }
                m_Probe = listener.gameObject.GetComponent<AECProbe>();
                if (!m_Probe)
                    m_Probe = listener.gameObject.AddComponent<AECProbe>();
                return m_Probe;
            }
        }

        /// <summary>
        /// A flag for this component is using AEC
        /// </summary>
        public override bool EnableAEC => IsAvailable && !IsPlayerTurn;
        /// <summary>
        /// A flag for this component is using VAD
        /// Currently we only support Windows, will support all platforms when Sentis is ready.
        /// </summary>
        public override bool EnableVAD => Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor;
        /// <summary>
        ///     Check Available, will add mac support in the next update.
        ///     For mobile device such as Android/iOS they naturally supported via hardware.
        /// </summary>
        public bool IsAvailable => Application.platform == RuntimePlatform.WindowsPlayer 
                                   || Application.platform == RuntimePlatform.WindowsEditor
                                   || Application.platform == RuntimePlatform.OSXEditor
                                   || Application.platform == RuntimePlatform.OSXPlayer;

        /// <summary>
        /// Get the audio data from the AudioListener.
        /// Need AECProbe attached to the AudioListener first.
        /// </summary>
        /// <param name="data">the output data</param>
        /// <param name="channels">the channels</param>
        public override void GetOutputData(float[] data, int channels)
        {
            if (!m_AudioFilterStarted)
                return;
#if !UNITY_WEBGL
            WavUtility.ConvertAudioClipDataToInt16Array(ref m_OutputBuffer, data, m_OutputSampleRate, channels);
#endif
        }

        public override bool StartMicrophone(string deviceName)
        {
            if (base.StartMicrophone(deviceName))
            {
                m_InputBuffer.Clear();
                m_OutputBuffer.Clear();
                m_AudioFilterStarted = false;
                for(int i = 0; i < m_OutputBufferOffset; i++)
                    m_OutputBuffer.Enqueue(0);
                Recording.Play();
                return true;
            }
            return false;
        }
        protected override int GetAudioData()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (WebGLIsRecording() == 0)
                StartMicrophone(m_DeviceName);
            m_nPosition = WebGLGetPosition();
            if (m_nPosition < m_LastPosition)
                m_nPosition = Recording.clip.samples;
            if (m_nPosition <= m_LastPosition)
            {
                return -1;
            }
            int nSize = m_nPosition - m_LastPosition;
            if (!WebGLGetAudioData())
                return -1;
            m_LastPosition = m_nPosition % Recording.clip.samples;
            return nSize;
#else
            if (!Microphone.IsRecording(m_DeviceName))
                StartMicrophone(m_DeviceName);
            return 1;
#endif
        }
        protected override void ProcessAudio()
        {
            if (!Probe || !Probe.enabled)
            {
                base.ProcessAudio();
                return;
            }
            m_PlayerVolumeCheckBuffer.Clear();
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
            if (EnableAEC)
            {
                if (!IsPlayerSpeaking && m_AECTimer < 0)
                {
                    if (m_AECHandle != IntPtr.Zero)
                    {
                        AECInterop.WebRtcAec3_Free(m_AECHandle);
                        m_AECHandle = IntPtr.Zero;
                    }
                    m_AECTimer = m_AECResetCountDown;
                }
                m_AECTimer -= 0.1f;
            }
            RemoveOverDueData(ref m_ProcessedWaveData, k_SampleRate);
        }
        /// <summary>
        /// Call it when you switch AudioListener(mostly Main Camera)
        /// </summary>
        public void SendProbeToAudioListener()
        {
            // TODO(Yan): Mark Send Probe.
            // if (IsAvailable)
            //     Probe.Init(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_AECHandle == IntPtr.Zero)
                return;
            if (EnableVAD)
            {
                VADInterop.VAD_Terminate();
            }
            AECInterop.WebRtcAec3_Free(m_AECHandle);
            m_AECHandle = IntPtr.Zero;
        }
        protected new void Update()
        {
            m_IsAudioDebugging = m_DumpAudioAction != null && m_DumpAudioAction.IsPressed();

            if (!m_IsAudioDebugging)
            {
                _DumpAudioFiles();
            }
            if (m_AudioToPush.Count > m_AudioToPushCapacity)
                m_AudioToPush.TryDequeue(out AudioChunk chunk);
        }
        // YAN: Currently if you'd like to use AEC. The Audio Setting for output has to be 16000 sample rate, and mono.
        //      We'll add resampling features in the next update.
        protected override void Init()
        {
            SendProbeToAudioListener();
            if (IsAvailable)
                m_AECHandle = AECInterop.WebRtcAec3_Create(k_SampleRate);
            else
                m_SamplingMode = MicSampleMode.TURN_BASED;
            if (EnableVAD)
#if UNITY_EDITOR
                VADInterop.VAD_Initialize($"{Application.dataPath}/{k_SourceFilePath}/{k_TargetFileName}");
#else
                VADInterop.VAD_Initialize($"{Application.streamingAssetsPath}/{k_TargetFileName}");
#endif
            m_PrevSampleMode = m_SamplingMode;
            m_DumpAudioAction = InworldAI.InputActions["DumpAudio"];

            Recording.loop = true;
            base.Init();
        }
        protected override bool DetectPlayerSpeaking()
        {
            if (!EnableVAD)
                return base.DetectPlayerSpeaking();
            float[] processedWave = WavUtility.ConvertInt16ArrayToFloatArray(m_PlayerVolumeCheckBuffer.ToArray());
            float vadResult = VADInterop.VAD_Process(processedWave, processedWave.Length);
            return !IsMute && AutoDetectPlayerSpeaking && vadResult * 30 > m_PlayerVolumeThreshold;
        }
        void _DumpAudioFiles()
        {
            if (m_DebugFilter.Count == 0 || m_DebugInput.Count == 0 || m_DebugOutput.Count == 0)
                return;
            WavUtility.ShortArrayToWavFile(m_DebugInput.ToArray(), "DebugInput.wav");
            WavUtility.ShortArrayToWavFile(m_DebugOutput.ToArray(), "DebugOutput.wav");
            WavUtility.ShortArrayToWavFile(m_DebugFilter.ToArray(), "DebugFilter.wav");
            m_DebugFilter.Clear();
            m_DebugInput.Clear();
            m_DebugOutput.Clear();
        }

        protected void FilterAudio(short[] inputData, short[] outputData)
        {
            short[] filterTmp = new short[k_NumSamples];
            if (outputData == null || outputData.Length == 0 || !EnableAEC)
            {
                m_PlayerVolumeCheckBuffer.AddRange(inputData);
                m_ProcessedWaveData.AddRange(inputData);
            }
            else
            {
                if (m_AECHandle == IntPtr.Zero)
                {
                    m_AECHandle = AECInterop.WebRtcAec3_Create(k_SampleRate);
                }
                AECInterop.WebRtcAec3_BufferFarend(m_AECHandle, outputData);
                AECInterop.WebRtcAec3_Process(m_AECHandle, inputData, filterTmp);
                m_PlayerVolumeCheckBuffer.AddRange(filterTmp);
                m_ProcessedWaveData.AddRange(filterTmp);
            }
            if (m_IsAudioDebugging)
            {
                m_DebugInput.AddRange(inputData);
                m_DebugFilter.AddRange(filterTmp);
                if (outputData != null)
                    m_DebugOutput.AddRange(outputData);
            }
        }
        
        void OnAudioFilterRead(float[] data, int channels)
        {
            if (!m_AudioFilterStarted)
            {
                m_AudioFilterStarted = true;
                return;
            }
            WavUtility.ConvertAudioClipDataToInt16Array(ref m_InputBuffer, data, m_OutputSampleRate, channels);
        }
    }
}


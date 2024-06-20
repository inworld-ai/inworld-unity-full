/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inworld.AEC
{
    public class InworldAECAudioCapture : AudioCapture
    {
        [Tooltip("Hold the key to sample, release the key to save to local files")]
        [SerializeField] KeyCode m_DumpAudioHotKey = KeyCode.None; 
        AECProbe m_Probe;
        bool m_IsAudioDebugging = false;
        const int k_NumSamples = 160;
        IntPtr m_AECHandle;

        protected List<short> m_OutputBuffer = new List<short>();
        
#region Debug Dump Audio
        List<short> m_DebugOutput = new List<short>();
        List<short> m_DebugInput = new List<short>();
        List<short> m_DebugFilter = new List<short>();
#endregion
        
        public AECProbe Probe
        {
            get
            {
                if (m_Probe)
                    return m_Probe;
                AudioListener listener = FindObjectOfType<AudioListener>();
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
        /// A flag for this component is using AEC (in this class always True)
        /// </summary>
        public override bool EnableAEC => IsAvailable && !IsPlayerTurn;

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
            PreProcessAudioData(ref m_OutputBuffer, data, channels, false);
        }
        protected override void ProcessAudio()
        {
            while (m_InputBuffer.Count > k_NumSamples && m_OutputBuffer.Count > k_NumSamples)
            {
                FilterAudio(m_InputBuffer.Take(k_NumSamples).ToArray(), m_OutputBuffer.Take(k_NumSamples).ToArray());
                m_InputBuffer.RemoveRange(0, k_NumSamples);
                m_OutputBuffer.RemoveRange(0, k_NumSamples);
            }
            RemoveOverDueData(ref m_InputBuffer);
            RemoveOverDueData(ref m_OutputBuffer);
            RemoveOverDueData(ref m_ProcessedWaveData);
        }
        /// <summary>
        /// Call it when you switch AudioListener(mostly Main Camera)
        /// </summary>
        public void SendProbeToAudioListener()
        {
            Probe.Init(this);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_AECHandle == IntPtr.Zero)
                return;
            AECInterop.WebRtcAec3_Free(m_AECHandle);
            m_AECHandle = IntPtr.Zero;
        }
        protected new void Update()
        {
            m_IsAudioDebugging = Input.GetKey(m_DumpAudioHotKey);
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
            {
                m_AECHandle = AECInterop.WebRtcAec3_Create(k_SampleRate);
            }
            else
                m_SamplingMode = MicSampleMode.TURN_BASED;
            m_InitSampleMode = m_SamplingMode;
            base.Init();
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
                m_ProcessedWaveData.AddRange(inputData);
            }
            else
            {
                
                AECInterop.WebRtcAec3_BufferFarend(m_AECHandle, outputData);
                AECInterop.WebRtcAec3_Process(m_AECHandle, inputData, filterTmp);
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
    }
}


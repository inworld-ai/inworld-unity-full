/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using CSCore;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.Streams;
using Inworld.AEC;
using Inworld.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inworld.Audio
{
    public class WinAudioInputCapture : AudioCapture
    {
        [Tooltip("Hold the key to sample, release the key to save to local files")]
        [SerializeField] KeyCode m_DumpAudioHotKey = KeyCode.None; 
        const int k_NumSamples = 160;
        int m_OutputSampleRate = k_SampleRate;
        int m_OutputChannels = k_Channel;

        WaveFormat m_TargetFormat = new WaveFormat(k_SampleRate, 16, 1);
        List<short> m_InputData = new List<short>();
        List<short> m_OutputData = new List<short>();
        List<short> m_FilterData = new List<short>();
        
#region Debug Dump Audio
        List<short> m_DebugOutput = new List<short>();
        List<short> m_DebugInput = new List<short>();
        List<short> m_DebugFilter = new List<short>();
#endregion
        IntPtr m_AECHandle;
        
        /// <summary>
        /// A flag for this component is using AEC (in this class always True)
        /// </summary>
        public override bool EnableAEC => IsAvailable && !IsPlayerTurn;

        /// <summary>
        ///     Check Available, will add mac support in the next update.
        ///     For mobile device such as Android/iOS they naturally supported via hardware.
        /// </summary>
        public bool IsAvailable => Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor;
        
        protected WasapiCapture m_InputCapture;
        protected WasapiLoopbackCapture m_OutputCapture;
        protected override void Init()
        {
            if (IsAvailable)
            {
                AudioConfiguration audioSetting = AudioSettings.GetConfiguration();
                m_OutputSampleRate = audioSetting.sampleRate;
                m_OutputChannels = audioSetting.speakerMode == AudioSpeakerMode.Stereo ? 2 : 1;
                m_AECHandle = AECInterop.WebRtcAec3_Create(k_SampleRate);
            }
            else
                m_SamplingMode = MicSampleMode.TURN_BASED;
            m_InitSampleMode = m_SamplingMode;
        }
        protected override void OnEnable()
        {
            m_InputCapture = new WasapiCapture();
            InitializeCapture(m_InputCapture, m_InputData);
            if (!IsAvailable)
                return;
            m_OutputCapture = new WasapiLoopbackCapture();
            InitializeCapture(m_OutputCapture, m_OutputData);
            base.OnEnable();
        }
        protected override void OnDisable()
        {
            StopCapture(m_InputCapture, m_InputData);
            StopCapture(m_OutputCapture, m_OutputData);
            StopCoroutine(m_AudioCoroutine);
        }
        // YAN: Instead of detecting player's speaking. We always sample to improve AEC quality.
        //      Always try best to match the same length and send to the filter data.
        protected override IEnumerator _Calibrate()
        {
            while (m_InputData.Count >= k_NumSamples && m_OutputData.Count >= k_NumSamples)
            {
                FilterAudio(m_InputData.Take(k_NumSamples).ToArray(), m_OutputData.Take(k_NumSamples).ToArray());
                m_InputData.RemoveRange(0, k_NumSamples);
                m_OutputData.RemoveRange(0, k_NumSamples);
            }
            yield return null;
        }
        protected override void Collect()
        {
            if (m_SamplingMode == MicSampleMode.NO_MIC)
                return;
            IsPlayerSpeaking = true;
            IsCapturing = IsRecording || AutoDetectPlayerSpeaking && IsPlayerSpeaking;
            if (IsCapturing)
            {
                string charName = InworldController.CharacterHandler.CurrentCharacter ? InworldController.CharacterHandler.CurrentCharacter.BrainName : "";
                byte[] output = Output(m_FilterData.Count);
                string audioData = Convert.ToBase64String(output);
                m_AudioToPush.Enqueue(new AudioChunk
                {
                    chunk = audioData,
                    targetName = charName
                });
            }
        }
        protected override byte[] Output(int nSize)
        {
            byte[] result = WavUtility.ShortArrayToByteArray(m_FilterData.ToArray());
            m_FilterData.Clear();
            return result;
        }
        protected void FilterAudio(short[] inputData, short[] outputData)
        {
            if (outputData == null || outputData.Length == 0 || !EnableAEC)
            {
                m_FilterData.AddRange(inputData);
            }
            else
            {
                short[] filterTmp = new short[k_NumSamples];
                AECInterop.WebRtcAec3_BufferFarend(m_AECHandle, outputData);
                AECInterop.WebRtcAec3_Process(m_AECHandle, inputData, filterTmp);
                m_FilterData.AddRange(filterTmp);
                if (!m_IsAudioDebugging)
                    return;
                m_DebugInput.AddRange(inputData);
                m_DebugOutput.AddRange(outputData);
                m_DebugFilter.AddRange(filterTmp);
            }
        }
        protected void InitializeCapture(WasapiCapture capture, List<short> audioData)
        {
            if (capture == null || audioData == null)
                return;
            capture.Initialize();
            IWaveSource waveSource = new SoundInSource(capture);
            DmoResampler resampler = new DmoResampler(waveSource, m_TargetFormat);
            capture.DataAvailable += (s, e) =>
            {
                int nRatio = capture.WaveFormat.BytesPerSecond / resampler.WaveFormat.BytesPerSecond;
                byte[] inputBuffer = new byte[e.ByteCount / nRatio];
                resampler.Read(inputBuffer, 0, inputBuffer.Length);
                audioData.AddRange(WavUtility.ByteArrayToShortArray(inputBuffer));
            };
            capture.Stopped += (s, e) =>
            {
                resampler.Dispose();
                waveSource.Dispose();
            };
            capture.Start();
        }
        protected void StopCapture(WasapiCapture capture, List<short> audioData)
        {
            capture?.Stop();
            capture?.Dispose();
            audioData?.Clear();
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
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_AECHandle == IntPtr.Zero)
                return;
            AECInterop.WebRtcAec3_Free(m_AECHandle);
            m_AECHandle = IntPtr.Zero;
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
    }
}

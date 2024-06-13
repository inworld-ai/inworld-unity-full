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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inworld.Audio
{
    public class WinAudioInputCapture : AudioCapture
    {
        int m_OutputSampleRate = k_SampleRate;
        int m_OutputChannels = k_Channel;

        WaveFormat m_TargetFormat = new WaveFormat(k_SampleRate, 16, 1);
        List<short> m_InputData = new List<short>();
        List<short> m_OutputData = new List<short>();
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
        protected override IEnumerator _Calibrate()
        {
            
            yield break;
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
    }
}

/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inworld.Audio
{
    public class PlayerVoiceDetector : PlayerEventModule, ICalibrateAudioHandler
    {
        [SerializeField] [Range(0.1f, 1f)] float m_BufferSeconds = 1f; 
        [SerializeField] [Range(0.1f, 5f)] protected float m_MinAudioSessionDuration = 0.5f;
        [SerializeField] [Range(10f, 100f)] protected float m_PlayerVolumeThreashold = 10f;
        
        protected float m_BackgroundNoise = float.MinValue;
        protected float m_CalibratingTime;
        protected float m_AudioSessionSwitchingTime;
        protected int m_CurrPosition;
        protected float m_PrevResult = float.MinValue;
        
        public CircularBuffer<short> ShortBufferToSend
        {
            get
            {
                IProcessAudioHandler processor = Audio.GetModule<IProcessAudioHandler>();
                return processor == null ? Audio.InputBuffer : processor.ProcessedBuffer;
            }
        }
        
        public override IEnumerator OnPlayerUpdate()
        {
            while (isActiveAndEnabled)
            {
                if (Audio.IsCalibrating)
                    OnCalibrate();
                else
                {
                    bool isPlayerSpeaking = DetectPlayerSpeaking();
                    if (isPlayerSpeaking)
                    {
                        m_AudioSessionSwitchingTime = 0;
                        Audio.IsPlayerSpeaking = true;
                    }
                    else
                    {
                        m_AudioSessionSwitchingTime += 0.1f;
                        if (m_AudioSessionSwitchingTime >= m_MinAudioSessionDuration)
                            Audio.IsPlayerSpeaking = false;
                    }
                }
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
        public void OnStartCalibration()
        {
            m_CalibratingTime = 0;
            Audio.IsCalibrating = true;
        }

        public void OnStopCalibration()
        {
            Audio.IsCalibrating = false;
        }
        public void OnCalibrate()
        {
            float rms = CalculateRMS();
            if (rms > m_BackgroundNoise)
                m_BackgroundNoise = rms;
            // YAN: Remove the first frame as the fixedDeltaTime is extremely high.
            if (Time.fixedUnscaledDeltaTime < m_BufferSeconds)
                m_CalibratingTime += Time.fixedUnscaledDeltaTime;
            if (m_CalibratingTime >= m_BufferSeconds)
                OnStopCalibration();
        }
        
        protected virtual bool DetectPlayerSpeaking()
        {
            float result = CalculateSNR();
            return CalculateSNR() > m_PlayerVolumeThreashold;
        }

        // Root Mean Square, used to measure the variation of the noise.
        protected float CalculateRMS()
        {
            CircularBuffer<short> buffer = ShortBufferToSend;
            if (buffer == null || m_CurrPosition == ShortBufferToSend.currPos)
                return m_PrevResult;
            List<short> data = buffer.GetRange(m_CurrPosition, buffer.currPos);
            if (data.Count == 0)
                return m_PrevResult;
            m_CurrPosition = buffer.currPos;
            double nMaxSample = data.Aggregate<short, double>(0, (current, s) => current + (float)s / short.MaxValue * s / short.MaxValue);
            m_PrevResult = Mathf.Sqrt((float)(nMaxSample / data.Count));
            return m_PrevResult;
        }
        public float CalculateSNR()
        {
            float backgroundNoise = Mathf.Approximately(m_BackgroundNoise, float.MinValue) ? 0.001f : m_BackgroundNoise; 
            return 20.0f * Mathf.Log10(CalculateRMS() / backgroundNoise); 
        }
    }
}
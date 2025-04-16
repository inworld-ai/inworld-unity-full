/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Inworld.Entities;
using UnityEngine;

namespace Inworld.Audio
{
    public class AudioDispatchModule: InworldAudioModule, ISendAudioHandler
    {
        [SerializeField] MicrophoneMode m_SamplingMode = MicrophoneMode.OPEN_MIC;
        [SerializeField] int m_MaxCapacity = 100;
        [SerializeField] bool m_IsAudioDebugging;
        [SerializeField] bool m_TestMode;
        
        readonly ConcurrentQueue<AudioChunk> m_AudioToSend = new ConcurrentQueue<AudioChunk>();
        List<short> m_DebugInput = new List<short>();
        int m_CurrPosition;

        public CircularBuffer<short> ShortBufferToSend
        {
            get
            {
                IProcessAudioHandler processor = Audio.GetModule<IProcessAudioHandler>();
                return processor == null ? Audio.InputBuffer : processor.ProcessedBuffer;
            }
        }

        public bool IsReadyToSend => InworldController.Instance
                                     && InworldController.Client.Status == InworldConnectionStatus.Connected
                                     && m_AudioToSend != null
                                     && m_AudioToSend.Count > 0;

        public MicrophoneMode SendingMode
        {
            get => m_SamplingMode;
            set => m_SamplingMode = value;
        }
        void OnEnable()
        {
            StartModule(AudioDispatchingCoroutine());
        }
        void OnDisable()
        {
            StopModule();
        }

        void OnDestroy()
        {
            if (!m_IsAudioDebugging)
                return;
            WavUtility.ShortArrayToWavFile(m_DebugInput.ToArray(),"DebugInput.wav");
        }

        AudioChunk GetAudioChunk(List<short> data)
        {
            string charName = InworldController.CharacterHandler.CurrentCharacter ? InworldController.CharacterHandler.CurrentCharacter.BrainName : "";
            int nWavCount = data.Count * k_SizeofInt16;
            byte[] output = new byte[nWavCount];
            Buffer.BlockCopy(data.ToArray(), 0, output, 0, nWavCount);
            string audioData = Convert.ToBase64String(output);
            AudioChunk chunk = new AudioChunk
            {
                chunk = audioData,
                targetName = charName
            };
            return chunk;
        }
        IEnumerator AudioDispatchingCoroutine()
        {
            while (isActiveAndEnabled)
            {
                if (Audio.IsPlayerSpeaking)
                {
                    CheckAudioChunkToSend();
                    ConverBufferToAudioChunk();
                }
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        protected virtual void CheckAudioChunkToSend()
        {
            if (IsReadyToSend)
            {
                if (m_AudioToSend?.TryDequeue(out AudioChunk audioChunk) ?? false)
                {                        
                    if (!OnSendAudio(audioChunk))
                        InworldAI.LogWarning($"Sending Audio to {audioChunk.targetName} Failed. ");
                }
            }
            else if (m_AudioToSend?.Count > m_MaxCapacity)
                m_AudioToSend.TryDequeue(out _);
        }

        protected virtual void ConverBufferToAudioChunk()
        {
            CircularBuffer<short> buffer = ShortBufferToSend;
            if (m_CurrPosition != buffer.currPos)
            {
                List<short> data = buffer.GetRange(m_CurrPosition, buffer.currPos);
                m_AudioToSend?.Enqueue(GetAudioChunk(data));
                m_CurrPosition = buffer.currPos;
                if (m_IsAudioDebugging)
                    m_DebugInput.AddRange(data);
            }
        }

        public void OnStartSendAudio()
        {
            UnderstandingMode understandingMode = m_TestMode ? UnderstandingMode.SPEECH_RECOGNITION_ONLY : UnderstandingMode.FULL;
            InworldCharacter character = InworldController.CharacterHandler.CurrentCharacter;
            InworldController.Client.StartAudioTo(character ? character.BrainName : null, m_SamplingMode,
                understandingMode);
        }

        public void OnStopSendAudio()
        {
            while (!m_AudioToSend.IsEmpty)
            {
                if (m_AudioToSend.TryDequeue(out AudioChunk audioChunk))
                {
                    OnSendAudio(audioChunk);
                }
            }
            InworldController.Client.StopAudioTo();
        }

        public bool OnSendAudio(AudioChunk chunk)
        {
            if (!InworldController.Client.Current.IsConversation && chunk.targetName != InworldController.Client.Current.Character?.brainName)
            {
                InworldController.Client.Current.Character = InworldController.CharacterHandler[chunk.targetName]?.Data;
            }
            return InworldController.Client.SendAudioTo(chunk.chunk);
        }
    }
}
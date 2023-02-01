/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Packets;
using System;
using System.Collections.Concurrent;
using UnityEngine;
namespace Inworld.Audio
{
    /// <summary>
    ///     This component is used to receive/send audio from server.
    /// </summary>
    public class AudioInteraction : MonoBehaviour
    {
        #region Callbacks
        void OnPacketEvents(InworldPacket packet)
        {
            if (packet.Routing.Target.Id != m_Character.ID && packet.Routing.Source.Id != m_Character.ID)
                return;
            if (packet is not AudioChunk audioChunk)
                return;
            m_AudioChunksQueue.Enqueue(audioChunk);
        }
        #endregion

        /// <summary>
        ///     Call this func to clean up cached queue.
        /// </summary>
        public void Clear()
        {
            m_AudioChunksQueue.Clear();
        }
        #region Private Properties Variables
        readonly ConcurrentQueue<AudioChunk> m_AudioChunksQueue = new ConcurrentQueue<AudioChunk>();
        const float k_FixedUpdatePeriod = 0.1f;
        float m_CurrentFixedUpdateTime;
        AudioChunk m_CurrentAudioChunk;
        InworldCharacter m_Character;
        float _CurrentAudioLength
        {
            get => Character ? Character.CurrentAudioRemainingTime : 0f;
            set
            {
                if (!Character)
                    return;
                Character.CurrentAudioRemainingTime = value;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        ///     Get/Set its attached Inworld Character.
        /// </summary>
        public InworldCharacter Character
        {
            get => m_Character;
            set
            {
                m_Character = value;
                m_Character.Audio = this;
            }
        }
        /// <summary>
        ///     Get/Set the Audio Source for play back.
        /// </summary>
        public AudioSource PlaybackSource { get; set; }

        public bool IsMute
        {
            get
            {
                if (!PlaybackSource)
                    return true;
                return PlaybackSource.volume == 0;
            }
            set
            {
                if (PlaybackSource)
                    PlaybackSource.volume = value ? 0 : 1;
            }
        }
        /// <summary>
        /// Get if the Audio Source is Playing.
        /// </summary>
        public bool IsPlaying => PlaybackSource != null && PlaybackSource.isPlaying;
        /// <summary>
        /// Get the Current Audio Chunk.
        /// </summary>
        public AudioChunk CurrentChunk => m_CurrentAudioChunk;
        /// <summary>
        /// Triggered when audio chunk received from server.
        /// </summary>
        public event Action<PacketId> OnAudioStarted;
        /// <summary>
        /// Triggered when audio clip ended.
        /// </summary>
        public event Action OnAudioFinished;
        #endregion

        #region MonoBehavior Functions
        void Awake()
        {
            Character ??= GetComponent<InworldCharacter>();
            PlaybackSource ??= GetComponent<AudioSource>();
        }
        void OnEnable()
        {
            InworldController.Instance.OnPacketReceived += OnPacketEvents;
            PlaybackSource.Stop();
        }
        void Update()
        {
            _TimerCountDown();
            _TryGetAudio();
        }
        void OnDisable()
        {
            if (InworldController.Instance)
                InworldController.Instance.OnPacketReceived -= OnPacketEvents;
        }
        #endregion

        #region Private Functions
        void _TimerCountDown()
        {
            if (_CurrentAudioLength <= 0)
                return;
            _CurrentAudioLength -= Time.deltaTime;
            if (_CurrentAudioLength > 0)
                return;
            _CurrentAudioLength = 0;
            OnAudioFinished?.Invoke();
        }
        void _TryGetAudio()
        {
            m_CurrentFixedUpdateTime += Time.deltaTime;
            if (m_CurrentFixedUpdateTime <= k_FixedUpdatePeriod)
                return;
            m_CurrentFixedUpdateTime = 0f;
            if (IsPlaying || !m_AudioChunksQueue.TryDequeue(out m_CurrentAudioChunk) || !m_Character.IsAudioChunkAvailable(m_CurrentAudioChunk.PacketId))
                return;
            AudioClip audioClip = WavUtility.ToAudioClip(m_CurrentAudioChunk.Chunk.ToByteArray());
            if (audioClip)
            {
                _CurrentAudioLength = audioClip.length;
                PlaybackSource.PlayOneShot(audioClip, 1f);
            }
            OnAudioStarted?.Invoke(m_CurrentAudioChunk.PacketId);
        }
        #endregion
    }
}

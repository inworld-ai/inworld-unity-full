/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System.Linq;
using UnityEngine;
using Inworld.Packet;


namespace Inworld.Interactions
{
    [RequireComponent(typeof(AudioSource))]
    public class InworldAudioInteraction : InworldInteraction
    {
        protected AudioSource m_PlaybackSource;
        [SerializeField] protected float m_VolumeInterpolationSpeed = 1f;
        [Range (0, 1)]
        [SerializeField] protected float m_VolumeOnPlayerSpeaking = 1f;

        public bool IsMute
        {
            get
            {
                return m_PlaybackSource == null || !m_PlaybackSource.enabled || m_PlaybackSource.mute;
            }
            set
            {
                if (m_PlaybackSource)
                    m_PlaybackSource.mute = value;
            }
        }

        void Awake()
        {
            m_PlaybackSource = GetComponent<AudioSource>();
            if(!m_PlaybackSource)
                m_PlaybackSource = gameObject.AddComponent<AudioSource>();
            m_PlaybackSource.playOnAwake = false;
            m_PlaybackSource.Stop();
        }

        protected new void Update()
        {
            if (HistoryItem.Count > m_MaxItemCount)
                RemoveHistoryItem();
            
            float targetVolume = InworldController.Audio.IsPlayerSpeaking ? m_VolumeOnPlayerSpeaking : 1f;
            m_PlaybackSource.volume = Mathf.Lerp(m_PlaybackSource.volume, targetVolume, m_VolumeInterpolationSpeed * Time.deltaTime);
            m_PlaybackSource.volume = InworldController.Audio.IsPlayerSpeaking ? m_VolumeOnPlayerSpeaking : 1f;
            
            if (m_PlaybackSource && !m_PlaybackSource.isPlaying)
                PlayNextUtterance();
        }

        public AudioPacket NextAudio => HistoryItem.Where(item => item.Status == PacketStatus.RECEIVED)
                                                   .SelectMany(item => item.Utterances)
                                                   .Where(utterance => utterance.Status == PacketStatus.RECEIVED)
                                                   .SelectMany(utterance => utterance.Packets)
                                                   .OfType<AudioPacket>()
                                                   .FirstOrDefault();
        protected override void PlayNextUtterance()
        {
            AudioPacket nextAudio = NextAudio;
            if (nextAudio == null)
            {
                IsSpeaking = false;
                return;
            }
            m_PlaybackSource.clip = nextAudio.Clip; //YAN: Now Clip will not clean AudioChunk data.
            m_PlaybackSource.Play();
            if (nextAudio.Clip)
                AudioLength = nextAudio.Clip.length;
            Dispatch(GetUnsolvedPackets(NextAudio));
        }
        
        
        
        public override void CancelResponse()
        {
            base.CancelResponse();
            if(Interruptable)
                m_PlaybackSource.Stop();
        }
    }
}

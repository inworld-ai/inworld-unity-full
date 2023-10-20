using System.Linq;
using UnityEngine;
using Inworld.Packet;
using System;
using System.Collections.Generic;
using System.Diagnostics;


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
            float targetVolume = InworldController.Audio.IsPlayerSpeaking ? m_VolumeOnPlayerSpeaking : 1f;
            m_PlaybackSource.volume = Mathf.Lerp(m_PlaybackSource.volume, targetVolume, m_VolumeInterpolationSpeed * Time.deltaTime);
            m_PlaybackSource.volume = InworldController.Audio.IsPlayerSpeaking ? m_VolumeOnPlayerSpeaking : 1f;
            
            if (m_PlaybackSource && !m_PlaybackSource.isPlaying)
                PlayNextUtterance();
        }
        
        protected override void PlayNextUtterance()
        {
            if (m_CurrentUtterance != null)
                UpdateHistory(m_CurrentUtterance, UtteranceStatus.COMPLETED);
            
            if (UtteranceQueue.Count == 0)
            {
                IsSpeaking = false;
                m_CurrentUtterance = null;
                m_CurrentInteraction = null;
                return;
            }
            
            m_CurrentUtterance = UtteranceQueue.Dequeue();
            
            if(m_CurrentInteraction != null && m_CurrentInteraction != m_CurrentUtterance.Interaction)
                InworldAI.LogException("Attempted to play utterance for an interaction that was not the current interaction.");
            
            m_CurrentInteraction = m_CurrentUtterance.Interaction;

            AudioPacket audioPacket = m_CurrentUtterance.GetAudioPacket();
            Dispatch(m_CurrentUtterance.GetTextPacket());
            Dispatch(audioPacket);
            UpdateHistory(m_CurrentUtterance, UtteranceStatus.STARTED);
            
            m_PlaybackSource.clip = audioPacket.Clip;
            m_PlaybackSource.Play();
            if (audioPacket.Clip)
                AudioLength = audioPacket.Clip.length;
        }

        protected override void HandleAgentPacket(InworldPacket inworldPacket)
        {
            Tuple<Interaction, Utterance> historyItem = AddToHistory(inworldPacket);
            switch (inworldPacket)
            {
                case ControlPacket:
                    historyItem.Item1.RecievedInteractionEnd = true;
                    break;
                case AudioPacket:
                case TextPacket:
                    QueueUtterance(historyItem.Item2);
                    break;
                default:
                    Dispatch(inworldPacket);
                    break;
            }
        }

        protected override void QueueUtterance(Utterance utterance)
        {
            if(utterance.GetTextPacket() != null && utterance.GetAudioPacket() != null)
                UtteranceQueue.Enqueue(utterance);
        }


        public override void CancelResponse()
        {
            base.CancelResponse();
            if(Interruptable)
                m_PlaybackSource.Stop();
        }
    }
}

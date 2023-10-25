using UnityEngine;
using Inworld.Packet;
using System;
using System.Linq;

namespace Inworld.Interactions
{
    [RequireComponent(typeof(AudioSource))]
    public class InworldAudioInteraction : InworldInteraction
    {
        [SerializeField] protected float m_VolumeInterpolationSpeed = 1f;
        [Range (0, 1)]
        [SerializeField] protected float m_VolumeOnPlayerSpeaking = 1f;
        protected AudioSource m_PlaybackSource;
        
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

            if (!InworldAI.Capabilities.audio)
                InworldAI.LogException("Audio Capabilities have been disabled in the Inworld AI object. Audio is required to be enabled when using the InworldAudioInteraction component.");
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
            {
                m_CurrentUtterance.GetTextPacket().packetId.Status = PacketStatus.PLAYED;
                m_CurrentUtterance.GetAudioPacket().packetId.Status = PacketStatus.PLAYED;
                UpdateHistory(m_CurrentUtterance.Interaction);
                m_CurrentUtterance = null;
            }
            
            if (UtteranceQueue.Count == 0)
            {
                IsSpeaking = false;
                m_CurrentInteraction = null;
                return;
            }
            
            m_CurrentUtterance = UtteranceQueue.Dequeue();

            m_CurrentInteraction = m_CurrentUtterance.Interaction;
            m_LastInteractionSequenceNumber = m_CurrentInteraction.SequenceNumber;

            if(m_CurrentInteraction.Status == InteractionStatus.CREATED)
                m_CurrentInteraction.Status = InteractionStatus.STARTED;

            AudioPacket audioPacket = m_CurrentUtterance.GetAudioPacket();
            Dispatch(m_CurrentUtterance.GetTextPacket());
            Dispatch(audioPacket);
            
            m_PlaybackSource.clip = audioPacket.Clip;
            m_PlaybackSource.Play();
            if (audioPacket.Clip)
                AudioLength = audioPacket.Clip.length;
        }

        protected override void HandleAgentPacket(InworldPacket inworldPacket)
        {
            Tuple<Interaction, Utterance> historyItem = AddToHistory(inworldPacket);
            Interaction interaction = historyItem.Item1;
            
            switch (inworldPacket)
            {
                case ControlPacket:
                    historyItem.Item1.ReceivedInteractionEnd = true;
                    inworldPacket.packetId.Status = PacketStatus.PROCESSED;
                    UpdateHistory(historyItem.Item1);
                    break;
                case AudioPacket:
                case TextPacket:
                    if (interaction == m_CurrentInteraction ||
                        interaction.SequenceNumber > m_LastInteractionSequenceNumber)
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

        protected override Utterance CreateUtterance(Interaction interaction, string utteranceId)
        {
            return new AudioUtterance(interaction, utteranceId);
        }
    }
}

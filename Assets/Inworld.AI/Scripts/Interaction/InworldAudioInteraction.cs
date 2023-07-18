using System.Linq;
using UnityEngine;
using Inworld.Packet;

namespace Inworld.Interactions
{
    [RequireComponent(typeof(AudioSource))]
    public class InworldAudioInteraction : InworldInteraction
    {
        AudioSource m_PlaybackSource;

        void Awake()
        {
            m_PlaybackSource ??= GetComponent<AudioSource>();
            m_PlaybackSource ??= gameObject.AddComponent<AudioSource>();
        }

        void Update()
        {
            if (HistoryItem.Count > m_MaxItemCount)
                RemoveHistoryItem();
            if (m_PlaybackSource && !m_PlaybackSource.isPlaying)
                PlayNextUtterance();
        }
        public bool IsMute
        {
            get
            {
                if (m_PlaybackSource)
                    return true;
                return m_PlaybackSource.volume == 0;
            }
            set
            {
                if (m_PlaybackSource)
                    m_PlaybackSource.volume = value ? 0 : 1;
            }
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
            m_PlaybackSource.PlayOneShot(nextAudio.Clip, 1f);
            if (nextAudio.Clip)
                AudioLength = nextAudio.Clip.length;

            UpdateInteraction(nextAudio);

            Dispatch(this[nextAudio.packetId.interactionId][nextAudio.packetId.utteranceId].Packets);
        }
    }
}

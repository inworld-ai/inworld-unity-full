using System.Linq;
using UnityEngine;
using Inworld.Packet;
using System.Collections.Generic;
using System.Diagnostics;

namespace Inworld.Interactions
{
    [RequireComponent(typeof(AudioSource))]
    public class InworldAudioInteraction : InworldInteraction
    {
        public static List<(float[], float)> SharedAudioData = new List<(float[], float)>();
        AudioSource m_PlaybackSource;
        public float VolumeInterpolationSpeed = 1f;
        [Range (0, 1)]
        public float m_VolumeOnPlayerSpeaking = 0.01f;
        public static System.Diagnostics.Stopwatch stopwatch;

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

        void Awake()
        {
            m_PlaybackSource ??= GetComponent<AudioSource>();
            m_PlaybackSource ??= gameObject.AddComponent<AudioSource>();
            if(stopwatch == null)
                stopwatch = System.Diagnostics.Stopwatch.StartNew();
        }

        void Update()
        {
            if (HistoryItem.Count > m_MaxItemCount)
                RemoveHistoryItem();
            
            float targetVolume = InworldController.IsPlayerSpeaking ? m_VolumeOnPlayerSpeaking : 1f;
            m_PlaybackSource.volume = Mathf.Lerp(m_PlaybackSource.volume, targetVolume, VolumeInterpolationSpeed * Time.deltaTime);

            m_PlaybackSource.volume = InworldController.IsPlayerSpeaking ? m_VolumeOnPlayerSpeaking : 1f;
            
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
            m_PlaybackSource.PlayOneShot(nextAudio.Clip, 1f);
            if (nextAudio.Clip)
                AudioLength = nextAudio.Clip.length;
            UpdateInteraction(nextAudio);
            Dispatch(this[nextAudio.packetId.interactionId][nextAudio.packetId.utteranceId].Packets);
        }
        
        public override void CancelResponse()
        {
            base.CancelResponse();
            if(Interruptable)
                m_PlaybackSource.Stop();
        }
        
        void OnAudioFilterRead(float[] data, int channels)
        {
            float time = (float)stopwatch.Elapsed.TotalSeconds;
            lock (SharedAudioData)
            {
                // Add the data and timestamp to the shared buffer
                SharedAudioData.Add((data, time));
            }

            // Clean up old data
            while (SharedAudioData.Count > 0 && time - SharedAudioData[0].Item2 > 1.0f)
            {
                lock (SharedAudioData)
                {
                    SharedAudioData.RemoveAt(0);
                }
            }
        }
        
    }
}

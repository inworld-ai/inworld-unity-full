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
        [SerializeField] protected float m_VolumeInterpolationSpeed = 1f;
        [Range (0, 1)][SerializeField] protected float m_VolumeOnPlayerSpeaking = 1f;
        
        AudioSource m_PlaybackSource;
        float m_LastSampleTime;
        float m_CurrentSampleTime;

        public AudioSource PlaybackSource => m_PlaybackSource;

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
        
        public override short[] GetCurrentAudioFragment()
        {
            m_LastSampleTime = m_CurrentSampleTime;
            m_CurrentSampleTime = m_PlaybackSource.time;
            return ExtractAudioSegment(m_PlaybackSource.clip, m_LastSampleTime, m_CurrentSampleTime);
        }
        short[] ExtractAudioSegment(AudioClip originalClip, float startTime, float endTime)
        {
            if (m_PlaybackSource.clip == null)
                return null;
            int startSample = Mathf.FloorToInt(startTime * originalClip.frequency);
            int endSample = Mathf.FloorToInt(endTime * originalClip.frequency);
            int sampleLength = endSample - startSample;

            if (sampleLength <= 0 || startSample < 0 || endSample > originalClip.samples)
            {
                // YAN: Just new pieces.
                return null;
            }

            float[] originalData = new float[originalClip.samples * originalClip.channels];
            originalClip.GetData(originalData, 0);

            float[] extractedData = new float[sampleLength * originalClip.channels];
            System.Array.Copy(originalData, startSample * originalClip.channels, extractedData, 0, sampleLength * originalClip.channels);

            return WavUtility.ConvertAudioClipDataToInt16Array(extractedData, extractedData.Length);
        }
    }
}

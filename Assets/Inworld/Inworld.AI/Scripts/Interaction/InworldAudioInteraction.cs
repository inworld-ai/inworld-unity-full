/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using UnityEngine;
using Inworld.Packet;
using System;

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
        /// <summary>
        /// Gets this character's audio source
        /// </summary>
        public AudioSource PlaybackSource => m_PlaybackSource;
        /// <summary>
        /// Mute/Unmute this character.
        /// </summary>
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
        /// <summary>
        /// Interrupt this character by cancelling its incoming responses.
        /// </summary>
        public override void CancelResponse()
        {
            base.CancelResponse();
            if(Interruptable)
                m_PlaybackSource.Stop();
        }
        /// <summary>
        /// Gets this current characters audio.
        /// Used in the mixer to calculate the environment noise.
        /// </summary>
        public short[] GetCurrentAudioFragment()
        {
            m_LastSampleTime = m_CurrentSampleTime;
            m_CurrentSampleTime = m_PlaybackSource.time;
            return ExtractAudioSegment(m_PlaybackSource.clip, m_LastSampleTime, m_CurrentSampleTime);
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

            if (inworldPacket is ControlPacket)
            {
                historyItem.Item1.ReceivedInteractionEnd = true;
                inworldPacket.packetId.Status = PacketStatus.PROCESSED;
                UpdateHistory(historyItem.Item1);
            }
            else if (inworldPacket is AudioPacket || inworldPacket is TextPacket)
            {
                if (interaction == m_CurrentInteraction ||
                    interaction.SequenceNumber > m_LastInteractionSequenceNumber)
                    QueueUtterance(historyItem.Item2);
            }
            else
                Dispatch(inworldPacket);
        }

        protected override void QueueUtterance(Utterance utterance)
        {
            if(utterance.GetTextPacket() != null && utterance.GetAudioPacket() != null)
                UtteranceQueue.Enqueue(utterance);
        }
        
        protected override Utterance CreateUtterance(Interaction interaction, string utteranceId)
        {
            return new AudioUtterance(interaction, utteranceId);
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
            Array.Copy(originalData, startSample * originalClip.channels, extractedData, 0, sampleLength * originalClip.channels);

            return WavUtility.ConvertAudioClipDataToInt16Array(extractedData, extractedData.Length);
        }
    }
}

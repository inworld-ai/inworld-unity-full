/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using UnityEngine;
using System.Collections;


namespace Inworld.Interactions
{
    [RequireComponent(typeof(AudioSource))]
    public class InworldAudioInteraction : InworldInteraction
    {
        [Range (0, 1)][SerializeField] protected float m_VolumeOnPlayerSpeaking = 1f;
        AudioSource m_PlaybackSource;
        AudioClip m_AudioClip;
        const string k_NoAudioCapabilities = "Audio Capabilities have been disabled in the Inworld AI object. Audio is required to be enabled when using the InworldAudioInteraction component.";
        public override float AnimFactor
        {
            get => m_AnimFactor;
            set => m_AnimFactor = value;
        }
        /// <summary>
        /// Gets this character's audio source
        /// </summary>
        public AudioSource PlaybackSource => m_PlaybackSource;
        /// <summary>
        /// Mute/Unmute this character.
        /// </summary>
        public bool IsMute
        {
            get => m_PlaybackSource == null || !m_PlaybackSource.enabled || m_PlaybackSource.mute;
            set
            {
                if (m_PlaybackSource)
                    m_PlaybackSource.mute = value;
            }
        }
        /// <summary>
        /// Interrupt this character by cancelling its incoming responses.
        /// </summary>
        public override void CancelResponse(bool isHardCancelling = true)
        {
            base.CancelResponse(isHardCancelling);
            if(m_Interruptable)
                m_PlaybackSource.Stop();
        }
        void Awake()
        {
            m_PlaybackSource = GetComponent<AudioSource>();
            if(!m_PlaybackSource)
                m_PlaybackSource = gameObject.AddComponent<AudioSource>();
            m_PlaybackSource.playOnAwake = false;
            m_PlaybackSource.Stop();
            if (!InworldAI.Capabilities.audio)
                InworldAI.LogException(k_NoAudioCapabilities);
        }
        protected override IEnumerator InteractionCoroutine()
        {
            while (true)
            {
                yield return AdjustVolume();
                yield return RemoveExceedItems();
                yield return HandleNextUtterance();
            }
        }
        protected override IEnumerator PlayNextUtterance()
        {
            m_AudioClip = m_CurrentInteraction.CurrentUtterance.GetAudioClip();
            if (m_AudioClip == null)
            {
                Dispatch(m_CurrentInteraction.CurrentUtterance.Packets);
                yield return new WaitForSeconds(m_CurrentInteraction.CurrentUtterance.GetTextSpeed() * m_TextSpeedMultipler);
                if (m_CurrentInteraction != null)
                    m_CurrentInteraction.CurrentUtterance = null;
            }
            else
            {
                AnimFactor = m_AudioClip.length;
                m_PlaybackSource.PlayOneShot(m_AudioClip);
                Dispatch(m_CurrentInteraction.CurrentUtterance.Packets);
                yield return new WaitUntil(() => !m_PlaybackSource.isPlaying);
                if (m_CurrentInteraction != null)
                    m_CurrentInteraction.CurrentUtterance = null;
            }
        }
        protected override void SkipCurrentUtterance()
        {
            base.SkipCurrentUtterance();
            m_PlaybackSource.Stop();
        }
        protected IEnumerator AdjustVolume()
        {
            m_PlaybackSource.volume = InworldController.Audio.IsPlayerSpeaking ? m_VolumeOnPlayerSpeaking : 1f;
            yield break;
        }
    }
}

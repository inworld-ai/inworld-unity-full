/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
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
        const string k_NoAudioCapabilities = "Audio Capabilities have been disabled in the Inworld AI object. Audio is required to be enabled when using the InworldAudioInteraction component.";

        public override float AnimFactor
        {
            get => m_AnimFactor;
            set => m_AnimFactor = value;
        } 
        protected float m_AudioReducer;
        protected bool m_IsPlayerSpeaking;
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

        protected override void OnCharacterSelected(string brainName)
        {
            if (brainName != m_Character.BrainName)
                return;
            base.OnCharacterSelected(brainName);
            m_AudioReducer = 0;
        }

        protected override void OnPlayerStartSpeaking()
        {
            m_IsPlayerSpeaking = true;
        }
        protected override void OnPlayerStopSpeaking()
        {
            m_IsPlayerSpeaking = false;
        }

        public override IEnumerator CancelResponseAsync()
        {
            while (m_PlaybackSource && m_PlaybackSource.volume > 0.1f)
            {
                m_AudioReducer += Time.fixedUnscaledDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            CancelResponse();
        }

        /// <summary>
        /// Interrupt this character by cancelling its incoming responses.
        /// </summary>
        public override bool CancelResponse(bool isHardCancelling = true)
        {
            if (!base.CancelResponse(isHardCancelling))
                return false;
            m_PlaybackSource.clip = null;
            m_PlaybackSource.Stop();
            return true;
        }
        protected override void Awake()
        {
            base.Awake();
            m_PlaybackSource = GetComponent<AudioSource>();
            if (!m_PlaybackSource)
                m_PlaybackSource = gameObject.AddComponent<AudioSource>();
            m_PlaybackSource.playOnAwake = false;
            m_PlaybackSource.Stop();
            if (!InworldAI.Capabilities.audio)
                InworldAI.LogWarning(k_NoAudioCapabilities);
        }

        void FixedUpdate()
        {
            if (!CanPass())
                AnimFactor += Time.fixedUnscaledDeltaTime;
            else
                AnimFactor = 0;
        }

        void LateUpdate()
        {
            if (!m_PlaybackSource || !m_Character)
                return;
            if (InworldController.CharacterHandler &&
                InworldController.CharacterHandler.SelectingMethod != CharSelectingMethod.SightAngle)
            {
                m_PlaybackSource.volume = 1f;
                return;
            }
            float fallBackValue =  Mathf.Min(0.8f, m_Character.Priority < 0 ? 0.8f : m_Character.Priority * 2);
            float playerReducer = m_IsPlayerSpeaking ? m_VolumeOnPlayerSpeaking : 1f;
            m_PlaybackSource.volume = (1 - fallBackValue) * playerReducer - m_AudioReducer;
        }
        protected override IEnumerator InteractionCoroutine()
        {
            while (true)
            {
                yield return RemoveExceedItems();
                yield return HandleUtterances();
                yield return null;
            }
        }
        protected override IEnumerator PlayCurrentUtterance()
        {
            AudioClip audioClip = m_CurrentInteraction.CurrentUtterance.AudioClip;
            if (audioClip == null)
            {
                m_Character.OnInteractionChanged(m_CurrentInteraction.CurrentUtterance.Packets);
                yield return new WaitForSeconds(m_CurrentInteraction.CurrentUtterance.GetTextSpeed() * m_TextSpeedMultipler);
            }
            else
            {
                if (audioClip != m_AudioClip)
                {
                    m_AudioClip = audioClip;
                    m_PlaybackSource.clip = m_AudioClip;
                    m_PlaybackSource.Play();
                }
                m_Character.OnInteractionChanged(m_CurrentInteraction.CurrentUtterance.Packets);
                yield return new WaitUntil(CanPass);
                m_PlaybackSource.clip = null;
            }
            m_CurrentInteraction?.Processed();
        }

        bool CanPass()
        {
            bool canPass = !m_PlaybackSource.clip || !m_PlaybackSource.isPlaying;
            if (canPass)
                AnimFactor = 0;
            return canPass;
        }


        protected override void SkipCurrentUtterance()
        {
            base.SkipCurrentUtterance();
            m_PlaybackSource.Stop();
        }
    }
}

/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using UnityEngine;
using Inworld.Packet;
using System.Collections;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Inworld.Interactions
{
    public class InworldInteraction : MonoBehaviour
    {
        [SerializeField] GameObject m_ContinueButton;
        [SerializeField] protected bool m_AutoProceed = true;
        [SerializeField] protected int m_MaxItemCount = 100;
        [SerializeField] protected float m_TextSpeedMultipler = 0.02f;
        protected InworldCharacter m_Character;
        protected Interaction m_CurrentInteraction;
        protected IEnumerator m_FadeOutCoroutine;
        protected InputAction m_ContinueAction;
        protected InputAction m_SkipAction;
        protected IEnumerator m_CurrentCoroutine;
        protected AudioSource m_PlaybackSource;
        protected AudioClip m_AudioClip;
        protected readonly IndexQueue<Interaction> m_Prepared = new IndexQueue<Interaction>();
        protected readonly IndexQueue<Interaction> m_Processed = new IndexQueue<Interaction>();
        protected readonly IndexQueue<Interaction> m_Cancelled = new IndexQueue<Interaction>();

        protected bool m_Proceed = true;
        protected bool m_IsContinueKeyPressed;
        protected bool m_LastFromPlayer;
        protected float m_AnimFactor;
        
        /// <summary>
        /// Gets the factor for selecting animation clips.
        /// If without Audio, it's a random value between 0 and 1.
        /// </summary>
        public virtual float AnimFactor
        {
            get
            {
                if (m_AnimFactor == 0)
                    m_AnimFactor = Random.Range(0, 1);
                return m_AnimFactor;
            }
            set => m_AnimFactor = value;
        }

        /// <summary>
        /// If the target packet is sent or received by this character.
        /// </summary>
        /// <param name="packet">the target packet.</param>
        public bool IsRelated(InworldPacket packet) => packet.IsRelated(m_Character.ID);

        /// <summary>
        /// Gradually cancel the current response.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator CancelResponseAsync()
        {
            yield return new WaitForSecondsRealtime(1f);
            CancelResponse();
        }
        /// <summary>
        /// Interrupt this character by cancelling its incoming sentences.
        /// Hard cancelling means even cancel and interrupt the current interaction.
        /// Soft cancelling will only cancel the stored interactions.
        /// </summary>
        /// <param name="isHardCancelling">If it's hard cancelling. By default it's true.</param>
        public virtual bool CancelResponse(bool isHardCancelling = true)
        {
            if (string.IsNullOrEmpty(m_Character.ID))
                return false;
            if (m_CurrentInteraction == null || !m_CurrentInteraction.Interruptible)
                return false;
            InworldController.Client.SendCancelEventTo(m_CurrentInteraction.ID, m_CurrentInteraction.CurrentUtterance?.ID, m_Character.BrainName, isHardCancelling);
            m_CurrentInteraction.Cancel(isHardCancelling);
            m_Prepared.Enqueue(m_CurrentInteraction);
            m_Prepared.PourTo(m_Cancelled);
            m_CurrentInteraction = null;
            return true;
        }
        protected virtual void Awake()
        {
            if (!m_Character)
            {
                m_Character = GetComponent<InworldCharacter>();
                m_Character.Event.onCharacterSelected.AddListener(OnCharacterSelected);
                m_Character.Event.onCharacterDeselected.AddListener(OnCharacterDeselected);
            }
            if (!m_Character)
                enabled = false;
            m_ContinueAction = InworldAI.InputActions["Continue"];
            m_SkipAction = InworldAI.InputActions["Skip"];
        }
        protected virtual void OnEnable()
        {
            InworldController.Audio.Event.onPlayerStartSpeaking.AddListener(OnPlayerStartSpeaking);
            InworldController.Audio.Event.onPlayerStopSpeaking.AddListener(OnPlayerStopSpeaking);
            InworldController.Client.OnPacketReceived += ReceivePacket;
            m_CurrentCoroutine = InteractionCoroutine();
            StartCoroutine(m_CurrentCoroutine);
        }

        protected virtual void OnDisable()
        {
            StopCoroutine(m_CurrentCoroutine);

            if (!InworldController.Instance)
                return;
            InworldController.Audio.Event.onPlayerStartSpeaking.RemoveListener(OnPlayerStartSpeaking);
            InworldController.Audio.Event.onPlayerStopSpeaking.RemoveListener(OnPlayerStopSpeaking);
            InworldController.Client.OnPacketReceived -= ReceivePacket;
        }

        protected virtual void OnCharacterDeselected(string brainName)
        {
            if (brainName != m_Character.BrainName)
                return;
            if (m_Character.IsOnDisable || m_FadeOutCoroutine != null)
                return;
            m_FadeOutCoroutine = CancelResponseAsync();
            StartCoroutine(m_FadeOutCoroutine);
        }
        protected virtual void OnCharacterSelected(string brainName)
        {
            if (!string.IsNullOrEmpty(brainName))
                return;
            if (m_FadeOutCoroutine == null)
                return;
            StopCoroutine(m_FadeOutCoroutine);
            m_FadeOutCoroutine = null;
        }
        protected virtual void OnPlayerStartSpeaking()
        {
            
        }
        protected virtual void OnPlayerStopSpeaking()
        {
            
        }
        void Update()
        {
            if (m_SkipAction != null && m_SkipAction.WasReleasedThisFrame())
                SkipCurrentUtterance();
            if (m_ContinueAction != null && m_ContinueAction.WasPressedThisFrame())
                UnpauseUtterance();
            if (m_ContinueAction != null && m_ContinueAction.WasReleasedThisFrame())
                PauseUtterance();
            m_Proceed = m_AutoProceed || m_LastFromPlayer || m_IsContinueKeyPressed || m_CurrentInteraction == null || m_CurrentInteraction.IsEmpty;
        }
        protected virtual void UnpauseUtterance()
        {
            m_IsContinueKeyPressed = true;
        }
        protected virtual void PauseUtterance()
        {
            m_IsContinueKeyPressed = false;
        }
        protected virtual void SkipCurrentUtterance() 
        {
            if (m_CurrentInteraction?.CurrentUtterance != null)
                m_CurrentInteraction.CurrentUtterance = null;
        }
        protected virtual IEnumerator InteractionCoroutine()
        {
            while (true)
            {
                yield return RemoveExceedItems();
                yield return HandleUtterances();
                yield return null;
            }
        }
        protected IEnumerator HandleUtterances()
        {
            if (m_Proceed)
            {
                HideContinue();
                if (m_CurrentInteraction == null)
                {
                    m_CurrentInteraction = GetNextInteraction();
                }
                if (m_CurrentInteraction != null && m_CurrentInteraction.CurrentUtterance == null)
                {
                    m_CurrentInteraction.CurrentUtterance = GetNextUtterance();
                }
                if (m_CurrentInteraction != null && m_CurrentInteraction.CurrentUtterance != null)
                {
                    yield return PlayCurrentUtterance();
                }
                else if (m_Character)
                    m_Character.IsSpeaking = false;
            }
            else
            {
                ShowContinue();
            }
        }
        void HideContinue()
        {
            if (m_ContinueButton)
                m_ContinueButton.SetActive(false);
        }
        void ShowContinue()
        {
            if (m_ContinueButton)
                m_ContinueButton.SetActive(true);
        }
        void ReceivePacket(InworldPacket incomingPacket)
        {
            if (!IsRelated(incomingPacket))
                return;
            if (incomingPacket.Source == SourceType.PLAYER && (incomingPacket.IsBroadCast || incomingPacket.IsTarget(m_Character.ID)))
            {
                if (!(incomingPacket is AudioPacket))
                    m_LastFromPlayer = true;
                m_Character.ProcessPacket(incomingPacket);
            }
            if (incomingPacket.Source == SourceType.AGENT && (incomingPacket.IsSource(m_Character.ID) || incomingPacket.IsTarget(m_Character.ID)))
            {
                if (incomingPacket is AudioPacket && !incomingPacket.IsSource(m_Character.ID)) //Audio chunk only dispatch once. to the source.
                    return;
                m_LastFromPlayer = false;
                HandleAgentPackets(incomingPacket);
            }
        }
        protected void HandleAgentPackets(InworldPacket packet)
        {
            if (m_Cancelled.Contains(packet))
                m_Cancelled.Add(packet);
            else if (m_CurrentInteraction != null && m_CurrentInteraction.Contains(packet))
                m_CurrentInteraction.Add(packet);
            else
                m_Prepared.Add(packet);
        }
        protected IEnumerator RemoveExceedItems()
        {
            if (m_Cancelled.Count > m_MaxItemCount)
                m_Cancelled.Dequeue();
            if (m_Processed.Count > m_MaxItemCount)
                m_Processed.Dequeue();
            yield break;
        }
        protected Interaction GetNextInteraction()
        {
            if (m_CurrentInteraction != null)
                return null;
            m_CurrentInteraction = m_Prepared.Dequeue(true);
            return m_CurrentInteraction;
        }
        protected Utterance GetNextUtterance()
        {
            if (m_CurrentInteraction.CurrentUtterance != null)
                return null;
            // YAN: At the moment of Dequeuing, the utterance is already in processed.
            m_CurrentInteraction.CurrentUtterance = m_CurrentInteraction.Dequeue();
            if (m_CurrentInteraction.CurrentUtterance != null)
                return m_CurrentInteraction.CurrentUtterance;
            // YAN: Else set the current interaction to null to get next dequeue interaction.
            m_Processed.Enqueue(m_CurrentInteraction);
            m_CurrentInteraction = null; 
            return null;
        }
        protected virtual IEnumerator PlayCurrentUtterance()
        {
            m_Character.OnInteractionChanged(m_CurrentInteraction.CurrentUtterance.Packets);
            yield return new WaitForSeconds(m_CurrentInteraction.CurrentUtterance.GetTextSpeed() * m_TextSpeedMultipler);
            m_CurrentInteraction?.Processed();
        }
    }
}

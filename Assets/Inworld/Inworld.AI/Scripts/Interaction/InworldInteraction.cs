/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;
using System.Collections.Generic;

using UnityEngine;
using Inworld.Packet;
using System.Collections;
using Random = UnityEngine.Random;

namespace Inworld.Interactions
{
    public class InworldInteraction : MonoBehaviour
    {
        [SerializeField] protected bool m_Interruptable = true;
        [SerializeField] protected bool m_AutoProceed = true;
        [SerializeField] protected int m_MaxItemCount = 100;
        [SerializeField] protected float m_TextSpeedMultipler = 0.02f;
        protected Interaction m_CurrentInteraction;
        protected IEnumerator m_CurrentCoroutine;
        protected readonly IndexQueue<Interaction> m_Prepared = new IndexQueue<Interaction>();
        protected readonly IndexQueue<Interaction> m_Processed = new IndexQueue<Interaction>();
        protected readonly IndexQueue<Interaction> m_Cancelled = new IndexQueue<Interaction>();
        protected bool m_IsSpeaking;
        public event Action<List<InworldPacket>> OnInteractionChanged;
        public event Action<bool> OnStartStopInteraction;

        protected float m_AnimFactor;
        /// <summary>
        /// Gets the factor for selecting animation clips.
        /// If without Audio, it's a random value between 0 and 1.
        /// </summary>
        public virtual float AnimFactor
        {
            get => Random.Range(0, 1);
            set => m_AnimFactor = value;
        }

        /// <summary>
        /// Gets/Sets if this character is speaking.
        /// If set, will trigger the event OnStartStopInteraction.
        /// </summary>
        public bool IsSpeaking
        {
            get => m_IsSpeaking;
            set
            {
                if (m_IsSpeaking == value)
                    return;
                m_IsSpeaking = value;
                OnStartStopInteraction?.Invoke(m_IsSpeaking);
            }
        }
        
        /// <summary>
        /// Gets/Sets the live session ID of the character.
        /// </summary>
        public string LiveSessionID { get; set; }
        
        /// <summary>
        /// If the target packet is sent or received by this character.
        /// </summary>
        /// <param name="packet">the target packet.</param>
        public bool IsRelated(InworldPacket packet) => 
            !string.IsNullOrEmpty(LiveSessionID) && 
            (packet.routing.source.name == LiveSessionID || packet.routing.target.name == LiveSessionID);
        /// <summary>
        /// Interrupt this character by cancelling its incoming sentences.
        /// Hard cancelling means even cancel and interrupt the current interaction.
        /// Soft cancelling will only cancel the stored interactions.
        /// </summary>
        /// <param name="isHardCancelling">If it's hard cancelling. By default it's true.</param>
        public virtual void CancelResponse(bool isHardCancelling = true)
        {
            if (string.IsNullOrEmpty(LiveSessionID) || !m_Interruptable)
                return;
            if (isHardCancelling && m_CurrentInteraction != null)
            {
                m_CurrentInteraction.Cancel();
                InworldController.Instance.SendCancelEvent(LiveSessionID, m_CurrentInteraction.ID);
            }
            m_Prepared.PourTo(m_Cancelled);
            m_CurrentInteraction = null;
        }
        protected virtual void OnEnable()
        {
            InworldController.Client.OnPacketReceived += ReceivePacket;
            m_CurrentCoroutine = InteractionCoroutine();
            StartCoroutine(m_CurrentCoroutine);
        }

        protected virtual void OnDisable()
        {
            StopCoroutine(m_CurrentCoroutine);
            if (InworldController.Instance)
                InworldController.Client.OnPacketReceived -= ReceivePacket;
        }

        protected virtual IEnumerator InteractionCoroutine()
        {
            while (true)
            {
                yield return RemoveExceedItems();
                yield return HandleNextUtterance();
            }
        }
        protected IEnumerator HandleNextUtterance()
        {
            if (m_AutoProceed || Input.GetKeyUp(KeyCode.Space))
            {
                if (m_CurrentInteraction == null)
                {
                    m_CurrentInteraction = GetNextInteraction();
                }
                if (m_CurrentInteraction != null && m_CurrentInteraction.CurrentUtterance == null)
                {
                    m_CurrentInteraction.CurrentUtterance = GetNextUtterance();
                }
                yield return PlayNextUtterance();
            }
            else
                yield return null;
        }
        void ReceivePacket(InworldPacket incomingPacket)
        {
            if (!IsRelated(incomingPacket))
                return;
            switch (incomingPacket.routing?.source?.type.ToUpper())
            {
                case "AGENT":
                    HandleAgentPackets(incomingPacket);
                    break;
                case "PLAYER":
                    // Send Directly.
                    Dispatch(incomingPacket);
                    break;
            }
        }
        protected void Dispatch(List<InworldPacket> packets) => OnInteractionChanged?.Invoke(packets);
        protected void Dispatch(InworldPacket packet) => OnInteractionChanged?.Invoke(new List<InworldPacket> {packet});

        protected void HandleAgentPackets(InworldPacket packet)
        {
            if (m_Processed.IsOverDue(packet))
                m_Processed.Add(packet);
            else if (m_Cancelled.IsOverDue(packet))
                m_Cancelled.Add(packet);
            else if (m_CurrentInteraction != null && m_CurrentInteraction.Contains(packet))
            {
                m_CurrentInteraction.Add(packet);
            }
            else
            {
                m_Prepared.Add(packet);
            }
        }
        protected IEnumerator RemoveExceedItems()
        {
            m_Cancelled.Clear();
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
            m_CurrentInteraction.CurrentUtterance = m_CurrentInteraction.Dequeue();
            if (m_CurrentInteraction.CurrentUtterance != null)
                return m_CurrentInteraction.CurrentUtterance;
            // YAN: Else set the current interaction to null to get next dequeue interaction.
            m_Processed.Enqueue(m_CurrentInteraction);
            m_CurrentInteraction = null; 
            return null;
        }
        protected virtual IEnumerator PlayNextUtterance()
        {
            if (m_CurrentInteraction == null || m_CurrentInteraction.CurrentUtterance == null)
                yield break;
            Dispatch(m_CurrentInteraction.CurrentUtterance.Packets);
            yield return new WaitForSeconds(m_CurrentInteraction.CurrentUtterance.GetTextSpeed() * m_TextSpeedMultipler);
            if (m_CurrentInteraction != null)
                m_CurrentInteraction.CurrentUtterance = null; // YAN: Processed.
        }
    }
}

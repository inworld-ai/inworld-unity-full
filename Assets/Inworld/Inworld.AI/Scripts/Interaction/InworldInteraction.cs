/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inworld.Packet;

namespace Inworld.Interactions
{
    public class InworldInteraction : MonoBehaviour
    {
        [SerializeField] bool m_Interruptable = true;
        [SerializeField] protected int m_MaxItemCount = 100;
        [SerializeField] float m_TextDuration = 0.5f;
        float m_CurrentTime;
        bool m_IsSpeaking;
        public float AudioLength { get; set; }
        public bool Interruptable
        {
            get => m_Interruptable;
            set => m_Interruptable = value;
        }
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
        public string LiveSessionID { get; set; }
        public List<Interaction> HistoryItem { get; set; } = new List<Interaction>();

        Interaction m_ProcessingInteraction = new Interaction();
        public event Action<List<InworldPacket>> OnInteractionChanged;
        public event Action<bool> OnStartStopInteraction;
        public Interaction this[string interactionID] => HistoryItem.FirstOrDefault(i => i.InteractionID == interactionID);
        public Utterance NextUtterance => HistoryItem
                                          .Where(i => i.Status == PacketStatus.RECEIVED)
                                          .SelectMany(item => item.Utterances)
                                          .FirstOrDefault(utterance => utterance.Status == PacketStatus.RECEIVED);

        public bool IsRelated(InworldPacket packet) => !string.IsNullOrEmpty(LiveSessionID) 
            && (packet.routing.source.name == LiveSessionID || packet.routing.target.name == LiveSessionID);
        
        protected virtual void OnEnable()
        {
            InworldController.Client.OnPacketReceived += ReceivePacket;
        }
        protected virtual void OnDisable()
        {
            if (InworldController.Instance)
                InworldController.Client.OnPacketReceived -= ReceivePacket;
        }
        protected void Update()
        {
            if (HistoryItem.Count > m_MaxItemCount)
                RemoveHistoryItem();
            
            m_CurrentTime += Time.deltaTime;
            if (m_CurrentTime < m_TextDuration)
                return;
            m_CurrentTime = 0;
            PlayNextUtterance();
        }

        protected void Dispatch(InworldPacket packet) => OnInteractionChanged?.Invoke(new List<InworldPacket> {packet});
        protected void Dispatch(List<InworldPacket> packets) => OnInteractionChanged?.Invoke(packets);
        public virtual short[] GetCurrentAudioFragment()
        {
            return null;
        }
        protected List<InworldPacket> GetUnsolvedPackets(InworldPacket packet)
        {
            List<InworldPacket> result = new List<InworldPacket>();

            // 1. Check all unsolved packets till now.
            foreach (Interaction i in HistoryItem)
            {
                foreach (Utterance u in i.Utterances.Where(t1 => t1.Status == PacketStatus.RECEIVED))
                {
                    u.Status = PacketStatus.PLAYED;
                    result.AddRange(u.Packets);
                    if (u.Packets.Contains(packet))
                        break;
                }
                // 2. Also update Interaction status.
                if (i.Utterances.All(u => u.Status != PacketStatus.RECEIVED))
                {
                    i.Status = PacketStatus.PLAYED;
                }
            }
            return result;
        }
        public virtual void CancelResponse()
        {
            if (string.IsNullOrEmpty(LiveSessionID) || !Interruptable)
                return;
            Interaction item = HistoryItem.LastOrDefault(i => i.Status == PacketStatus.RECEIVED);
            if (item == null)
                return;
            item.Status = PacketStatus.CANCELLED;
            string interactionToCancel = item.InteractionID;
            InworldController.Instance.SendCancelEvent(LiveSessionID, interactionToCancel);
        }
        protected virtual void PlayNextUtterance()
        {
            Utterance utterance = NextUtterance;
            if (utterance == null)
            {
                IsSpeaking = false;
                return;
            }
            Dispatch(GetUnsolvedPackets(utterance.Packets[0]));
        }

        protected void RemoveHistoryItem()
        {
            Interaction toDelete = HistoryItem.FirstOrDefault(i => 
                i.Status != PacketStatus.RECEIVED || 
                i.Utterances.Any(u => u.Status != PacketStatus.RECEIVED));
            if (toDelete != null)
            {
                HistoryItem.Add(toDelete);
            }
                
        }
        void ReceivePacket(InworldPacket incomingPacket)
        {
            try
            {
                if (!IsRelated(incomingPacket))
                {
                    return;
                }

                switch (incomingPacket?.routing?.source?.type.ToUpper())
                {
                    case "AGENT":
                        switch (incomingPacket)
                        {
                            case ControlPacket controlPacket:
                                _FinishCurrInteraction();
                                break;
                            case CustomPacket customPacket:
                                Dispatch(customPacket);
                                break;
                            default:
                                Add(incomingPacket);
                                break;
                        }
                        break;
                    case "PLAYER":
                        // Send Directly.
                        Dispatch(incomingPacket);
                        break;
                }
            }
            catch (Exception e)
            {
                InworldAI.LogException(e.Message);
            }
        }
        void Add(InworldPacket packet)
        {
            if (packet.packetId == null || string.IsNullOrEmpty(packet.packetId.interactionId))
            {
                return;
            }
            Interaction overdueInteraction = HistoryItem.FirstOrDefault(i => i.InteractionID == packet.packetId.interactionId);
            if (overdueInteraction != null)
            {
                Dispatch(packet);
                return;
            }
            if (m_ProcessingInteraction.InteractionID != packet.packetId.interactionId)
            {
                if (!string.IsNullOrEmpty(m_ProcessingInteraction.InteractionID))
                    _FinishCurrInteraction();
                m_ProcessingInteraction = new Interaction
                {
                    InteractionID = packet.packetId.interactionId,
                    Status = PacketStatus.RECEIVED
                };
            }
            Utterance utterance = m_ProcessingInteraction[packet.packetId.utteranceId] ?? new Utterance(packet.packetId.utteranceId);
            utterance.Packets.Add(packet);
            if (!m_ProcessingInteraction.Utterances.Contains(utterance))
            {
                m_ProcessingInteraction.Utterances.Add(utterance);
            }
        }
        void _FinishCurrInteraction()
        {
            if (!HistoryItem.Contains(m_ProcessingInteraction))
                HistoryItem.Add(m_ProcessingInteraction);
        }
    }
}

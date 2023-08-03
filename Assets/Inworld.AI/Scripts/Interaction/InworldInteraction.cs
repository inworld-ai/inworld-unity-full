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
        public float AudioLength { get; set; }
        public bool Interruptable
        {
            get => m_Interruptable;
            set => m_Interruptable = value;
        }
        public Utterance CurrentUtterance { get; set; }
        
        float m_CurrentTime;
        bool m_IsSpeaking;
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
        public event Action<List<InworldPacket>> OnInteractionChanged;
        public event Action<bool> OnStartStopInteraction;
        public Interaction this[string interactionID] => HistoryItem.FirstOrDefault(i => i.InteractionID == interactionID);
        public Utterance NextUtterance => HistoryItem
                                          .Where(i => i.Status == PacketStatus.RECEIVED)
                                          .SelectMany(item => item.Utterances)
                                          .FirstOrDefault(utterance => utterance.Status == PacketStatus.RECEIVED);

        public bool IsRelated(InworldPacket packet) => !string.IsNullOrEmpty(LiveSessionID) 
            && (packet.routing.source.name == LiveSessionID || packet.routing.target.name == LiveSessionID);
        
        void OnEnable()
        {
            InworldController.Client.OnPacketReceived += ReceivePacket;
        }
        void OnDisable()
        {
            if (InworldController.Instance)
                InworldController.Client.OnPacketReceived -= ReceivePacket;
        }
        void Update()
        {
            if (HistoryItem.Count > m_MaxItemCount)
                RemoveHistoryItem();
            
            m_CurrentTime += Time.deltaTime;
            if (m_CurrentTime < m_TextDuration)
                return;
            m_CurrentTime = 0;
            PlayNextUtterance();
        }

        protected void Dispatch(List<InworldPacket> packet) => OnInteractionChanged?.Invoke(packet);

        protected void UpdateInteraction(InworldPacket packet)
        {
            Interaction interaction = this[packet.packetId.interactionId];
            CurrentUtterance = interaction?[packet.packetId.utteranceId];
            if (CurrentUtterance == null)
                return;
            CurrentUtterance.Status = PacketStatus.PLAYED;
            if (interaction != null && interaction.Utterances.All(u => u.Status != PacketStatus.RECEIVED))
                interaction.Status = PacketStatus.PLAYED;
        }

        protected virtual void PlayNextUtterance()
        {
            Utterance utterance = NextUtterance;
            if (utterance == null)
            {
                IsSpeaking = false;
                return;
            }
            UpdateInteraction(utterance.Packets[0]);
            Dispatch(utterance.Packets);
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
                        Add(incomingPacket);
                        break;
                    case "PLAYER":
                        // Send Directly.
                        OnInteractionChanged?.Invoke
                        (
                            new List<InworldPacket>
                            {
                                incomingPacket
                            }
                        );
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        void Add(InworldPacket packet)
        {
            if (packet.packetId == null || string.IsNullOrEmpty(packet.packetId.interactionId))
                return;
            Interaction interaction = this[packet.packetId.interactionId] ?? new Interaction(packet.packetId.interactionId);
            Utterance utterance = interaction[packet.packetId.utteranceId] ?? new Utterance(packet.packetId.utteranceId);
            interaction.Status = PacketStatus.RECEIVED; // Refresh Interaction Status.
            utterance.Packets.Add(packet);
            
            if (!interaction.Utterances.Contains(utterance))
            {
                interaction.Utterances.Add(utterance);
            }

            if (!HistoryItem.Contains(interaction))
                HistoryItem.Add(interaction);

            if (CurrentUtterance != null && packet.packetId.utteranceId == CurrentUtterance.UtteranceID || packet is CustomPacket)
            {
                // YAN: Send Overdue packets and trigger
                OnInteractionChanged?.Invoke(utterance.Packets);
            } 
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
    }
}

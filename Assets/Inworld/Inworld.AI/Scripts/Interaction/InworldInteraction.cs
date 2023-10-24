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
        [SerializeField] float m_TextSpeed = 1.0f;
        float m_CurrentTime;
        bool m_IsSpeaking;
        protected int m_LastInteractionSequenceNumber;
        protected int m_SequenceNumber;
        protected List<Interaction> m_History = new List<Interaction>();
        protected Interaction m_CurrentInteraction;
        protected Utterance m_CurrentUtterance;
        
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
        public Queue<Utterance> UtteranceQueue { get; } = new Queue<Utterance>();
        public event Action<List<InworldPacket>> OnInteractionChanged;
        public event Action<bool> OnStartStopInteraction;
        public Interaction FindInteraction(string interactionID) => m_History.FirstOrDefault(i => i.InteractionID == interactionID);

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
            if (m_CurrentTime > 0)
            {
                m_CurrentTime -= Time.deltaTime;
                return;
            }
                
            PlayNextUtterance();
        }

        protected void Dispatch(InworldPacket packet)
        {
            OnInteractionChanged?.Invoke
            (
                new List<InworldPacket>
                {
                    packet
                }
            );
            packet.packetId.Status = PacketStatus.PROCESSED;
        }

        public virtual void CancelResponse()
        {
            if (string.IsNullOrEmpty(LiveSessionID) || m_CurrentInteraction == null || !Interruptable)
                return;
            
            InworldController.Instance.SendCancelEvent(LiveSessionID, m_CurrentInteraction.InteractionID);

            m_CurrentInteraction.Cancel();
            while (UtteranceQueue.Peek().Interaction == m_CurrentInteraction)
                UtteranceQueue.Dequeue();
            
            m_CurrentUtterance = null;
            m_CurrentInteraction = null;
            m_CurrentTime = 0;
        }
        protected virtual void PlayNextUtterance()
        {
            if (m_CurrentUtterance != null)
            {
                m_CurrentUtterance.GetTextPacket().packetId.Status = PacketStatus.PLAYED;
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
            TextPacket textPacket = m_CurrentUtterance.GetTextPacket();

            const float timePerChar = 0.05f;
            
            m_CurrentTime = (textPacket.text.text.Length * timePerChar) / m_TextSpeed;
            
            m_CurrentInteraction = m_CurrentUtterance.Interaction;
            m_LastInteractionSequenceNumber = m_CurrentInteraction.SequenceNumber;

            if(m_CurrentInteraction.Status == InteractionStatus.CREATED)
                m_CurrentInteraction.Status = InteractionStatus.STARTED;
            
            Dispatch(textPacket);
            m_CurrentUtterance.Status = InteractionStatus.STARTED;
        }

        protected void RemoveHistoryItem()
        {
            Interaction toDelete = m_History.FirstOrDefault(i => i.Status != InteractionStatus.STARTED);
            
            if (toDelete != null)
                m_History.Remove(toDelete);
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
                        HandleAgentPacket(incomingPacket);
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

        protected virtual void HandleAgentPacket(InworldPacket inworldPacket)
        {
            Tuple<Interaction, Utterance> historyItem = AddToHistory(inworldPacket);
            Interaction interaction = historyItem.Item1;
            
            switch (inworldPacket)
            {
                case ControlPacket:
                    historyItem.Item1.ReceivedInteractionEnd = true;
                    inworldPacket.packetId.Status = PacketStatus.PROCESSED;
                    UpdateHistory(historyItem.Item1);
                    break;
                case AudioPacket:
                    // Ignore Audio Packets
                    inworldPacket.packetId.Status = PacketStatus.PROCESSED;
                    break;
                case TextPacket:
                    if (interaction == m_CurrentInteraction ||
                        interaction.SequenceNumber > m_LastInteractionSequenceNumber)
                        QueueUtterance(historyItem.Item2);
                    break;
                default:
                    Dispatch(inworldPacket);
                    break;
            }
        }

        protected virtual void QueueUtterance(Utterance utterance)
        {
            UtteranceQueue.Enqueue(utterance);
        }

        protected void UpdateHistory(Interaction interaction)
        {
            if (!interaction.ReceivedInteractionEnd)
                return;
            
            interaction.UpdateStatus();

            if (interaction.Status == InteractionStatus.COMPLETED && interaction == m_CurrentInteraction)
                m_CurrentInteraction = null;
        }
        
        protected Tuple<Interaction, Utterance> AddToHistory(InworldPacket packet)
        {
            if (packet.packetId == null || string.IsNullOrEmpty(packet.packetId.interactionId))
                return null;

            Interaction interaction = m_History.FirstOrDefault(i => i.InteractionID == packet.packetId.interactionId);
            if (interaction == null)
            {
                if (m_History.Count == m_MaxItemCount)
                    RemoveHistoryItem();
                interaction = new Interaction(packet.packetId.interactionId, ++m_SequenceNumber);
                m_History.Add(interaction);
            }

            Utterance utterance = interaction[packet.packetId.utteranceId] ?? CreateUtterance(interaction, packet.packetId.utteranceId);
            utterance.Packets.Add(packet);
            if (!interaction.Utterances.Contains(utterance))
                interaction.Utterances.Add(utterance);
            return new Tuple<Interaction, Utterance>(interaction, utterance);
        }

        protected virtual Utterance CreateUtterance(Interaction interaction, string utteranceId)
        {
            return new Utterance(interaction, utteranceId);
        }
    }
}

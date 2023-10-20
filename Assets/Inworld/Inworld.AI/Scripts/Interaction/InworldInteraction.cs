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
        protected List<Interaction> m_History { get; } = new List<Interaction>();
        
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

        protected Interaction m_CurrentInteraction;
        protected Utterance m_CurrentUtterance;

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

        protected void Dispatch(InworldPacket packet) => OnInteractionChanged?.Invoke(new List<InworldPacket> {packet});
        protected void Dispatch(List<InworldPacket> packets) => OnInteractionChanged?.Invoke(packets);

        public virtual void CancelResponse()
        {
            if (string.IsNullOrEmpty(LiveSessionID) || m_CurrentInteraction == null || !Interruptable)
                return;
            
            m_CurrentInteraction.Status = InteractionStatus.CANCELLED;
            InworldController.Instance.SendCancelEvent(LiveSessionID, m_CurrentInteraction.InteractionID);
            
            m_CurrentUtterance = null;
            m_CurrentInteraction = null;
        }
        protected virtual void PlayNextUtterance()
        {
            if (m_CurrentUtterance != null)
                UpdateHistory(m_CurrentUtterance, UtteranceStatus.COMPLETED);
            
            if (UtteranceQueue.Count == 0)
            {
                IsSpeaking = false;
                m_CurrentUtterance = null;
                m_CurrentInteraction = null;
                return;
            }

            m_CurrentTime = m_TextDuration;
            m_CurrentUtterance = UtteranceQueue.Dequeue();
            
            if(m_CurrentInteraction != null && m_CurrentInteraction != m_CurrentUtterance.Interaction)
                InworldAI.LogException("Attempted to play utterance for an interaction that was not the current interaction.");
            
            m_CurrentInteraction = m_CurrentUtterance.Interaction;
            
            Dispatch(m_CurrentUtterance.GetTextPacket());
            UpdateHistory(m_CurrentUtterance, UtteranceStatus.STARTED);
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
            switch (inworldPacket)
            {
                case ControlPacket:
                    historyItem.Item1.RecievedInteractionEnd = true;
                    break;
                case TextPacket:
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

        protected void UpdateHistory(Utterance utterance, UtteranceStatus utteranceStatus)
        {
            Interaction interaction = utterance.Interaction;
            utterance.Status = utteranceStatus;

            InteractionStatus interactionStatus = interaction.Status;
            if (interactionStatus is InteractionStatus.COMPLETED or InteractionStatus.CANCELLED)
                return;

            if (interaction.RecievedInteractionEnd && interaction.Utterances.All(u => u.Status == UtteranceStatus.COMPLETED))
            {
                interaction.Status = InteractionStatus.COMPLETED;
                m_CurrentInteraction = null;
            }
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
                interaction = new Interaction(packet.packetId.interactionId);
                m_History.Add(interaction);
            }

            if (m_CurrentInteraction == null)
                m_CurrentInteraction = interaction;

            Utterance utterance = interaction[packet.packetId.utteranceId] ?? new Utterance(interaction, packet.packetId.utteranceId);
            utterance.Packets.Add(packet);
            if (!interaction.Utterances.Contains(utterance))
                interaction.Utterances.Add(utterance);
            return new Tuple<Interaction, Utterance>(interaction, utterance);
        }

        Utterance FindUtterance(string interactionId, string utteranceId)
        {
            Interaction interaction = m_History.FirstOrDefault(i => i.InteractionID == interactionId);
            return interaction?[utteranceId];
        }
    }
}

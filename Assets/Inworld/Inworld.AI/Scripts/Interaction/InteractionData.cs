using System.Collections.Generic;
using System.Linq;
using Inworld.Packet;
using System;

namespace Inworld.Interactions
{
    public class Interaction
    {
        public string InteractionID { get; set; }
        public List<Utterance> Utterances { get; set; } = new List<Utterance>();
        public InteractionStatus Status { get; set; }
        public bool ReceivedInteractionEnd { get; set; }
        public int SequenceNumber { get; set; }
        public Utterance this[string utteranceID] => Utterances.FirstOrDefault(u => u.UtteranceID == utteranceID);
        public Interaction(string interactionID, int sequenceNumber)
        {
            InteractionID = interactionID;
            SequenceNumber = sequenceNumber;
            Status = InteractionStatus.CREATED;
        }
        public void UpdateStatus()
        {
            if (Status == InteractionStatus.CANCELLED)
                return;
            
            bool isComplete = true;
            foreach (var utterance in Utterances)
            {
                utterance.UpdateStatus();
                if (utterance.Status != InteractionStatus.COMPLETED)
                    isComplete = false;
            }
            if (isComplete)
                Status = InteractionStatus.COMPLETED;
        }
        public void Cancel()
        {
            foreach (var utterance in Utterances)
            {
                if (utterance.Status != InteractionStatus.COMPLETED)
                    utterance.Cancel();
            }
            Status = InteractionStatus.CANCELLED;
        }
    }

    public class AudioUtterance : Utterance
    {
        public AudioUtterance(Interaction interaction, string utteranceID) : base(interaction, utteranceID) {}
        
        public override void UpdateStatus()
        {
            if (Status == InteractionStatus.CANCELLED)
                return;
            
            bool isComplete = true;
            foreach (InworldPacket packet in Packets)
            {
                switch (packet)
                {
                    case TextPacket:
                    case AudioPacket:
                        if (packet.packetId.Status != PacketStatus.PLAYED)
                            isComplete = false;
                        break;
                    default:
                        if (packet.packetId.Status != PacketStatus.PROCESSED)
                            isComplete = false;
                        break;
                }
                if (!isComplete)
                    break;
            }
            if (isComplete)
                Status = InteractionStatus.COMPLETED;
        }

        public override void Cancel()
        {
            foreach (InworldPacket packet in Packets)
            {
                switch (packet)
                {
                    case TextPacket:
                    case AudioPacket:
                        if (packet.packetId.Status != PacketStatus.PLAYED)
                            packet.packetId.Status = PacketStatus.CANCELLED;
                        break;
                    default:
                        if (packet.packetId.Status != PacketStatus.PROCESSED)
                            packet.packetId.Status = PacketStatus.CANCELLED;
                        break;
                }
            }
            Status = InteractionStatus.CANCELLED;
        }
    }
    
    public class Utterance
    {
        public Interaction Interaction { get; set; }
        public string UtteranceID { get; set; }
        public List<InworldPacket> Packets { get; set; } = new List<InworldPacket>();
        public InteractionStatus Status { get; set; }
        public Utterance(Interaction interaction, string utteranceID)
        {
            Interaction = interaction;
            UtteranceID = utteranceID;
            Status = InteractionStatus.CREATED;
        }
        public TextPacket GetTextPacket()
        {
            return Packets.FirstOrDefault(packet => packet is TextPacket) as TextPacket;
        }
                
        public AudioPacket GetAudioPacket()
        {
            return Packets.FirstOrDefault(packet => packet is AudioPacket) as AudioPacket;
        }

        public virtual void UpdateStatus()
        {
            if (Status == InteractionStatus.CANCELLED)
                return;
            
            bool isComplete = true;
            foreach (InworldPacket packet in Packets)
            {
                switch (packet)
                {
                    case TextPacket:
                        if (packet.packetId.Status != PacketStatus.PLAYED)
                            isComplete = false;
                        break;
                    default:
                        if (packet.packetId.Status != PacketStatus.PROCESSED)
                            isComplete = false;
                        break;
                }
                if (!isComplete)
                    break;
            }
            if (isComplete)
                Status = InteractionStatus.COMPLETED;
        }

        public virtual void Cancel()
        {
            foreach (InworldPacket packet in Packets)
            {
                switch (packet)
                {
                    case TextPacket:
                        if (packet.packetId.Status != PacketStatus.PLAYED)
                            packet.packetId.Status = PacketStatus.CANCELLED;
                        break;
                    default:
                        if (packet.packetId.Status != PacketStatus.PROCESSED)
                            packet.packetId.Status = PacketStatus.CANCELLED;
                        break;
                }
            }
            Status = InteractionStatus.CANCELLED;
        }
    }
}

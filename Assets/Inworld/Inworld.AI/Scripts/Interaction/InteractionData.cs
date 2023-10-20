using System.Collections.Generic;
using System.Linq;
using Inworld.Packet;
using System;

namespace Inworld.Interactions
{
    // public class AudioEvent : UtteranceEvent
    // {
    //     AudioPacket m_AudioPacket;
    //     public AudioEvent(PacketId packetId) : base(packetId)
    //     {
    //         
    //     }
    //     public void AddPacket(AudioPacket audioPacket)
    //     {
    //         m_AudioPacket = audioPacket;
    //     }
    //     public bool HasAudioPacket()
    //     {
    //         return m_AudioPacket != null;
    //     }
    //     public AudioPacket GetAudioPacket()
    //     {
    //         return m_AudioPacket;
    //     }
    // }
    // public class UtteranceEvent
    // {
    //     public readonly string InteractionId;
    //     public readonly string UtteranceId;
    //     TextPacket m_TextPacket;
    //
    //     public UtteranceEvent(PacketId packetId)
    //     {
    //         InteractionId = packetId.interactionId;
    //         UtteranceId = packetId.utteranceId;
    //     }
    //
    //     public void AddPacket(TextPacket textPacket)
    //     {
    //         m_TextPacket = textPacket;
    //     }
    //     public bool HasTextPacket()
    //     {
    //         return m_TextPacket != null;
    //     }
    //     public TextPacket GetTextPacket()
    //     {
    //         return m_TextPacket;
    //     }
    // }
    public class Interaction
    {
        public string InteractionID { get; set; }
        public List<Utterance> Utterances { get; set; } = new List<Utterance>();
        public InteractionStatus Status { get; set; }
        public bool RecievedInteractionEnd { get; set; }
        public Utterance this[string utteranceID] => Utterances.FirstOrDefault(u => u.UtteranceID == utteranceID);
        public Interaction(string interactionID)
        {
            InteractionID = interactionID;
            Status = InteractionStatus.CREATED;
        }
    }
    public class Utterance
    {
        public Interaction Interaction { get; set; }
        public string UtteranceID { get; set; }
        public List<InworldPacket> Packets { get; set; } = new List<InworldPacket>();
        public UtteranceStatus Status { get; set; }
        public Utterance(Interaction interaction, string utteranceID)
        {
            Interaction = interaction;
            UtteranceID = utteranceID;
            Status = UtteranceStatus.CREATED;
        }
        public TextPacket GetTextPacket()
        {
            return Packets.FirstOrDefault(packet => packet is TextPacket) as TextPacket;
        }
        
        public AudioPacket GetAudioPacket()
        {
            return Packets.FirstOrDefault(packet => packet is AudioPacket) as AudioPacket;
        }
    }
}

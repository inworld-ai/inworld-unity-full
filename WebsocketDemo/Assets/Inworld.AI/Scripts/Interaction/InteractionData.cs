using System.Collections.Generic;
using System.Linq;
using Inworld.Packet;
using System.Collections.Concurrent;

namespace Inworld.Interactions
{
    public class Interaction
    {
        public string InteractionID { get; set; }
        public List<Utterance> Utterances { get; set; } = new List<Utterance>();
        public PacketStatus Status { get; set; }
        public Utterance this[string utteranceID] => Utterances.FirstOrDefault(u => u.UtteranceID == utteranceID);

        public Interaction()
        {
            Status = PacketStatus.RECEIVED;
        }
        public Interaction(string interactionID)
        {
            InteractionID = interactionID;
            Status = PacketStatus.RECEIVED;
        }
    }
    public class Utterance
    {
        public string UtteranceID { get; set; }
        public List<InworldPacket> Packets { get; set; } = new List<InworldPacket>();
        public PacketStatus Status { get; set; }
        public Utterance()
        {
            Status = PacketStatus.RECEIVED;
        }
        public Utterance(string utteranceID)
        {
            UtteranceID = utteranceID;
            Status = PacketStatus.RECEIVED;
        }
    }
}

using System;
using System.Collections.Generic;

namespace Inworld.Packet
{
    [Serializable]
    public class CancelResponseEvent
    {
        public string interactionId;
        public List<string> utteranceId;
    }
    [Serializable]
    public class CancelResponsePacket : InworldPacket
    {
        public CancelResponseEvent cancelResponses;
        
        public CancelResponsePacket()
        {
            cancelResponses = new CancelResponseEvent();
        }
        public CancelResponsePacket(InworldPacket rhs, CancelResponseEvent evt) : base(rhs)
        {
            cancelResponses = evt;
        }
    }
}

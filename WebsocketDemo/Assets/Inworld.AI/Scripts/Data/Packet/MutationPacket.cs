using System;


namespace Inworld.Packet
{
    [Serializable]
    public class CancelResponse
    {
        public string interactionId;
    }
    [Serializable]
    public class MutationEvent
    {
        public CancelResponse cancelResponses;
    }
    [Serializable]
    public class MutationPacket : InworldPacket
    {
        public MutationEvent mutation;
        
        public MutationPacket()
        {
            mutation = new MutationEvent();
        }
        public MutationPacket(InworldPacket rhs, MutationEvent evt) : base(rhs)
        {
            mutation = evt;
        }
    }
}

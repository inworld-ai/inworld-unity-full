using System;

namespace Inworld.Packet
{
    [Serializable]
    public class ControlEvent
    {
        public string action;
        public string description;
    }
    [Serializable]
    public class ControlPacket : InworldPacket
    {
        public ControlEvent control;

        public ControlPacket()
        {
            type = "CONTROL";
            control = new ControlEvent();
        }
        public ControlPacket(InworldPacket rhs, ControlEvent evt) : base(rhs)
        {
            type = "CONTROL";
            control = evt;
        }
    }
}

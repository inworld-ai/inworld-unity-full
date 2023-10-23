using System;
namespace Inworld.Packet
{
    [Serializable]
    public class GestureEvent
    {
        public string type;
        public string playback;
    }
    [Serializable]
    public class GesturePacket : InworldPacket
    {
        public GestureEvent gesture;
        
        public GesturePacket()
        {
            type = "GESTURE";
            gesture = new GestureEvent();
        }
        public GesturePacket(InworldPacket rhs, GestureEvent evt) : base(rhs)
        {
            type = "GESTURE";
            gesture = evt;
        }
    }
}

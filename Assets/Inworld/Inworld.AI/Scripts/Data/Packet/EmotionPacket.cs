using System;
namespace Inworld.Packet
{
    [Serializable]
    public class EmotionEvent
    {
        public float joy;
        public float fear;
        public float trust;
        public float surprise;
        public string behavior;
        public string strength;
        public override string ToString() => $"{strength} {behavior}";
    }
    [Serializable]
    public class EmotionPacket : InworldPacket
    {
        public EmotionEvent emotion;
        
        public EmotionPacket()
        {
            emotion = new EmotionEvent();
        }
        public EmotionPacket(InworldPacket rhs, EmotionEvent evt) : base(rhs)
        {
            emotion = evt;
        }
    }
}

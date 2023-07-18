using System;
namespace Inworld.Packet
{
    [Serializable]
    public class TextEvent
    {
        public string text;
        public string sourceType;
        public bool final;

        public TextEvent(string textToSend = "")
        {
            text = textToSend;
            sourceType = "TYPED_IN";
            final = true;
        }
    }
    [Serializable]
    public class TextPacket : InworldPacket
    {
        public TextEvent text;
        public TextPacket()
        {
            text = new TextEvent();
        }
        public TextPacket(InworldPacket rhs, TextEvent evt) : base(rhs)
        {
            text = evt;
        }
    }
}

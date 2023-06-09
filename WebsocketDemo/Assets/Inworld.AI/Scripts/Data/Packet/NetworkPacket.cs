using System;

namespace Inworld.Packet
{
    [Serializable]
    public class NetworkPacketResponse
    {
        public InworldNetworkPacket result;
    }
    [Serializable]
    public class InworldNetworkPacket : InworldPacket
    {
        public TextEvent text;
        public ControlEvent control;
        public DataChunk dataChunk;
        public GestureEvent gesture;
        public CustomEvent custom;
        public CancelResponseEvent cancelResponses;
        public EmotionEvent emotion;
        public ActionEvent action;

        public InworldPacket Packet
        {
            get
            {
                if (text != null && !string.IsNullOrEmpty(text.text))
                    return new TextPacket(this, text);
                if (control != null && !string.IsNullOrEmpty(control.action))
                    return new ControlPacket(this, control);
                if (dataChunk != null && !string.IsNullOrEmpty(dataChunk.chunk) && dataChunk.type == "AUDIO")
                    return new AudioPacket(this, dataChunk);
                if (gesture != null && !string.IsNullOrEmpty(gesture.type))
                    return new GesturePacket(this, gesture);
                if (custom != null && !string.IsNullOrEmpty(custom.name))
                    return new CustomPacket(this, custom);
                if (cancelResponses != null && !string.IsNullOrEmpty(cancelResponses.interactionId))
                    return new CancelResponsePacket(this, cancelResponses);
                if (emotion != null && !string.IsNullOrEmpty(emotion.behavior))
                    return new EmotionPacket(this, emotion);
                if (action != null && !string.IsNullOrEmpty(action.content))
                    return new ActionPacket(this, action);
                return this;
            }
        }
        public PacketType Type
        {
            get
            {
                if (text != null && !string.IsNullOrEmpty(text.text))
                    return PacketType.TEXT;
                if (control != null && !string.IsNullOrEmpty(control.action))
                    return PacketType.CONTROL;
                if (dataChunk != null && !string.IsNullOrEmpty(dataChunk.chunk) && dataChunk.type == "AUDIO")
                    return PacketType.AUDIO;
                if (gesture != null && !string.IsNullOrEmpty(gesture.type))
                    return PacketType.GESTURE;
                if (custom != null && !string.IsNullOrEmpty(custom.name))
                    return PacketType.CUSTOM;
                if (cancelResponses != null && !string.IsNullOrEmpty(cancelResponses.interactionId))
                    return PacketType.CANCEL_RESPONSE;
                if (emotion != null && !string.IsNullOrEmpty(emotion.behavior))
                    return PacketType.EMOTION;
                if (action != null && !string.IsNullOrEmpty(action.content))
                    return PacketType.ACTION;
                return PacketType.UNKNOWN;
            }
        }
    }
}

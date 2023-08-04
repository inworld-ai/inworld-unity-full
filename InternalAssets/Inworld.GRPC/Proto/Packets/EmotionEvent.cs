using System;

namespace Inworld.Packets
{
    public class EmotionEvent : InworldPacket
    {
        public DateTime Timestamp { get; set; }
        public PacketId PacketId { get; set; }
        public Routing Routing { get; set; }

        public Grpc.EmotionEvent.Types.SpaffCode SpaffCode { get; set; }
        
        public Grpc.EmotionEvent.Types.Strength Strength { get; set; }

        public EmotionEvent()
        {
            Timestamp = DateTime.UtcNow;
            PacketId = new PacketId();
            SpaffCode = Grpc.EmotionEvent.Types.SpaffCode.Neutral;
            Strength = Grpc.EmotionEvent.Types.Strength.Normal;
        }
        
        public EmotionEvent(Grpc.InworldPacket packet)
        {
            Timestamp = packet.Timestamp.ToDateTime();
            Routing = new Routing(packet.Routing);
            PacketId = new PacketId(packet.PacketId);
            SpaffCode = packet.Emotion.Behavior;
            Strength = packet.Emotion.Strength;
        }
        
        public Grpc.InworldPacket ToGrpc()
        {
            return new Grpc.InworldPacket
            {
                Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(this.Timestamp),
                Routing = Routing.ToGrpc(),
                PacketId = PacketId.ToGrpc(),
                Emotion = new Grpc.EmotionEvent
                {
                    Behavior = SpaffCode,
                    Strength = Strength
                }
            };
        }

        protected bool Equals(EmotionEvent other)
        {
            return other.SpaffCode == SpaffCode && other.Strength == Strength && Timestamp.Equals(other.Timestamp) && Equals(PacketId, other.PacketId) && Equals(Routing, other.Routing);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) 
                return false;
            if (ReferenceEquals(this, obj)) 
                return true;
            if (obj.GetType() != GetType()) 
                return false;
            return Equals((EmotionEvent) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SpaffCode.GetHashCode();
                hashCode = (hashCode * 397) ^ Strength.GetHashCode();
                hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ (PacketId != null ? PacketId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Routing != null ? Routing.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
    
    
}
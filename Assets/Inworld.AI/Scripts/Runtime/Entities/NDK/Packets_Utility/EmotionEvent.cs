using System;
#if INWORLD_NDK
using GrpcSpaffCode= Inworld.ProtoBuf.EmotionEvent.Types.SpaffCode;
using GrpcStrength = Inworld.ProtoBuf.EmotionEvent.Types.Strength;
using GrpcEmotionEvent = Inworld.ProtoBuf.EmotionEvent;
#else
using GrpcSpaffCode = Inworld.Grpc.EmotionEvent.Types.SpaffCode;
using GrpcStrength = Inworld.Grpc.EmotionEvent.Types.Strength;
using GrpcEmotionEvent = Inworld.Grpc.EmotionEvent;
#endif

namespace Inworld.Packets
{
    public class EmotionEvent : InworldPacket
    {
        public DateTime Timestamp { get; set; }
        public PacketId PacketId { get; set; }
        public byte[] PacketBytes { get; set; }
        public Routing Routing { get; set; }

        public GrpcSpaffCode SpaffCode { get; set; }
        
        public GrpcStrength Strength { get; set; }

        public EmotionEvent(byte[] packetBytes)
        {
            PacketBytes = packetBytes;
            var packet = InworldPacketGenerator.Instance.ToProtobufPacket(this);
            Timestamp = packet.Timestamp.ToDateTime();
            PacketId = InworldPacketGenerator.Instance.FromProtoPacketId(packet.PacketId);
            Routing = InworldPacketGenerator.Instance.FromProtoRouting(packet.Routing);
            SpaffCode = packet.Emotion.Behavior;
            Strength = packet.Emotion.Strength;
        }
        // public GrpcPacket ToGrpc()
        // {
        //     return new GrpcPacket
        //     {
        //         Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(this.Timestamp),
        //         Routing = Routing.ToGrpc(),
        //         PacketId = PacketId.ToGrpc(),
        //         Emotion = new GrpcEmotionEvent
        //         {
        //             Behavior = this.SpaffCode,
        //             Strength = this.Strength
        //         }
        //     };
        // }

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
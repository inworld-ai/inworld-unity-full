using System;
#if INWORLD_NDK
using GrpcSpaffCode= Inworld.ProtoBuf.EmotionEvent.Types.SpaffCode;
using GrpcStrength = Inworld.ProtoBuf.EmotionEvent.Types.Strength;
using GrpcPacket = Inworld.ProtoBuf.InworldPacket;
using GrpcPacketID = Inworld.ProtoBuf.PacketId;
using GrpcRouting = Inworld.ProtoBuf.Routing;
using GrpcActor = Inworld.ProtoBuf.Actor;
using ActorTypes = Inworld.ProtoBuf.Actor.Types;  
using GrpcEmotionEvent = Inworld.ProtoBuf.EmotionEvent;
#else
using GrpcSpaffCode = Inworld.Grpc.EmotionEvent.Types.SpaffCode;
using GrpcStrength = Inworld.Grpc.EmotionEvent.Types.Strength;
using GrpcPacket = Inworld.Grpc.InworldPacket;
using GrpcPacketID = Inworld.Grpc.PacketId;
using GrpcRouting = Inworld.Grpc.Routing;
using GrpcActor = Inworld.Grpc.Actor;
using ActorTypes = Inworld.Grpc.Actor.Types;
using GrpcEmotionEvent = Inworld.Grpc.EmotionEvent;
#endif

namespace Inworld.Packets
{
    public class EmotionEvent : InworldPacket
    {
        public DateTime Timestamp { get; set; }
        public PacketId PacketId { get; set; }
        public Routing Routing { get; set; }

        public GrpcSpaffCode SpaffCode { get; set; }
        
        public GrpcStrength Strength { get; set; }

        public EmotionEvent()
        {
            Timestamp = DateTime.UtcNow;
            PacketId = new PacketId();
            //Necessary due to issues with static read only access
#if INWORLD_NDK
            SpaffCode = Inworld.ProtoBuf.EmotionEvent.Types.SpaffCode.Neutral;
            Strength =  Inworld.ProtoBuf.EmotionEvent.Types.Strength.Normal;
#else
            SpaffCode = Inworld.Grpc.EmotionEvent.Types.SpaffCode.Neutral;
            Strength =  Inworld.Grpc.EmotionEvent.Types.Strength.Normal;
#endif
        }
        
        public EmotionEvent(GrpcPacket packet)
        {
            Timestamp = packet.Timestamp.ToDateTime();
            Routing = new Routing(packet.Routing);
            PacketId = new PacketId(packet.PacketId);
            SpaffCode = packet.Emotion.Behavior;
            Strength = packet.Emotion.Strength;
        }
        
        public GrpcPacket ToGrpc()
        {
            return new GrpcPacket
            {
                Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(this.Timestamp),
                Routing = Routing.ToGrpc(),
                PacketId = PacketId.ToGrpc(),
                Emotion = new GrpcEmotionEvent
                {
                    Behavior = this.SpaffCode,
                    Strength = this.Strength
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
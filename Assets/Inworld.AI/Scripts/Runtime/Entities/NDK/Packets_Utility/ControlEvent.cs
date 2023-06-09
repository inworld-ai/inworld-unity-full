using System;
#if INWORLD_NDK
using GrpcControlEvent = Inworld.ProtoBuf.ControlEvent;
using GrpcPacket = Inworld.ProtoBuf.InworldPacket;
using GrpcPacketID = Inworld.ProtoBuf.PacketId;
using GrpcRouting = Inworld.ProtoBuf.Routing;
using GrpcActor = Inworld.ProtoBuf.Actor;
using ActorTypes = Inworld.ProtoBuf.Actor.Types;  
#else
using GrpcControlEvent = Inworld.Grpc.ControlEvent;
using GrpcPacket = Inworld.Grpc.InworldPacket;
using GrpcPacketID = Inworld.Grpc.PacketId;
using GrpcRouting = Inworld.Grpc.Routing;
using GrpcActor = Inworld.Grpc.Actor;
using ActorTypes = Inworld.Grpc.Actor.Types;
#endif

namespace Inworld.Packets
{
    public class ControlEvent : InworldPacket
    {
        public DateTime Timestamp { get; set; }
        public PacketId PacketId { get; set; }
        public Routing Routing { get; set; }
        
        public GrpcControlEvent.Types.Action Action;

        ControlEvent()
        {
            Timestamp = DateTime.UtcNow;
            PacketId = new PacketId();
        }

        public ControlEvent( GrpcControlEvent.Types.Action action, Routing routing): this()
        {
            Action = action;
            Routing = routing;
        }
        
        public ControlEvent(GrpcPacket packet)
        {
            Timestamp = packet.Timestamp.ToDateTime();
            Routing = new Routing(packet.Routing);
            PacketId = new PacketId(packet.PacketId);
            Action = packet.Control.Action;
        }
        
        public GrpcPacket ToGrpc()
        {
            return new GrpcPacket
            {
                Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(this.Timestamp),
                Routing = Routing.ToGrpc(),
                PacketId = PacketId.ToGrpc(),
                Control = new GrpcControlEvent() {Action = this.Action}
            };
        }

        protected bool Equals(ControlEvent other)
        {
            return Action == other.Action && Timestamp.Equals(other.Timestamp) && Equals(PacketId, other.PacketId) && Equals(Routing, other.Routing);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ControlEvent) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Action;
                hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ (PacketId != null ? PacketId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Routing != null ? Routing.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
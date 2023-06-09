using System;
using System.Collections.Generic;
using System.Diagnostics;
#if INWORLD_NDK
using GrpcPacket = Inworld.ProtoBuf.InworldPacket;
using GrpcPacketID = Inworld.ProtoBuf.PacketId;
using GrpcRouting = Inworld.ProtoBuf.Routing;
using GrpcActor = Inworld.ProtoBuf.Actor;
using GrpcTextEvent = Inworld.ProtoBuf.TextEvent;
using GrpcSourceType = Inworld.ProtoBuf.TextEvent.Types.SourceType;
using ActorTypes = Inworld.ProtoBuf.Actor.Types;  
#else
using GrpcPacket = Inworld.Grpc.InworldPacket;
using GrpcPacketID = Inworld.Grpc.PacketId;
using GrpcRouting = Inworld.Grpc.Routing;
using GrpcActor = Inworld.Grpc.Actor;
using GrpcTextEvent = Inworld.Grpc.TextEvent;
using GrpcSourceType = Inworld.Grpc.TextEvent.Types.SourceType;
using ActorTypes = Inworld.Grpc.Actor.Types;
#endif

namespace Inworld.Packets
{
    [DebuggerDisplay(
        "Timestamp={Timestamp}, EventId={PacketId}, Routing={Routing}, Text={Text}, SourceType = {SourceType}, Final={Final}, Id={Id}")]
    public class TextEvent : InworldPacket
    {
        public DateTime Timestamp { get; set; }
        public PacketId PacketId { get; set; }
        public Routing Routing { get; set; }
        public string Text { get; set; }
        public GrpcSourceType SourceType { get; set; }
        public bool Final { get; set; }

        public TextEvent()
        {
            Timestamp = DateTime.UtcNow;
            PacketId = new PacketId();
            Routing = new Routing();
        }

        public TextEvent(GrpcPacket grpcEvent)
        {
            Timestamp = grpcEvent.Timestamp.ToDateTime();
            Routing = new Routing(grpcEvent.Routing);

            PacketId = new PacketId(grpcEvent.PacketId);
            Text = grpcEvent.Text.Text;
            SourceType = grpcEvent.Text.SourceType;
            Final = grpcEvent.Text.Final;
        }

        public GrpcPacket ToGrpc() =>
            new GrpcPacket
            {
                Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(Timestamp),
                Routing = Routing?.ToGrpc(),
                PacketId = PacketId.ToGrpc(),
                Text = new GrpcTextEvent {Text = Text, SourceType = SourceType, Final = Final}
            };

        protected bool Equals(TextEvent other) =>
            Timestamp.Equals(other.Timestamp) && 
            Equals(PacketId, other.PacketId) && 
            Equals(Routing, other.Routing) && 
            Text == other.Text && 
            SourceType == other.SourceType && 
            Final == other.Final;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextEvent) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Timestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ (PacketId != null ? PacketId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Routing != null ? Routing.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Text != null ? Text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) SourceType;
                hashCode = (hashCode * 397) ^ Final.GetHashCode();
                return hashCode;
            }
        }
    }
}
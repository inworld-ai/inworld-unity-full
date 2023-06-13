using System;
#if INWORLD_NDK
using GrpcControlEvent = Inworld.ProtoBuf.ControlEvent;
#else
using GrpcControlEvent = Inworld.Grpc.ControlEvent;
#endif

namespace Inworld.Packets
{
    public class ControlEvent : InworldPacket
    {
        public DateTime Timestamp { get; set; }
        public PacketId PacketId { get; set; }
        public byte[] PacketBytes { get; set; }
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
        
        public ControlEvent(byte[] packetBytes)
        {
            PacketBytes = packetBytes;
            var packet = InworldPacketGenerator.Instance.ToProtobufPacket(this);
            Timestamp = packet.Timestamp.ToDateTime();
            PacketId = InworldPacketGenerator.Instance.FromProtoPacketId(packet.PacketId);
            Routing = InworldPacketGenerator.Instance.FromProtoRouting(packet.Routing);
            Action = packet.Control.Action;
        }
        // public GrpcPacket ToGrpc()
        // {
        //     return new GrpcPacket
        //     {
        //         Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(this.Timestamp),
        //         Routing = Routing.ToGrpc(),
        //         PacketId = PacketId.ToGrpc(),
        //         Control = new GrpcControlEvent() {Action = this.Action}
        //     };
        // }

        // public ControlEvent(PacketId id, DateTime timestamp, GrpcControlEvent.Types.Action action, Routing routing): this()
        // {
        //     PacketId = id;
        //     Timestamp = timestamp;
        //     Action = action;
        //     Routing = routing;
        // }

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
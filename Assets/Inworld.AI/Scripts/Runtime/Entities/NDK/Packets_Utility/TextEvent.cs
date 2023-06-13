using System;
using System.Collections.Generic;
using System.Diagnostics;
#if INWORLD_NDK
using GrpcSourceType = Inworld.ProtoBuf.TextEvent.Types.SourceType;
#else
using GrpcSourceType = Inworld.Grpc.TextEvent.Types.SourceType;
#endif

namespace Inworld.Packets
{
    [DebuggerDisplay(
        "Timestamp={Timestamp}, EventId={PacketId}, Routing={Routing}, Text={Text}, SourceType = {SourceType}, Final={Final}, Id={Id}")]
    public class TextEvent : InworldPacket
    {
        public byte[] PacketBytes { get; set; }
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

        public TextEvent(byte[] packetBytes)
        {
            PacketBytes = packetBytes;
            var packet = InworldPacketGenerator.Instance.ToProtobufPacket(this);
            Timestamp = packet.Timestamp.ToDateTime();
            PacketId = InworldPacketGenerator.Instance.FromProtoPacketId(packet.PacketId);
            Routing = InworldPacketGenerator.Instance.FromProtoRouting(packet.Routing);
            Text = packet.Text.Text;
            SourceType = packet.Text.SourceType;
            Final = packet.Text.Final;
        }

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
using Inworld.Grpc;
using System;
using System.Diagnostics;
namespace Inworld.Packets
{
    [DebuggerDisplay("Timestamp={Timestamp}, EventId={PacketId}, Routing={Routing}")]
    public class ActionEvent: InworldPacket
    {
        public DateTime Timestamp { get; set; }
        public Routing Routing { get; set; }
        public PacketId PacketId { get; set; }
        public string Content { get; set; }
        
        ActionEvent()
        {
            Timestamp = DateTime.UtcNow;
            PacketId = new PacketId();
            Content = "";
        }

        public ActionEvent(string content, Routing routing): this()
        {
            Content = content;
            Routing = routing;
        }

        public ActionEvent(string content): this(content, new Routing()) { }

        public ActionEvent(Grpc.InworldPacket packet)
        {
            Timestamp = packet.Timestamp.ToDateTime();
            Routing = new Routing(packet.Routing);
            PacketId = new PacketId(packet.PacketId);
            Content = packet.Action.NarratedAction.Content;
        }
        
        public Grpc.InworldPacket ToGrpc() => new Grpc.InworldPacket
        {
            Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(Timestamp),
            Routing = Routing?.ToGrpc(),
            PacketId = PacketId.ToGrpc(),
            Action = new Grpc.ActionEvent
            {
                NarratedAction = new NarratedAction
                {
                    Content = this.Content
                },
                Playback = Playback.Utterance
            }
        };
        
        protected bool Equals(ActionEvent other)
        {
            return Content == other.Content && Timestamp.Equals(other.Timestamp) && Equals(PacketId, other.PacketId) && Equals(Routing, other.Routing);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ActionEvent) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Content != null ? Content.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ (PacketId != null ? PacketId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Routing != null ? Routing.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}

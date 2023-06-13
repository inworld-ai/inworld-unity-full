using System;

namespace Inworld.Packets
{
    public class ControlEvent : InworldPacket
    {
        public DateTime Timestamp { get; set; }
        public PacketId PacketId { get; set; }
        public Routing Routing { get; set; }
        
        public Grpc.ControlEvent.Types.Action Action;

        ControlEvent()
        {
            Timestamp = DateTime.UtcNow;
            PacketId = new PacketId();
        }

        public ControlEvent( Grpc.ControlEvent.Types.Action action, Routing routing): this()
        {
            Action = action;
            Routing = routing;
        }
        
        public ControlEvent(Grpc.InworldPacket packet)
        {
            Timestamp = packet.Timestamp.ToDateTime();
            Routing = new Routing(packet.Routing);
            PacketId = new PacketId(packet.PacketId);
            Action = packet.Control.Action;
        }
        
        public Grpc.InworldPacket ToGrpc()
        {
            return new Grpc.InworldPacket
            {
                Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(this.Timestamp),
                Routing = Routing.ToGrpc(),
                PacketId = PacketId.ToGrpc(),
                Control = new Grpc.ControlEvent() {Action = this.Action}
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Google.Protobuf.Collections;
#if INWORLD_NDK
using GrpcCancelEvent = Inworld.ProtoBuf.CancelResponsesEvent;
#else
using GrpcCancelEvent = Inworld.Grpc.CancelResponsesEvent;
#endif

namespace Inworld.Packets
{
    [DebuggerDisplay(
        "Timestamp={Timestamp}, EventId={PacketId}, Routing={Routing}, InteractionId={InteractionId}, UtterancesIds={UtteranceIds}")]
    public class CancelResponsesEvent : InworldPacket
    {
        public byte[] PacketBytes { get; set; }
        public DateTime Timestamp { get; set; }
        public PacketId PacketId { get; set; }

        public Routing Routing { get; set; }

        // Interaction Id to cancel.
        public string InteractionId;

        // Utterances Ids to cancel within given interaction.
        public ReadOnlyCollection<string> UtteranceIds;


        CancelResponsesEvent()
        {
            Timestamp = DateTime.UtcNow;
            PacketId = new PacketId();
            Routing = new Routing();
        }

        public CancelResponsesEvent(string interactionId, List<string> utteranceIds) : this()
        {
            Timestamp = DateTime.UtcNow;
            PacketId = new PacketId();
            InteractionId = interactionId;
            UtteranceIds = utteranceIds.AsReadOnly();
        }

        public CancelResponsesEvent(byte[] packetBytes) : this ()
        {
            PacketBytes = packetBytes;
            var packet = InworldPacketGenerator.Instance.ToProtobufPacket(this);
            Timestamp = packet.Timestamp.ToDateTime();
            PacketId = InworldPacketGenerator.Instance.FromProtoPacketId(packet.PacketId);
            Routing = InworldPacketGenerator.Instance.FromProtoRouting(packet.Routing);
            InteractionId = packet.CancelResponses.InteractionId;
            UtteranceIds = packet.CancelResponses.UtteranceId.ToList().AsReadOnly();
        }

        // public GrpcPacket PacketBytes()
        // {
        //     var result = new GrpcPacket
        //     {
        //         Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(this.Timestamp),
        //         Routing = this.Routing.ToGrpc(),
        //         PacketId = PacketId.ToByteArray(),
        //         CancelResponses = new GrpcCancelEvent() {InteractionId = InteractionId}
        //     };
        //     result.CancelResponses.UtteranceId.AddRange(UtteranceIds);
        //     return result;
        // }

        protected bool Equals(CancelResponsesEvent other)
        {
            return InteractionId == other.InteractionId && UtteranceIds.SequenceEqual(other.UtteranceIds) &&
                   Timestamp.Equals(other.Timestamp) && Equals(PacketId, other.PacketId) &&
                   Equals(Routing, other.Routing);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CancelResponsesEvent) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (InteractionId != null ? InteractionId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (UtteranceIds != null ? UtteranceIds.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ (PacketId != null ? PacketId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Routing != null ? Routing.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return
                $"{nameof(InteractionId)}: {InteractionId}, {nameof(UtteranceIds)}: {UtteranceIds}, {nameof(Timestamp)}: {Timestamp}, {nameof(PacketId)}: {PacketId}, {nameof(Routing)}: {Routing}";
        }
    }
}
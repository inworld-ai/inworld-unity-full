using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Diagnostics;
#if INWORLD_NDK
using GrpcCustomEvent = Inworld.ProtoBuf.CustomEvent;
#else
using GrpcCustomEvent = Inworld.Grpc.CustomEvent;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace Inworld.Packets
{

    [DebuggerDisplay("Timestamp={Timestamp}, EventId={PacketId}, Routing={Routing}")]
    public class CustomEvent : InworldPacket
    {
        public DateTime Timestamp { get; set; }
        public PacketId PacketId { get; set; }
        public byte[] PacketBytes { get; set; }
        public Routing Routing { get; set; }
        public string TriggerName { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        CustomEvent()
        {
            Timestamp = DateTime.UtcNow;
            PacketId = new PacketId();
            Parameters = new Dictionary<string, string>();
        }

        public CustomEvent(string triggerName, Routing routing): this()
        {
            TriggerName = triggerName;
            Routing = routing;
        }

        public CustomEvent(string triggerName): this(triggerName, new Routing()) { }

        public CustomEvent(byte[] packetBytes)
        {
            PacketBytes = packetBytes;
            var packet = InworldPacketGenerator.Instance.ToProtobufPacket(this);
            Timestamp = packet.Timestamp.ToDateTime();
            PacketId = InworldPacketGenerator.Instance.FromProtoPacketId(packet.PacketId);
            Routing = InworldPacketGenerator.Instance.FromProtoRouting(packet.Routing);
            TriggerName = packet.Custom.Name;
            Parameters = new Dictionary<string, string>();
            foreach (GrpcCustomEvent.Types.Parameter param in packet.Custom.Parameters)
            {
                Parameters[param.Name] = param.Value;
            }
        }

        public void AddParameter(string key, string value) => Parameters[key] = value;

        public GrpcCustomEvent ParamToGrpc()
        {
            GrpcCustomEvent customEvent = new GrpcCustomEvent
            {
                Name = this.TriggerName
            };
            foreach (KeyValuePair<string, string> kvp in Parameters)
            {
                customEvent.Parameters.Add(new GrpcCustomEvent.Types.Parameter()
                {
                    Name = kvp.Key,
                    Value = kvp.Value
                });
            }
            return customEvent;
        }
        // public GrpcPacket ToGrpc() => new GrpcPacket
        // {
        //     Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(this.Timestamp),
        //     Routing = Routing.ToGrpc(),
        //     PacketId = PacketId.ToGrpc(),
        //     Custom = ParamToGrpc()
        // };

        protected bool Equals(CustomEvent other)
        {
            return TriggerName == other.TriggerName && Timestamp.Equals(other.Timestamp) && Equals(PacketId, other.PacketId) && Equals(Routing, other.Routing);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CustomEvent) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (TriggerName != null ? TriggerName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ (PacketId != null ? PacketId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Routing != null ? Routing.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{nameof(TriggerName)}: {TriggerName}, {nameof(Timestamp)}: {Timestamp}, {nameof(PacketId)}: {PacketId}, {nameof(Routing)}: {Routing}";
        }
    }
}
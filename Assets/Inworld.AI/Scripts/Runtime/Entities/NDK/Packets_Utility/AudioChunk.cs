using Google.Protobuf;
using Google.Protobuf.Collections;
using System;
using System.Diagnostics;
#if INWORLD_NDK
using Inworld.ProtoBuf;
using GrpcPacket = Inworld.ProtoBuf.InworldPacket;
using GrpcPacketID = Inworld.ProtoBuf.PacketId;
using GrpcRouting = Inworld.ProtoBuf.Routing;
using GrpcActor = Inworld.ProtoBuf.Actor;
using ActorTypes = Inworld.ProtoBuf.Actor.Types;
using AdditionalPhonemeInfo = Inworld.ProtoBuf.AdditionalPhonemeInfo;
#else
using Inworld.Grpc;
using GrpcPacket = Inworld.Grpc.InworldPacket;
using GrpcPacketID = Inworld.Grpc.PacketId;
using GrpcRouting = Inworld.Grpc.Routing;
using GrpcActor = Inworld.Grpc.Actor;
using ActorTypes = Inworld.Grpc.Actor.Types;
using AdditionalPhonemeInfo = Inworld.Grpc.AdditionalPhonemeInfo;
#endif

namespace Inworld.Packets
{
    [DebuggerDisplay("Timestamp={Timestamp}, EventId={PacketId}, Routing={Routing}")]
    public class AudioChunk : InworldPacket
    {
        public DateTime Timestamp { get; set; }
        public PacketId PacketId { get; set; }
        public Routing Routing { get; set; }
        
        public readonly ByteString Chunk;

        public readonly RepeatedField<AdditionalPhonemeInfo> PhonemeInfo;

        public AudioChunk(ByteString chunk, Routing routing)
        {
            Timestamp = DateTime.UtcNow;
            PacketId = new PacketId();
            Routing = routing;
            Chunk = chunk;
        }

        public AudioChunk(GrpcPacket grpcEvent)
        {
            Timestamp = grpcEvent.Timestamp.ToDateTime();
            if (grpcEvent.Routing != null)
                Routing = new Routing(grpcEvent.Routing);
            PacketId = new PacketId(grpcEvent.PacketId);
            Chunk = grpcEvent.DataChunk.Chunk;
            PhonemeInfo = grpcEvent.DataChunk.AdditionalPhonemeInfo;
        }

        public GrpcPacket ToGrpc() => new GrpcPacket
        {
            Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(Timestamp),
            Routing = Routing?.ToGrpc(),
            PacketId = PacketId.ToGrpc(),
            DataChunk = new DataChunk()
            {
                Chunk = Chunk,
                Type = DataChunk.Types.DataType.Audio
            }
        };
    }
}

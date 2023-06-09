using System;
using System.Diagnostics;
using Google.Protobuf;
#if INWORLD_NDK
using Inworld.ProtoBuf;
using GrpcPacket = Inworld.ProtoBuf.InworldPacket;
using GrpcPacketID = Inworld.ProtoBuf.PacketId;
using GrpcRouting = Inworld.ProtoBuf.Routing;
using GrpcActor = Inworld.ProtoBuf.Actor;
using ActorTypes = Inworld.ProtoBuf.Actor.Types;  
#else
using Inworld.Grpc;
using GrpcPacket = Inworld.Grpc.InworldPacket;
using GrpcPacketID = Inworld.Grpc.PacketId;
using GrpcRouting = Inworld.Grpc.Routing;
using GrpcActor = Inworld.Grpc.Actor;
using ActorTypes = Inworld.Grpc.Actor.Types;
#endif

namespace Inworld.Packets
{
    [DebuggerDisplay("Timestamp={Timestamp}, EventId={PacketId}, Routing={Routing}")]
    public class StateChunk : InworldPacket
    {
        public DateTime Timestamp { get; set; }
        public PacketId PacketId { get; set; }
        public Routing Routing { get; set; }
    
        public readonly ByteString Chunk;

        public StateChunk(ByteString chunk, Routing routing)
        {
            Timestamp = DateTime.UtcNow;
            PacketId = new PacketId();
            Routing = routing;
            Chunk = chunk;
        }

        public StateChunk(GrpcPacket grpcEvent)
        {
            Timestamp = grpcEvent.Timestamp.ToDateTime();
            if (grpcEvent.Routing != null)
                Routing = new Routing(grpcEvent.Routing);
            PacketId = new PacketId(grpcEvent.PacketId);
            Chunk = grpcEvent.DataChunk.Chunk;
        }

        public GrpcPacket ToGrpc() => new GrpcPacket
        {
            Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(Timestamp),
            Routing = Routing?.ToGrpc(),
            PacketId = PacketId.ToGrpc(),
            DataChunk = new DataChunk()
            {
                Chunk = Chunk , Type = DataChunk.Types.DataType.State
            }
        };
    }
    
}

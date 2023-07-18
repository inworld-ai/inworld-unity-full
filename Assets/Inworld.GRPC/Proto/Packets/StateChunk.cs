using System;
using System.Diagnostics;
using Google.Protobuf;
using Inworld.Grpc;

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

        public StateChunk(Grpc.InworldPacket grpcEvent)
        {
            Timestamp = grpcEvent.Timestamp.ToDateTime();
            if (grpcEvent.Routing != null)
                Routing = new Routing(grpcEvent.Routing);
            PacketId = new PacketId(grpcEvent.PacketId);
            Chunk = grpcEvent.DataChunk.Chunk;
        }

        public Grpc.InworldPacket ToGrpc() => new Grpc.InworldPacket
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

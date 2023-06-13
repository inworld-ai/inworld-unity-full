using System;
using System.Diagnostics;
using Google.Protobuf;

namespace Inworld.Packets
{
    [DebuggerDisplay("Timestamp={Timestamp}, EventId={PacketId}, Routing={Routing}")]
    public class StateChunk : InworldPacket
    {
        public DateTime Timestamp { get; set; }
        public PacketId PacketId { get; set; }
        public byte[] PacketBytes { get; set; }
        public Routing Routing { get; set; }
    
        public readonly ByteString Chunk;
        byte[] m_PacketBytes;

        public StateChunk(ByteString chunk, Routing routing)
        {
            Timestamp = DateTime.UtcNow;
            PacketId = new PacketId();
            Routing = routing;
            Chunk = chunk;
        }

        public StateChunk(byte[] packetBytes)
        {
            PacketBytes = packetBytes;
            var packet = InworldPacketGenerator.Instance.ToProtobufPacket(this);
            Timestamp = packet.Timestamp.ToDateTime();
            PacketId = InworldPacketGenerator.Instance.FromProtoPacketId(packet.PacketId);
            if(packet.Routing != null)
                Routing = InworldPacketGenerator.Instance.FromProtoRouting(packet.Routing);
            Chunk = packet.DataChunk.Chunk;
        }
    }
    
}

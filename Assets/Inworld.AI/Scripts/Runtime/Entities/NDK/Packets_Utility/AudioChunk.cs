using Google.Protobuf;
using Google.Protobuf.Collections;
using System;
using System.Diagnostics;
#if INWORLD_NDK
using AdditionalPhonemeInfo = Inworld.ProtoBuf.AdditionalPhonemeInfo;
#else
using AdditionalPhonemeInfo = Inworld.Grpc.AdditionalPhonemeInfo;
#endif

namespace Inworld.Packets
{
    [DebuggerDisplay("Timestamp={Timestamp}, EventId={PacketId}, Routing={Routing}")]
    public class AudioChunk : InworldPacket
    {
        public byte[] PacketBytes { get; set; }
        public DateTime Timestamp { get; set; }
        public PacketId PacketId { get; set; }
        public Routing Routing { get; set; }
        
        public ByteString Chunk;

        public RepeatedField<AdditionalPhonemeInfo> PhonemeInfo;

        public readonly long duration;

        public AudioChunk(ByteString chunk, Routing routing)
        {
            Timestamp = DateTime.UtcNow;
            PacketId = new PacketId();
            Routing = routing;
            Chunk = chunk;
        }

        public AudioChunk(byte[] packetBytes)
        {
            PacketBytes = packetBytes;
            var packet = InworldPacketGenerator.Instance.ToProtobufPacket(this);
            Timestamp = packet.Timestamp.ToDateTime();
            PacketId = InworldPacketGenerator.Instance.FromProtoPacketId(packet.PacketId);
            if(packet.Routing != null)
                Routing = InworldPacketGenerator.Instance.FromProtoRouting(packet.Routing);
            Chunk = packet.DataChunk.Chunk;
            PhonemeInfo = packet.DataChunk.AdditionalPhonemeInfo;
        }
        

        // public GrpcPacket PacketBytes() => new GrpcPacket
        // {
        //     Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(Timestamp),
        //     Routing = Routing?.ToGrpc(),
        //     PacketId = PacketId.ToByteArray(),
        //     DataChunk = new DataChunk()
        //     {
        //         Chunk = Chunk,
        //         Type = DataChunk.Types.DataType.Audio
        //     }
        // };
    }
}

﻿using System;
namespace Inworld.Packet
{
    [Serializable]
    public class Source
    {
        public string type;
        public string name;
        public bool isPlayer;
        public bool isCharacter;
    }
    [Serializable]
    public class Routing
    {
        public Source source;
        public Source target;

        public Routing()
        {
            source = new Source();
            target = new Source();
        }
        public Routing(string id = "")
        {
            source = new Source
            {
                name = "player",
                type = "PLAYER",
                isPlayer = true,
                isCharacter = false
            };
            target = new Source
            {
                name = id,
                type = "AGENT",
                isCharacter = true,
                isPlayer = false
            };
        }
    }

    [Serializable]
    public class PacketId
    {
        public string packetId;    // Unique.
        public string utteranceId; // Each sentence is an utterance. But can be interpreted as multiple behavior (Text, EmotionChange, Audio, etc)
        public string interactionId; // Lot of sentences included in one interaction.
        public string correlationId; // Used in future.
        
        public PacketId()
        {
            packetId = Guid.NewGuid().ToString();
            utteranceId = Guid.NewGuid().ToString();
            interactionId = Guid.NewGuid().ToString();
        }
        public override string ToString() => $"{Status} I: {interactionId} U: {utteranceId} P: {packetId}";
        
        public PacketStatus Status { get; set; }
    }

    [Serializable]
    public class InworldPacket
    {
        public string timestamp;
        public string type;
        public PacketId packetId;
        public Routing routing;

        public InworldPacket()
        {
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
            packetId = new PacketId();
            routing = new Routing();
        }
        public InworldPacket(InworldPacket rhs)
        {
            timestamp = rhs.timestamp;
            packetId = rhs.packetId;
            routing = rhs.routing;
            type = rhs.type;
        }
    }
}

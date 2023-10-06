using Inworld.Packet;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Inworld.NDK
{
    public static class InworldNDK
    {
        public static class From
        {
            public static LoadSceneResponse NDKLoadSceneResponse(List<AgentInfo> rhs) => new LoadSceneResponse
            {
                key = "",
                agents = rhs.Select(_GenerateAgent).ToList()
            };

            static InworldCharacterData _GenerateAgent(AgentInfo rhs) => new InworldCharacterData
            {
                agentId = rhs.AgentId,
                brainName = rhs.BrainName,
                givenName = rhs.GivenName,
                characterAssets = new CharacterAssets
                {
                    rpmModelUri = rhs.RpmModelUri,
                    rpmImageUriPortrait = rhs.RpmImageUriPortrait,
                    rpmImageUriPosture = rhs.RpmImageUriPosture,
                    avatarImg = rhs.AvatarImg,
                    avatarImgOriginal = rhs.AvatarImgOriginal
                }
            };
            public static Token NDKToken(SessionInfo rhs) => new Token
            {
                sessionId = rhs.sessionId,
                token = rhs.token,
                type = "Bearer",
                expirationTime = DateTime.UtcNow.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };
            static Inworld.Packet.PacketId NDKPacketId(PacketId rhs) => new Inworld.Packet.PacketId()
            {
                packetId = rhs.uid,
                interactionId = rhs.interactionID,
                utteranceId = rhs.utteranceID,
            };
            static Inworld.Packet.Routing NDKRouting(Routing rhs) => new Inworld.Packet.Routing
            {
                source = new Source
                {
                    name = rhs.source,
                    type = string.IsNullOrEmpty(rhs.source) ? "PLAYER" : "AGENT"
                },
                target = new Source
                {
                    name = rhs.target,
                    type = string.IsNullOrEmpty(rhs.target) ? "PLAYER" : "AGENT"
                }
            };
            public static Inworld.Packet.TextPacket NDKTextPacket(NDKPacket rhs) => new Inworld.Packet.TextPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = rhs.packetType,
                packetId = NDKPacketId(rhs.packetInfo.packetId),
                routing = NDKRouting(rhs.packetInfo.routing),
                text = new TextEvent(rhs.textPacket.text)
            };
            public static Inworld.Packet.AudioPacket NDKAudioChunk(NDKPacket rhs) => new Inworld.Packet.AudioPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "AUDIO",
                packetId = NDKPacketId(rhs.packetInfo.packetId),
                routing = NDKRouting(rhs.packetInfo.routing),
                dataChunk = new DataChunk
                {
                    chunk = rhs.audioPacket.audioChunk,
                    type = "AUDIO",
                    additionalPhonemeInfo = new List<Inworld.Packet.PhonemeInfo>()
                }
            };

            public static Inworld.Packet.ControlPacket NDKControlPacket(NDKPacket rhs) => new Inworld.Packet.ControlPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "CONTROL",
                packetId = NDKPacketId(rhs.packetInfo.packetId),
                routing = NDKRouting(rhs.packetInfo.routing),
                control = new ControlEvent
                {
                    action = InworldNDKEnum.GetAction(rhs.ctrlPacket.action)
                }
            };
            public static Inworld.Packet.EmotionPacket NDKEmotionPacket(NDKPacket rhs) => new Inworld.Packet.EmotionPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "EMOTION",
                packetId = NDKPacketId(rhs.packetInfo.packetId),
                routing = NDKRouting(rhs.packetInfo.routing),
                emotion = new EmotionEvent
                {
                    behavior = InworldNDKEnum.GetEmotion(rhs.emoPacket.behavior),
                    strength = InworldNDKEnum.GetStrength(rhs.emoPacket.strength)
                } 
            };
            public static MutationPacket NDKCancelResponse(NDKPacket rhs) => new MutationPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "CANCEL_RESPONSE",
                packetId = NDKPacketId(rhs.packetInfo.packetId),
                routing = NDKRouting(rhs.packetInfo.routing),
                mutation = new MutationEvent
                {
                    cancelResponses = new CancelResponse
                    {
                        interactionId = rhs.cancelResponsePacket.cancelInteractionID
                    }
                } 
            };
            public static Inworld.Packet.CustomPacket NDKCustomPacket(NDKPacket rhs) => new Inworld.Packet.CustomPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "CUSTOM",
                packetId = NDKPacketId(rhs.packetInfo.packetId),
                routing = NDKRouting(rhs.packetInfo.routing),
                custom = new CustomEvent
                {
                    name = rhs.customPacket.triggerName
                }
            };
            public static Inworld.Packet.RelationPacket NDKRelationPacket(NDKPacket rhs) => new Inworld.Packet.RelationPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "CUSTOM",
                packetId = NDKPacketId(rhs.packetInfo.packetId),
                routing = NDKRouting(rhs.packetInfo.routing),
                debugInfo = new RelationEvent
                {
                    relation = new RelationData
                    {
                        relationState = new RelationState
                        {
                            attraction = rhs.relationPacket.attraction,
                            flirtatious = rhs.relationPacket.flirtatious,
                            familiar = rhs.relationPacket.familiar,
                            respect = rhs.relationPacket.respect,
                            trust = rhs.relationPacket.trust,
                        },
                        relationUpdate = new RelationState
                        {
                            attraction = rhs.relationPacket.attraction,
                            flirtatious = rhs.relationPacket.flirtatious,
                            familiar = rhs.relationPacket.familiar,
                            respect = rhs.relationPacket.respect,
                            trust = rhs.relationPacket.trust,
                        }
                    }
                }
            };
            public static Inworld.Packet.ActionPacket NDKActionPacket(NDKPacket rhs) => new Inworld.Packet.ActionPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "CUSTOM",
                packetId = NDKPacketId(rhs.packetInfo.packetId),
                routing = NDKRouting(rhs.packetInfo.routing),
                action = new ActionEvent
                {
                    narratedAction = new NarrativeAction
                    {
                        content = rhs.actionPacket.content
                    }
                }
            };
            public static InworldPacket NDKUnknownPacket(NDKPacket rhs) => new InworldPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = rhs.packetType,
                packetId = NDKPacketId(rhs.packetInfo.packetId),
                routing = NDKRouting(rhs.packetInfo.routing),
            };
        }
        
        public static class To
        {
            public static Inworld.Packet.TextPacket TextPacket(string characterID, string textToSend) => new Inworld.Packet.TextPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "TEXT",
                packetId = new Inworld.Packet.PacketId(),
                routing = new Inworld.Packet.Routing(characterID),
                text = new TextEvent(textToSend)
            };
        }
    }
}

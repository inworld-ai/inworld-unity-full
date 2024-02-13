/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Packet;
using Inworld.Entities;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Inworld.NDK
{
    /// <summary>
    /// This is the data converter class for converting Unity data to NDK acceptable data, or vise versa.
    /// </summary>
    public static class InworldNDK
    {
        public static class From
        {
            /// <summary>
            /// Transform the load scene response from NDK's format to Unity.
            /// </summary>
            /// <param name="rhs">the list of the agent info (InworldCharacterData in Unity)</param>
            /// <returns></returns>
            public static LoadSceneResponse NDKLoadSceneResponse(List<AgentInfo> rhs) => new LoadSceneResponse
            {
                agents = rhs.Select(_GenerateAgent).ToList()
            };
            /// <summary>
            /// Convert access token from NDK to Unity.
            /// </summary>
            /// <param name="rhs"></param>
            /// <returns></returns>
            public static Token NDKToken(SessionInfo rhs) => new Token
            {
                sessionId = rhs.sessionId,
                token = rhs.token,
                type = "Bearer",
                expirationTime = InworldDateTime.ToString(DateTime.UtcNow.AddHours(1))
            };
            /// <summary>
            /// Convert text packet from NDK to Unity
            /// </summary>
            /// <param name="rhs">the text packet from NDK</param>
            public static Inworld.Packet.TextPacket NDKTextPacket(NDKPacket rhs) => new Inworld.Packet.TextPacket
            {
                timestamp = InworldDateTime.UtcNow,
                type = rhs.packetType,
                packetId = NDKPacketId(rhs.packetInfo.packetId),
                routing = NDKRouting(rhs.packetInfo.routing),
                text = new TextEvent(rhs.textPacket.text)
            };
            /// <summary>
            /// Convert audio chunk from NDK to unity.
            /// Note: We cannot get phoneme data immediately from NDK. phonemes will be gathered afterwards.
            /// </summary>
            /// <param name="rhs">the audio packet from NDK</param>
            public static Inworld.Packet.AudioPacket NDKAudioChunk(NDKPacket rhs) => new Inworld.Packet.AudioPacket
            {
                timestamp = InworldDateTime.UtcNow,
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
            /// <summary>
            /// Convert control packet from NDK to unity.
            /// </summary>
            /// <param name="rhs">the control packets from NDK to convert.</param>
            public static Inworld.Packet.ControlPacket NDKControlPacket(NDKPacket rhs) => new Inworld.Packet.ControlPacket
            {
                timestamp = InworldDateTime.UtcNow,
                type = "CONTROL",
                packetId = NDKPacketId(rhs.packetInfo.packetId),
                routing = NDKRouting(rhs.packetInfo.routing),
                control = new ControlEvent
                {
                    action = InworldNDKEnum.GetAction(rhs.ctrlPacket.action)
                }
            };
            /// <summary>
            /// Convert emotion packet from NDK to Unity
            /// </summary>
            /// <param name="rhs">the emotion packets from NDK to convert.</param>
            public static Inworld.Packet.EmotionPacket NDKEmotionPacket(NDKPacket rhs) => new Inworld.Packet.EmotionPacket
            {
                timestamp = InworldDateTime.UtcNow,
                type = "EMOTION",
                packetId = NDKPacketId(rhs.packetInfo.packetId),
                routing = NDKRouting(rhs.packetInfo.routing),
                emotion = new EmotionEvent
                {
                    behavior = InworldNDKEnum.GetEmotion(rhs.emoPacket.behavior),
                    strength = InworldNDKEnum.GetStrength(rhs.emoPacket.strength)
                } 
            };
            /// <summary>
            /// Convert cancel response packets from NDK to Unity
            /// </summary>
            /// <param name="rhs">the NDK packets to convert.</param>
            public static MutationPacket NDKCancelResponse(NDKPacket rhs) => new MutationPacket
            {
                timestamp = InworldDateTime.UtcNow,
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
            /// <summary>
            /// Convert custom packets (Triggers) from NDK to unity.
            /// </summary>
            /// <param name="rhs">the NDK packet to convert.</param>
            public static Inworld.Packet.CustomPacket NDKCustomPacket(NDKPacket rhs) => new Inworld.Packet.CustomPacket
            {
                timestamp = InworldDateTime.UtcNow,
                type = "CUSTOM",
                packetId = NDKPacketId(rhs.packetInfo.packetId),
                routing = NDKRouting(rhs.packetInfo.routing),
                custom = new CustomEvent
                {
                    name = rhs.customPacket.triggerName
                }
            };
            /// <summary>
            /// Convert the relation packets from NDK to unity.
            /// </summary>
            /// <param name="rhs">the relation packets to convert.</param>
            public static Inworld.Packet.RelationPacket NDKRelationPacket(NDKPacket rhs) => new Inworld.Packet.RelationPacket
            {
                timestamp = InworldDateTime.UtcNow,
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
            /// <summary>
            /// Convert the narrative action packets from NDK to Unity
            /// </summary>
            /// <param name="rhs">the packets from NDK to convert</param>
            public static Inworld.Packet.ActionPacket NDKActionPacket(NDKPacket rhs) => new Inworld.Packet.ActionPacket
            {
                timestamp = InworldDateTime.UtcNow,
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
            /// <summary>
            /// Convert unknown packets from NDK to Unity.
            /// Sometimes the packets may be defined in NDK but not implemented in Unity yet.
            /// </summary>
            /// <param name="rhs">the packet from NDK to convert.</param>
            public static InworldPacket NDKUnknownPacket(NDKPacket rhs) => new InworldPacket
            {
                timestamp = InworldDateTime.UtcNow,
                type = rhs.packetType,
                packetId = NDKPacketId(rhs.packetInfo.packetId),
                routing = NDKRouting(rhs.packetInfo.routing),
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
        }
        
        public static class To
        {
            /// <summary>
            /// Convert the text packet from Unity to NDK.
            /// </summary>
            /// <param name="characterID">the live session ID of the character</param>
            /// <param name="textToSend">the message to send.</param>
            public static Inworld.Packet.TextPacket TextPacket(string characterID, string textToSend) => new Inworld.Packet.TextPacket
            {
                timestamp = InworldDateTime.UtcNow,
                type = "TEXT",
                packetId = new Inworld.Packet.PacketId(),
                routing = new Inworld.Packet.Routing(characterID),
                text = new TextEvent(textToSend)
            };
        }
    }
}

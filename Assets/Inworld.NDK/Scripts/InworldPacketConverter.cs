using Google.Protobuf;
using Inworld.Packet;
using System.Linq;
using System;
using System.Collections.Generic;


namespace Inworld.NDK
{
    public static class InworldPacketConverter
    {
        public static class To
        {
            public static CapabilitiesRequest Capabilities => new CapabilitiesRequest
            {
                Audio = InworldAI.Capabilities.audio,
                Emotions = InworldAI.Capabilities.emotions,
                Interruptions = InworldAI.Capabilities.interruptions,
                NarratedActions = InworldAI.Capabilities.narratedActions,
                SilenceEvents = InworldAI.Capabilities.silence,
                Text = InworldAI.Capabilities.text,
                Triggers = InworldAI.Capabilities.triggers,
                Continuation = InworldAI.Capabilities.continuation,
                TurnBasedSTT = InworldAI.Capabilities.turnBasedStt,
                PhonemeInfo = InworldAI.Capabilities.phonemeInfo
            };

            public static UserRequest User => new UserRequest
            {
                Name = InworldAI.User.Name
            };
            public static ClientRequest Client => new ClientRequest
            {
                Id = "unity"
            };
            public static UserSettings UserSetting => new UserSettings
            {
                ViewTranscriptConsent = true,
                PlayerProfile = new PlayerProfile()
                {
                    Fields =
                    {
                        InworldAI.User.Setting?.playerProfile.fields.Select
                        (
                            profile => new Fields()
                            {
                                FieldId = profile.fieldId,
                                FieldValue = profile.fieldValue
                            }
                        )
                    }
                }
            };
            public static InworldPacket NDKPacket(string charID) => new InworldPacket
            {
                Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                Routing = new Routing
                {
                    Source = new Actor
                    {
                        Name = InworldAI.User.Name,
                        Type = Actor.Types.Type.Player
                    },
                    Target = new Actor
                    {
                        Name = charID,
                        Type = Actor.Types.Type.Agent
                    }
                },
                PacketId = new PacketId
                {
                    PacketId_ = Guid.NewGuid().ToString(),
                    InteractionId = Guid.NewGuid().ToString(),
                    UtteranceId = Guid.NewGuid().ToString(),
                }
            };
            public static InworldPacket NDKPacket(Packet.InworldPacket rhs) => new InworldPacket
            {
                Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                Routing = new Routing
                {
                    Source = new Actor
                    {
                        Name = rhs.routing.source.name,
                        Type = Actor.Types.Type.Player
                    },
                    Target = new Actor
                    {
                        Name = rhs.routing.target.name,
                        Type = Actor.Types.Type.Agent
                    }
                },
                PacketId = new PacketId
                {
                    PacketId_ = rhs.packetId.packetId,
                    InteractionId = rhs.packetId.interactionId,
                    UtteranceId = rhs.packetId.utteranceId,
                    CorrelationId = rhs.packetId.correlationId
                }
            };
            
            public static InworldPacket TextEvent(string charID, string txtToSend)
            {
                InworldPacket toSend = NDKPacket(charID);
                toSend.Text = new TextEvent
                {
                    Text = txtToSend,
                    SourceType = NDK.TextEvent.Types.SourceType.TypedIn,
                    Final = true
                };
                return toSend;
            }
            public static InworldPacket CancelResponseEvent(string charID, string interactionID)
            {
                InworldPacket toSend = NDKPacket(charID);
                toSend.Mutation = new MutationEvent
                {
                    CancelResponses = new CancelResponses
                    {
                        InteractionId = interactionID
                    }
                };
                return toSend;
            }
            public static InworldPacket CustomEvent(string charID, string triggerName, Dictionary<string, string> parameters)
            {
                InworldPacket toSend = NDKPacket(charID);
                toSend.Custom = new CustomEvent
                {
                    Name = triggerName,
                };
                foreach (KeyValuePair<string, string> kvp in parameters)
                {
                    toSend.Custom.Parameters.Add(new CustomEvent.Types.Parameter
                    {
                        Name = kvp.Key,
                        Value = kvp.Value
                    });
                }
                return toSend;
            }
            public static InworldPacket AudioSessionStart(string charID)
            {
                InworldPacket toSend = NDKPacket(charID);
                toSend.Control = new ControlEvent
                {
                    Action = ControlEvent.Types.Action.AudioSessionStart
                };
                return toSend;
            }
            public static InworldPacket AudioSessionEnd(string charID)
            {
                InworldPacket toSend = NDKPacket(charID);
                toSend.Control = new ControlEvent
                {
                    Action = ControlEvent.Types.Action.AudioSessionEnd
                };
                return toSend;
            }
            public static InworldPacket AudioChunk(string charID, string base64)
            {
                InworldPacket toSend = NDKPacket(charID);
                toSend.DataChunk = new DataChunk
                {
                    Type = DataChunk.Types.DataType.Audio,
                    Chunk = ByteString.FromBase64(base64)
                };
                return toSend;
            }
        }
        public static class From
        {
            public static Inworld.LoadSceneResponse NDKLoadSceneResponse(LoadSceneResponse rhs) => new Inworld.LoadSceneResponse
            {
                key = rhs.Key,
                agents = rhs.Agents.Select(_GenerateAgent).ToList()
            };
            static Inworld.InworldCharacterData _GenerateAgent(Inworld.NDK.InworldCharacterData rhs) => new Inworld.InworldCharacterData
            {
                agentId = rhs.AgentId,
                brainName = rhs.BrainName,
                givenName = rhs.GivenName,
                characterAssets = new Inworld.CharacterAssets()
                {
                    rpmModelUri = rhs.CharacterAssets.RpmModelUri,
                    rpmImageUriPortrait = rhs.CharacterAssets.RpmImageUriPortrait,
                    rpmImageUriPosture = rhs.CharacterAssets.RpmImageUriPosture,
                    avatarImg = rhs.CharacterAssets.AvatarImg,
                    avatarImgOriginal = rhs.CharacterAssets.AvatarImgOriginal
                }
            };
            public static Inworld.Token NDKToken(Inworld.NDK.Token authToken) => new Inworld.Token
            {
                token = authToken.Token_,
                type = authToken.Type,
                expirationTime = authToken.ExpirationTime.ToDateTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", System.Globalization.CultureInfo.InvariantCulture),
                sessionId = authToken.SessionId
            };
            public static Packet.InworldPacket NDKPacket(InworldPacket grpcPacket) => new Packet.InworldPacket
            {
                timestamp = grpcPacket.Timestamp.ToDateTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = grpcPacket.GetType().ToString(),
                packetId = new Packet.PacketId
                {
                    interactionId = grpcPacket.PacketId.InteractionId,
                    packetId = grpcPacket.PacketId.PacketId_,
                    utteranceId = grpcPacket.PacketId.UtteranceId,
                    correlationId = grpcPacket.PacketId.CorrelationId,
                    Status = PacketStatus.RECEIVED
                },
                routing = new Packet.Routing
                {
                    source = new Source
                    {
                        name = grpcPacket.Routing.Source?.Name,
                        type = grpcPacket.Routing.Source?.Type.ToString()
                    },
                    target = new Source
                    {
                        name = grpcPacket.Routing.Target?.Name,
                        type = grpcPacket.Routing.Target?.Type.ToString()
                    }
                }
            };
            public static Packet.InworldPacket NDKAudioChunk(InworldPacket response) => new AudioPacket
            (
                NDKPacket(response), new Packet.DataChunk
                {
                    type = "AUDIO",
                    chunk = response.DataChunk.Chunk.ToBase64(),
                    additionalPhonemeInfo = response.DataChunk.AdditionalPhonemeInfo.Select
                    (
                        p => new PhonemeInfo
                        {
                            phoneme = p.Phoneme,
                            startOffset = (float)p.StartOffset.ToTimeSpan().TotalSeconds
                        }
                    ).ToList()
                }
            );
            public static Packet.InworldPacket NDKTextPacket(InworldPacket response) => new TextPacket
            (
                NDKPacket(response), new Packet.TextEvent
                {
                    text = response.Text.Text,
                    sourceType = response.Text.SourceType.ToString(),
                    final = response.Text.Final
                }
            );

            public static Packet.InworldPacket NDKControlPacket(InworldPacket response) => new ControlPacket
            (
                NDKPacket(response), new Packet.ControlEvent
                {
                    action = response.Control.Action.ToString(),
                    description = response.Control.Description
                }
            );

            public static Packet.InworldPacket NDKEmotionPacket(InworldPacket response) => new EmotionPacket
            (
                NDKPacket(response), new Packet.EmotionEvent
                {
                    behavior = response.Emotion.Behavior.ToString(),
                    strength = response.Emotion.Strength.ToString()
                }
            );

            public static Packet.InworldPacket NDKActionPacket(InworldPacket response) => new ActionPacket
            (
                NDKPacket(response), new Packet.ActionEvent
                {
                    content = response.Action.NarratedAction.Content
                }
            );

            public static Packet.InworldPacket NDKCustomPacket(InworldPacket response) => new CustomPacket
            (
                NDKPacket(response), new Packet.CustomEvent
                {
                    name = response.Custom.Name,
                    parameters = response.Custom.Parameters.Select
                    (
                        p => new TriggerParamer
                        {
                            name = p.Name,
                            value = p.Value
                        }
                    ).ToList()
                }
            );
        }
        
    }
}

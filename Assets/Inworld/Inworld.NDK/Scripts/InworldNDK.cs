using Inworld.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Inworld.NDK
{
    public static class InworldNDK
    {
        static AudioPacket s_CurrentAudio;
        static List<Inworld.Packet.PhonemeInfo> s_CurrentPhoneme = new List<Inworld.Packet.PhonemeInfo>();
        public static class API
        {
            public static void GetAccessToken(string serverURL, string apiKey, string apiSecret)
            {
                NDKInterop.Unity_GetAccessToken(serverURL, apiKey, apiSecret, OnTokenGenerated);
            }
            public static void LoadScene(string sceneFullName)
            {
                Inworld.Capabilities capabilities = InworldAI.Capabilities;
                Capabilities cap = new Capabilities
                {
                    Text  = capabilities.text,
                    Audio = capabilities.audio,
                    Emotions = capabilities.emotions,
                    Interruptions  = capabilities.interruptions,
                    Triggers  = capabilities.triggers,
                    PhonemeInfo = capabilities.text,
                    TurnBasedSTT  = capabilities.turnBasedStt,
                    NarratedActions = capabilities.narratedActions,
                };
                NDKInterop.Unity_SetCapabilities(ref cap);
                if (string.IsNullOrEmpty(InworldAI.User.ID))
                    InworldAI.User.ID = SystemInfo.deviceUniqueIdentifier;
                NDKInterop.Unity_SetUserRequest(InworldAI.User.Name, InworldAI.User.ID);
                foreach (InworldPlayerProfile profile in InworldAI.User.PlayerProfiles)
                {
                    NDKInterop.Unity_AddUserProfile(profile.property, profile.value);
                }
                NDKInterop.Unity_SetClientRequest("Unity NDK", InworldAI.Version);
                NDKInterop.Unity_LoadScene(sceneFullName, OnSceneLoaded);
            }
            public static void Init()
            {
                InworldAI.Log("[NDK] Start Init");
                NDKInterop.Unity_InitWrapper();
                InworldAI.Log("[NDK] Start Set Logger");
                NDKInterop.Unity_SetLogger(OnLogReceived);
                InworldAI.Log("[NDK] Start Set Back");
                NDKInterop.Unity_SetPacketCallback(OnTextReceived, 
                                                  OnAudioReceived, 
                                                  OnControlReceived, 
                                                  OnEmotionReceived, 
                                                  OnCancelReceived, 
                                                  OnTriggerReceived, 
                                                  OnPhomeneReceived, 
                                                  OnTriggerParamReceived);
            }
#region Callback
            static void OnTokenGenerated()
            {
                SessionInfo sessionInfo = NDKInterop.Unity_GetSessionInfo();
                InworldController.Client.Token = From.NDKToken(sessionInfo);
                InworldAI.Log("Get Session ID: " + sessionInfo.sessionId);
                InworldController.Client.Status = InworldConnectionStatus.Initialized;
            }
            public static void OnSceneLoaded()
            {
                InworldAI.Log("[NDK] Scene loaded");
                if (InworldController.Client is not InworldNDKClient ndkClient)
                    return;
                int nAgentSize = NDKInterop.Unity_GetAgentCount();
                ndkClient.AgentList.Clear();
                for (int i = 0; i < nAgentSize; i++)
                {
                    ndkClient.AgentList.Add(NDKInterop.Unity_GetAgentInfo(i));
                }
                InworldController.Client.Status = InworldConnectionStatus.LoadingSceneCompleted;
            }
            // YAN: To use callback it has to be static.
            static void OnLogReceived(string log, int severity)
            {
                switch (severity)
                {
                    case 1:
                        InworldAI.LogWarning($"[NDK]: {log}");
                        break;
                    case 2:
                        InworldAI.LogError($"[NDK]: {log}");
                        break;
                    case 3:
                        InworldAI.LogException($"[NDK]: {log}");
                        break;
                    default:
                        InworldAI.Log($"[NDK]: {log}");
                        break;
                }
            }
            static void OnTextReceived(TextPacket packet)
            {
                InworldController.Client.Dispatch(From.NDKTextPacket(packet));
            }
            static void OnAudioReceived(AudioPacket packet)
            {
                s_CurrentAudio = packet;
                s_CurrentPhoneme.Clear();
            }
            static void OnControlReceived(ControlPacket packet)
            {
                InworldController.Client.Dispatch(From.NDKControlPacket(packet));
            }
            static void OnEmotionReceived(EmotionPacket packet)
            {
                InworldController.Client.Dispatch(From.NDKEmotionPacket(packet));
            }
            static void OnCancelReceived(CancelResponsePacket packet)
            {
                InworldController.Client.Dispatch(From.NDKCancelResponse(packet));
            }
            static void OnTriggerReceived(CustomPacket packet)
            {
                InworldController.Client.Dispatch(From.NDKCustomPacket(packet));
            }
            static void OnTriggerParamReceived(TriggerParam packet)
            {
                // Currently server doesn't support send trigger callback with param. 
            }
            static void OnPhomeneReceived(PhonemeInfo packet)
            {
                s_CurrentPhoneme.Add(new Inworld.Packet.PhonemeInfo
                {
                    phoneme = packet.code,
                    startOffset = packet.timeStamp
                });
                if (s_CurrentPhoneme.Count == s_CurrentAudio.phonemeCount)
                    InworldController.Client.Dispatch(From.NDKAudioChunk());
            }   
#endregion
        }

        public static class From
        {
            static string _GetEmotion(int nEmotionCode)
            {
                switch (nEmotionCode) 
                {
                    case 0: return "NEUTRAL";
                    case 1: return "DISGUST";
                    case 2: return "CONTEMPT";
                    case 3: return "BELLIGERENCE";
                    case 4: return "DOMINEERING";
                    case 5: return "CRITICISM";
                    case 6: return "ANGER";
                    case 7: return "TENSION";
                    case 8: return "TENSE_HUMOR";
                    case 9: return "DEFENSIVENESS";
                    case 10: return "WHINING";
                    case 11: return "SADNESS";
                    case 12: return "STONEWALLING";
                    case 13: return "INTEREST";
                    case 14: return "VALIDATION";
                    case 15: return "AFFECTION";
                    case 16: return "HUMOR";
                    case 17: return "SURPRISE";
                    case 18: return "JOY";
                    default: return "UNKNOWN"; 
                }
            }
            static string _GetStrength(int nStrength)
            {
                switch (nStrength) 
                {
                    case 0: return "UNSPECIFIED";
                    case 1: return "WEAK";
                    case 2: return "STRONG";
                    case 3: return "NORMAL";
                    default: return "UNKNOWN"; 
                }
            }
            static string _GetAction(int code) 
            {
                switch (code) 
                {
                    case 0: return "UNKNOWN";
                    case 1: return "AUDIO_SESSION_START";
                    case 2: return "AUDIO_SESSION_END";
                    case 3: return "INTERACTION_END";
                    case 4: return "TTS_PLAYBACK_START";
                    case 5: return "TTS_PLAYBACK_END";
                    case 6: return "TTS_PLAYBACK_MUTE";
                    case 7: return "TTS_PLAYBACK_UNMUTE";
                    case 8: return "WARNING";
                    case 9: return "SESSION_END";
                    default: return "INVALID";
                }
            }
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
                    type = "AGENT"
                },
                target = new Source
                {
                    name = rhs.target,
                    type = "PLAYER"
                }
            };
            public static Inworld.Packet.TextPacket NDKTextPacket(TextPacket rhs) => new Inworld.Packet.TextPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "TEXT",
                packetId = NDKPacketId(rhs.packet.packetId),
                routing = NDKRouting(rhs.packet.routing),
                text = new TextEvent(rhs.text)
            };
            public static Inworld.Packet.AudioPacket NDKAudioChunk() => new Inworld.Packet.AudioPacket()
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "AUDIO",
                packetId = NDKPacketId(s_CurrentAudio.packet.packetId),
                routing = NDKRouting(s_CurrentAudio.packet.routing),
                dataChunk = new DataChunk
                {
                    chunk = s_CurrentAudio.audioChunk,
                    type = "AUDIO",
                    additionalPhonemeInfo = s_CurrentPhoneme
                }
            };
            public static Inworld.Packet.ControlPacket NDKControlPacket(ControlPacket rhs) => new Inworld.Packet.ControlPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "CONTROL",
                packetId = NDKPacketId(rhs.packet.packetId),
                routing = NDKRouting(rhs.packet.routing),
                control = new ControlEvent
                {
                    action = _GetAction(rhs.action)
                }
            };
            public static Inworld.Packet.EmotionPacket NDKEmotionPacket(EmotionPacket rhs) => new Inworld.Packet.EmotionPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "EMOTION",
                packetId = NDKPacketId(rhs.packet.packetId),
                routing = NDKRouting(rhs.packet.routing),
                emotion = new EmotionEvent
                {
                    behavior = _GetEmotion(rhs.behavior),
                    strength = _GetStrength(rhs.strength)
                } 
            };
            public static MutationPacket NDKCancelResponse(CancelResponsePacket rhs) => new MutationPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "CANCEL_RESPONSE",
                packetId = NDKPacketId(rhs.packet.packetId),
                routing = NDKRouting(rhs.packet.routing),
                mutation = new MutationEvent
                {
                    cancelResponses = new CancelResponse
                    {
                        interactionId = rhs.cancelInteractionID
                    }
                } 
            };
            public static Inworld.Packet.CustomPacket NDKCustomPacket(CustomPacket rhs) => new Inworld.Packet.CustomPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "CUSTOM",
                packetId = NDKPacketId(rhs.packet.packetId),
                routing = NDKRouting(rhs.packet.routing),
                custom = new CustomEvent
                {
                    name = rhs.triggerName
                }
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

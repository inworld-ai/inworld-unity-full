using Google.Protobuf;
using Grpc.Core;
using Inworld.Packet;
using Inworld.Runtime;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace Inworld.Grpc
{
    public class InworldGRPCClient : InworldClient
    {
        InworldAuthGRPC m_AuthGRPC;
        WorldEngine.WorldEngineClient m_WorldEngineClient;
        AsyncDuplexStreamingCall<InworldPacket, InworldPacket> m_StreamingCall;
        ConcurrentQueue<InworldPacket> m_IncomingAudioQueue = new ConcurrentQueue<InworldPacket>();
        ConcurrentQueue<InworldPacket> m_IncomingEventsQueue = new ConcurrentQueue<InworldPacket>();
        ConcurrentQueue<InworldPacket> m_OutgoingEventsQueue = new ConcurrentQueue<InworldPacket>();
        LoadSceneResponse m_LoadSceneResponse; // Yan: Grpc.LoadSceneResponse
        Channel m_Channel;
        Metadata m_Header;
        float m_CoolDown;

        string LastState { get; set; }
        
        protected override void Init()
        {
            base.Init();
            m_AuthGRPC = new InworldAuthGRPC();
            m_Channel = new Channel(m_ServerConfig.RuntimeServer, new SslCredentials());
            m_WorldEngineClient = new WorldEngine.WorldEngineClient(m_Channel);
        }
        void Update()
        {
            if (Status == InworldConnectionStatus.Connected)
            {
                m_CoolDown += Time.deltaTime;
                if (m_CoolDown > 0.1f)
                {
                    m_CoolDown = 0;
                    m_IncomingEventsQueue.TryDequeue(out InworldPacket incomingPacket);
                    if (incomingPacket != null)
                        _ResolveGRPCPackets(incomingPacket);
                }
            }
                
        }
        void OnDisable()
        {
            _EndSession();
        }
        void OnDestroy()
        {
            _EndSession();
        }
        async Task _EndSession()
        {
            _ResetCommunicationData();
            if (Status == InworldConnectionStatus.Connected)
            {
                Status = InworldConnectionStatus.Idle;
                await m_StreamingCall.RequestStream.CompleteAsync();
                m_StreamingCall.Dispose();
            }
        }

        public override void GetAccessToken() => _GenerateAccessTokenAsync();
        // ReSharper disable Unity.PerformanceAnalysis
        public override void LoadScene(string sceneFullName) => _LoadSceneAsync(sceneFullName);
        public override Inworld.LoadSceneResponse GetLiveSessionInfo() => InworldGRPC.From.GRPCLoadSceneResponse(m_LoadSceneResponse);
#pragma warning disable CS4014
        // ReSharper disable Unity.PerformanceAnalysis
        public override void StartSession() => _StartSession();
#pragma warning restore CS4014
        public override void SendText(string characterID, string textToSend)
        {
            if (string.IsNullOrEmpty(characterID) || string.IsNullOrEmpty(textToSend))
                return;
            TextPacket packet = new TextPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "TEXT",
                packetId = new Packet.PacketId(),
                routing = new Packet.Routing(characterID),
                text = new Packet.TextEvent(textToSend)
            };
            Dispatch(packet);
            _SendPacket(InworldGRPC.To.TextEvent(characterID, textToSend));
        }
        
        public override void SendCancelEvent(string characterID, string interactionID)
        {
            if (string.IsNullOrEmpty(characterID))
                return;
            _SendPacket(InworldGRPC.To.CancelResponseEvent(characterID, interactionID));
        }
        
        public override void SendTrigger(string charID, string triggerName, Dictionary<string, string> parameters)
        {
            if (string.IsNullOrEmpty(charID))
                return;
            InworldAI.Log($"Send Trigger {triggerName}");
            _SendPacket(InworldGRPC.To.CustomEvent(charID, triggerName, parameters));
        }
        public override void StartAudio(string charID)
        {
            if (string.IsNullOrEmpty(charID))
                return;

            _SendPacket(InworldGRPC.To.AudioSessionStart(charID));
            if (!m_AudioCapture.IsCapturing)
                m_AudioCapture.StartRecording();
        }
        public override void StopAudio(string charID)
        {
            if (string.IsNullOrEmpty(charID))
                return;
            _SendPacket(InworldGRPC.To.AudioSessionEnd(charID));
            if (m_AudioCapture.IsCapturing)
                m_AudioCapture.StopRecording();
        }
        public override void SendAudio(string charID, string base64)
        {
            if (string.IsNullOrEmpty(charID) || string.IsNullOrEmpty(base64))
                return;
            _SendPacket(InworldGRPC.To.AudioChunk(charID, base64));
        }
        
        internal async Task _StartSession()
        {
            if (!IsTokenValid)
                throw new ArgumentException("No sessionKey to start Inworld session, use CreateWorld first.");
            _ResetCommunicationData();
            Status = InworldConnectionStatus.Connected;
            try
            {
                using (m_StreamingCall = m_WorldEngineClient.Session(m_Header))
                {
                    // https://grpc.github.io/grpc/csharp/api/Grpc.Core.IAsyncStreamReader-1.html
                    Task inputTask = Task.Run
                    (
                        async () =>
                        {
                            while (Status == InworldConnectionStatus.Connected)
                            {
                                bool next;
                                try
                                {
                                    // Waiting response for some time before checking if done.
                                    next = await m_StreamingCall.ResponseStream.MoveNext();
                                }
                                catch (RpcException rpcException)
                                {
                                    if (rpcException.StatusCode == StatusCode.Cancelled)
                                    {
                                        next = false;
                                    }
                                    else
                                    {
                                        // rethrowing other errors.
                                        throw;
                                    }
                                }
                                if (next)
                                {
                                    m_IncomingEventsQueue.Enqueue(m_StreamingCall.ResponseStream.Current);
                                }
                                else
                                {
                                    InworldAI.Log("Session is closed.");
                                    break;
                                }
                            }
                        }
                    );
                    Task outputTask = Task.Run
                    (
                        async () =>
                        {
                            while (Status == InworldConnectionStatus.Connected)
                            {
                                Task.Delay(100).Wait();
                                // Sending all outgoing events.
                                while (m_OutgoingEventsQueue.TryDequeue(out InworldPacket e))
                                {
                                    if (Status == InworldConnectionStatus.Connected)
                                    {
                                        await m_StreamingCall.RequestStream.WriteAsync(e);
                                    }
                                }
                            }
                        }
                    );
                    await Task.WhenAll(inputTask, outputTask);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("inactivity"))
                    Status = InworldConnectionStatus.LostConnect;
                else
                    Error = e.ToString();
            }
            finally
            {
                Status = InworldConnectionStatus.Idle;
            }
        }
        async void _GenerateAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(m_CustomToken))
            {
                _ReceiveCustomToken();
                return;
            }
            GenerateTokenRequest gtRequest = new GenerateTokenRequest
            {
                Key = m_APIKey,
                Resources =
                {
                    InworldController.Instance.CurrentWorkspace
                }
            };
            Metadata metadata = new Metadata
            {
                {
                    "authorization", m_AuthGRPC.GetHeader(m_ServerConfig.RuntimeServer, m_APIKey, m_APISecret)
                }
            };
            
            try
            {
                m_AuthGRPC.Token = await m_WorldEngineClient.GenerateTokenAsync(gtRequest, metadata, DateTime.UtcNow.AddHours(1));
                InworldAI.Log("Init Success!");
                m_Header = new Metadata
                {
                    {"authorization", $"Bearer {m_AuthGRPC.Token.Token}"},
                    {"session-id", m_AuthGRPC.Token.SessionId}
                };
                m_Token = InworldGRPC.From.GRPCToken(m_AuthGRPC.Token);
                Status = InworldConnectionStatus.Initialized;
            }
            catch (RpcException e)
            {
                Error = e.ToString();
            }
        }
        async void _LoadSceneAsync(string sceneName)
        {
            LoadSceneRequest lsRequest = new LoadSceneRequest
            {
                Name = sceneName,
                Capabilities = InworldGRPC.To.Capabilities,
                User = InworldGRPC.To.User,
                Client = InworldGRPC.To.Client,
                UserSettings = InworldGRPC.To.UserSetting
            };
            if (!string.IsNullOrEmpty(LastState))
            {
                lsRequest.SessionContinuation = new SessionContinuation
                {
                    PreviousState = ByteString.FromBase64(LastState)
                };
            }
            try
            {
                m_LoadSceneResponse = await m_WorldEngineClient.LoadSceneAsync(lsRequest, m_Header);
                // Yan: They somehow use {WorkSpace}:{sessionKey} as "sessionKey" now. Need to remove the first part.
                m_SessionKey = m_LoadSceneResponse.Key.Split(':')[1];
                Debug.Log($"Session ID: {m_Token.sessionId}");
                if (m_LoadSceneResponse.PreviousState != null)
                {
                    foreach (PreviousState.Types.StateHolder stateHolder in m_LoadSceneResponse.PreviousState.StateHolders)
                    {
                        InworldAI.Log($"Received Previous Packets: {stateHolder.Packets.Count}");
                    }
                }
                m_Header.Add("Authorization", $"Bearer {m_SessionKey}");
                Status = InworldConnectionStatus.LoadingSceneCompleted;
            }
            catch (RpcException e)
            {
                Error = e.ToString();
            }
        }
        
        void _SendPacket(InworldPacket packet)
        {
            if (Status == InworldConnectionStatus.Connected)
                m_OutgoingEventsQueue.Enqueue(packet);
        }
        void _ResolveGRPCPackets(InworldPacket response)
        {
            if (response.DataChunk != null)
            {
                switch (response.DataChunk.Type)
                {
                    case DataChunk.Types.DataType.Audio:
                        Dispatch(InworldGRPC.From.GRPCAudioChunk(response));
                        break;
                    case DataChunk.Types.DataType.State:
                    case DataChunk.Types.DataType.Silence:
                        // TODO(YAN): Support State and Silence.
                        break;
                    default:
                        InworldAI.LogError($"Unsupported incoming event: {response}");
                        Dispatch(InworldGRPC.From.GRPCPacket(response));
                        break;
                }
            }
            else if (response.Text != null)
            {
                Dispatch(InworldGRPC.From.GRPCTextPacket(response));
            }
            else if (response.Control != null)
            {
                Dispatch(InworldGRPC.From.GRPCControlPacket(response));
            }
            else if (response.Emotion != null)
            {
                Dispatch(InworldGRPC.From.GRPCEmotionPacket(response));
            }
            else if (response.Action != null)
            {
                Dispatch(InworldGRPC.From.GRPCActionPacket(response));
            }
            else if (response.Custom != null)
            {
                Dispatch(InworldGRPC.From.GRPCCustomPacket(response));
            }
            else
            {
                Debug.LogError($"YAN UnSupported {response}");
                Dispatch(InworldGRPC.From.GRPCPacket(response));
            }
        }
        void _ReceiveCustomToken()
        {
            JObject data = JObject.Parse(m_CustomToken);
            if (data.ContainsKey("sessionId") && data.ContainsKey("token"))
            {
                InworldAI.Log("Init Success with Custom Token!");
                m_Header = new Metadata
                {
                    {"authorization", $"Bearer {data["token"]}"},
                    {"session-id", data["sessionId"]?.ToString()}
                };
                Status = InworldConnectionStatus.Initialized;
            }
            else
                Error = "Token Invalid";
        }
        void _ResetCommunicationData()
        {
            m_IncomingAudioQueue.Clear();
            m_IncomingEventsQueue.Clear();
            m_OutgoingEventsQueue.Clear();
        }
    }
}


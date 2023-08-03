using Google.Protobuf;
using Grpc.Core;
using Inworld.Packet;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;


namespace Inworld.NDK
{
    public class InworldNDKClient : InworldClient
    {
        [FormerlySerializedAs("useAEC")] public bool useAec = false;
        InworldNDKBridge m_Wrapper;
        ConnectionStateCallbackType m_ConnectionCallback;
        PacketCallbackType m_PacketCallback;
        LoadSceneCallbackType m_Callback;
        float m_LastPacketSendTime = 0f;
        TaskCompletionSource<bool> m_AgentInfosFilled;

    #region Wrapper variables
        readonly SessionInfo m_SessionInfo = new SessionInfo();
        readonly ClientOptions m_Options = new ClientOptions();
        AgentInfoArray m_AgentInfoArray = new AgentInfoArray();
    #endregion

        readonly ConcurrentQueue<InworldPacket> m_IncomingEventsQueue = new ConcurrentQueue<InworldPacket>();
        readonly ConcurrentQueue<InworldPacket> m_OutgoingEventsQueue = new ConcurrentQueue<InworldPacket>();
        Inworld.LoadSceneResponse m_LoadSceneResponse; // Yan: Grpc.LoadSceneResponse
        float m_CoolDown;

        protected override void Init()
        {
            base.Init();
            m_ConnectionCallback = ConnectionStateCallback;
            m_PacketCallback = PacketCallback;
            m_Wrapper = new InworldNDKBridge(m_ConnectionCallback, m_PacketCallback);
        }
        
        void ConnectionStateCallback(ConnectionState state)
        {
            InworldAI.Log("CONNECTION STATE IS " + state);
            switch (state)
            {
                case ConnectionState.Connected:
                    Status = InworldConnectionStatus.Connected;
                    InworldAI.Log("Init Success!");
                    break;
                case ConnectionState.Disconnected:
                    Status = InworldConnectionStatus.LostConnect;
                    break;
                case ConnectionState.Failed:
                    Status = InworldConnectionStatus.InitFailed;
                    InworldAI.Log("Connection state returned failed from NDK");
                    break;
                case ConnectionState.Idle:
                    Status = InworldConnectionStatus.Idle;
                    break;
                case ConnectionState.Reconnecting:
                    case ConnectionState.Connecting:
                    Status = InworldConnectionStatus.Connecting;
                    break;
            }
        }

        public void PacketCallback(IntPtr packetwrapper, int packetSize)
        {
            // Create a byte array to store the data
            byte[] data = new byte[packetSize];
            // Copy the data from the IntPtr to the byte array
            Marshal.Copy(packetwrapper, data, 0, packetSize);
            //Marshal.PtrToStructure<GrpcPacket>(packetwrapper);

            // Deserialize the byte array to an InworldPacket instance using protobuf
            InworldPacket response = InworldPacket.Parser.ParseFrom(data);
            m_IncomingEventsQueue.Enqueue(response);
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
                        ResolvePackets(incomingPacket);
                }
                if (Time.time - m_LastPacketSendTime > 0.1f)
                {
                    m_LastPacketSendTime = Time.time;

                    InworldPacket e;
                    while (m_OutgoingEventsQueue.TryDequeue(out e))
                    {
                        SendRawEvent(e);
                    }
                }
            }
            InworldNDKBridge.ClientWrapper_Update(m_Wrapper.instance);

        }
        void OnDisable()
        {
            _EndSession();
        }
        void OnDestroy()
        {
            _EndSession();
        }
        Task _EndSession()
        {
            _ResetCommunicationData();
            m_Token = null;
            InworldNDKBridge.ClientWrapper_StopClient(m_Wrapper.instance);
            return Task.CompletedTask;
        }

        public override void Disconnect()
        {
            _EndSession();
        }
        public override void GetAccessToken() => Authenticate();//_GenerateAccessTokenAsync();
        // ReSharper disable Unity.PerformanceAnalysis
        public override void LoadScene(string sceneFullName) => _LoadSceneAsync(sceneFullName);
        public override Inworld.LoadSceneResponse GetLiveSessionInfo() => m_LoadSceneResponse;
#pragma warning disable CS4014
        // ReSharper disable Unity.PerformanceAnalysis
        public override void StartSession() => _StartSession();
#pragma warning restore CS4014
        void SendRawEvent(InworldPacket packet)
        {
            byte[] packetBytes = packet.ToByteArray();
            IntPtr packetPtr = Marshal.AllocHGlobal(packetBytes.Length);
            try
            {
                Marshal.Copy(packetBytes, 0, packetPtr, packetBytes.Length);
                InworldNDKBridge.ClientWrapper_SendPacket(m_Wrapper.instance, packetPtr);
            }
            finally
            {
                Marshal.FreeHGlobal(packetPtr);  // Make sure to free the allocated memory
            }
        }
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
            _SendPacket(InworldPacketConverter.To.TextEvent(characterID, textToSend));
        }
        
        public override void SendCancelEvent(string characterID, string interactionID)
        {
            if (string.IsNullOrEmpty(characterID))
                return;
            _SendPacket(InworldPacketConverter.To.CancelResponseEvent(characterID, interactionID));
        }
        
        public override void SendTrigger(string charID, string triggerName, Dictionary<string, string> parameters)
        {
            if (string.IsNullOrEmpty(charID))
                return;
            InworldAI.Log($"Send Trigger {triggerName}");
            _SendPacket(InworldPacketConverter.To.CustomEvent(charID, triggerName, parameters));
        }
        public override void StartAudio(string charID)
        {
            if (string.IsNullOrEmpty(charID))
                return;

            InworldNDKBridge.ClientWrapper_StartAudioSession(m_Wrapper.instance, charID);
            if (!m_AudioCapture.IsCapturing)
                m_AudioCapture.StartRecording();
        }
        public override void StopAudio(string charID)
        {
            if (string.IsNullOrEmpty(charID))
                return;
            
            InworldNDKBridge.ClientWrapper_StopAudioSession(m_Wrapper.instance, charID);
            
            if (m_AudioCapture.IsCapturing)
                m_AudioCapture.StopRecording();
        }
        public override void SendAudio(string charID, string base64)
        {
            if (string.IsNullOrEmpty(charID) || string.IsNullOrEmpty(base64))
                return;
            var audioChunk = InworldPacketConverter.To.AudioChunk(charID, base64);
            byte[] data = audioChunk.DataChunk.Chunk.ToByteArray();
            
            InworldNDKBridge.ClientWrapper_SendSoundMessage(m_Wrapper.instance, charID, data, data.Length);
        }

        Task _StartSession()
        {
            if (!IsTokenValid && !String.IsNullOrEmpty(m_CustomToken))
            {
                _ReceiveCustomToken();
                InworldLog.Log("Custom token received");
            }
            
            _ResetCommunicationData();
            if(Status == InworldConnectionStatus.Initialized)
                Status = InworldConnectionStatus.Connected;
            return Task.CompletedTask;
        }

        public void Authenticate(Inworld.Token sessionToken = null)
        {
            m_Options.AuthUrl = m_ServerConfig.studio;
            m_Options.LoadSceneUrl = m_ServerConfig.RuntimeServer;
            m_Options.SceneName = InworldController.Instance.CurrentScene;
            m_Options.PlayerName = InworldAI.User.Name;
            m_Options.ApiKey = m_APIKey;
            m_Options.ApiSecret = m_APISecret;
            InworldAI.Log("GetAppAuth with wrapper options");
            m_Callback = new LoadSceneCallbackType(LoadSceneCallback);
            m_Options.Capabilities = InworldPacketConverter.To.Capabilities;

            byte[] serializedData = m_Options.ToByteArray();

            if (sessionToken != null)
            {
                m_SessionInfo.Token = sessionToken.token;
                m_SessionInfo.SessionId = sessionToken.sessionId;
            }
            else
            {
                m_SessionInfo.Token = "";
                m_SessionInfo.SessionId = "";
            }
            
            m_SessionInfo.ExpirationTime = DateTime.UtcNow.AddHours(1).ToFileTime();;
            m_SessionInfo.IsValid = true;
            byte[] serializedSessionInfo = m_SessionInfo.ToByteArray();

            InworldNDKBridge.ClientWrapper_StartClientWithCallback(m_Wrapper.instance, serializedData,
                                                                    serializedData.Length, serializedSessionInfo, serializedSessionInfo.Length, m_Callback);
        }
        
        public void LoadSceneCallback(IntPtr serializedAgentInfoArray, int serializedAgentInfoArraySize)
        {
            byte[] serializedData = new byte[serializedAgentInfoArraySize];
            Marshal.Copy(serializedAgentInfoArray, serializedData, 0, serializedAgentInfoArraySize);
            m_AgentInfoArray = AgentInfoArray.Parser.ParseFrom(serializedData);
        
            // Now you can use agentInfoArray in your C# code

            // Free the allocated buffer
            //Marshal.FreeHGlobal(serializedAgentInfoArray);
            if(m_AgentInfosFilled != null)
                m_AgentInfosFilled.TrySetResult(true);

            Marshal.FreeCoTaskMem(serializedAgentInfoArray);
            Status = InworldConnectionStatus.Initialized;
        }
        
        async Task<Inworld.LoadSceneResponse> _LoadSceneAsync(string sceneName)
        {
            if(m_AgentInfoArray.AgentInfoList.Count == 0)
            {
                m_AgentInfosFilled = new TaskCompletionSource<bool>();
                await m_AgentInfosFilled.Task;
            }

            m_LoadSceneResponse = new Inworld.LoadSceneResponse();

            foreach(AgentInfo agentInfo in m_AgentInfoArray.AgentInfoList)
            {
                Inworld.InworldCharacterData agent = new Inworld.InworldCharacterData
                {
                    givenName = agentInfo.GivenName,
                    brainName = agentInfo.BrainName,
                    agentId = agentInfo.AgentId
                };
                m_LoadSceneResponse.agents.Add(agent);
            }
            m_AgentInfoArray.AgentInfoList.Reverse();
            
            Status = InworldConnectionStatus.LoadingSceneCompleted;
            return m_LoadSceneResponse;
        }
        
        void _SendPacket(InworldPacket packet)
        {
            if (Status == InworldConnectionStatus.Connected)
                m_OutgoingEventsQueue.Enqueue(packet);
        }
        void ResolvePackets(InworldPacket response)
        {
            if (response.DataChunk != null)
            {
                switch (response.DataChunk.Type)
                {
                    case DataChunk.Types.DataType.Audio:
                        Dispatch(InworldPacketConverter.From.NDKAudioChunk(response));
                        break;
                    case DataChunk.Types.DataType.State:
                    case DataChunk.Types.DataType.Silence:
                        // TODO(YAN): Support State and Silence.
                        break;
                    default:
                        InworldAI.LogError($"Unsupported incoming event: {response}");
                        Dispatch(InworldPacketConverter.From.NDKPacket(response));
                        break;
                }
            }
            else if (response.Text != null)
            {
                Dispatch(InworldPacketConverter.From.NDKTextPacket(response));
            }
            else if (response.Control != null)
            {
                Dispatch(InworldPacketConverter.From.NDKControlPacket(response));
            }
            else if (response.Emotion != null)
            {
                Dispatch(InworldPacketConverter.From.NDKEmotionPacket(response));
            }
            else if (response.Action != null)
            {
                Dispatch(InworldPacketConverter.From.NDKActionPacket(response));
            }
            else if (response.Custom != null)
            {
                Dispatch(InworldPacketConverter.From.NDKCustomPacket(response));
            }
            else
            {
                Debug.LogError($"YAN UnSupported {response}");
                Dispatch(InworldPacketConverter.From.NDKPacket(response));
            }
        }
        void _ReceiveCustomToken()
        {
            JObject data = JObject.Parse(m_CustomToken);
            if (data.ContainsKey("sessionId") && data.ContainsKey("token"))
            {
                InworldAI.Log("Init Success with Custom Token!");
                new Metadata
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
            m_IncomingEventsQueue.Clear();
            m_OutgoingEventsQueue.Clear();
        }
    }
}


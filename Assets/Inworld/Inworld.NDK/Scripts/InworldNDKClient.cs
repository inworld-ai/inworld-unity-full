using Google.Protobuf;
using Inworld.Packet;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Inworld.NDK
{
    public class InworldNDKClient : InworldClient
    {
        InworldNDKBridge m_Wrapper;
        ConnectionStateCallbackType m_ConnectionCallback;
        PacketCallbackType m_PacketCallback;
        LogCallbackType m_LogCallbackType;
        TokenCallbackType m_TokenCallbackType;
        LoadSceneCallbackType m_Callback;

        float m_LastPacketSendTime;
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
            m_LogCallbackType = LogCallback;
            m_TokenCallbackType = TokenCallback;
            m_Wrapper = new InworldNDKBridge(m_ConnectionCallback, m_PacketCallback, m_LogCallbackType, m_TokenCallbackType);
        }

        void ConnectionStateCallback(ConnectionState state)
        {
            InworldAI.Log("connection state from ndk is " + state.ToString());

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

        void PacketCallback(IntPtr packetWrapper, int packetSize)
        {
            // Create a byte array to store the data
            byte[] data = new byte[packetSize];
            // Copy the data from the IntPtr to the byte array
            Marshal.Copy(packetWrapper, data, 0, packetSize);

            // Deserialize the byte array to an InworldPacket instance using protobuf
            InworldPacket response = InworldPacket.Parser.ParseFrom(data);
            if (response != null)
                ResolvePackets(response);
            
            Marshal.FreeCoTaskMem(packetWrapper);
        }
        
        void TokenCallback(string token)
        {
            //for memory safety, we copy the token
            m_CustomToken = String.Copy(token);
            Marshal.FreeCoTaskMem(Marshal.StringToHGlobalAnsi(token));
            if(_ReceiveCustomToken())
                InworldAI.Log("Valid NDK Token received");
        }
        
        void LogCallback(string message, int severity)
        {
            if(severity == 0)
                InworldAI.Log(message);
            else if(severity == 1)
                InworldAI.LogWarning(message);
            else if(severity == 2)
                InworldAI.LogError(message);
            else
                InworldAI.LogException(message);
            
            //Marshal.FreeCoTaskMem(Marshal.StringToHGlobalAnsi(message));
        }

        void Update()
        {
            if (Status == InworldConnectionStatus.Connected)
            {
                if (Time.time - m_LastPacketSendTime > 0.1f)
                {
                    m_LastPacketSendTime = Time.time;

                    while (m_OutgoingEventsQueue.TryDequeue(out InworldPacket e))
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
        void _EndSession()
        {
            _ResetCommunicationData();
            m_OutgoingEventsQueue.Clear();
            InworldNDKBridge.ClientWrapper_StopClient(m_Wrapper.instance);
        }

        public override void Disconnect()
        {
            _EndSession();
        }
        public override void GetAccessToken() => Authenticate();
#pragma warning disable CS4014
        public override void LoadScene(string sceneFullName) => _LoadSceneAsync(sceneFullName);
        public override Inworld.LoadSceneResponse GetLiveSessionInfo() => m_LoadSceneResponse;
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
                Marshal.FreeHGlobal(packetPtr); // Make sure to free the allocated memory
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
            
            InworldPacket audioChunk = InworldPacketConverter.To.AudioChunk(charID, base64);
            byte[] data = audioChunk.DataChunk.Chunk.ToByteArray();
            InworldNDKBridge.ClientWrapper_SendSoundMessage(m_Wrapper.instance, charID, data, data.Length);
        }

        void _StartSession()
        {
            _ResetCommunicationData();
            if (Status == InworldConnectionStatus.LoadingSceneCompleted)
                Status = InworldConnectionStatus.Connected;

            if (Status != InworldConnectionStatus.Connected)
                Authenticate();
        }

        void Authenticate()
        {
            m_Options.ServerUrl = m_ServerConfig.RuntimeServer;
            m_Options.SceneName = InworldController.Instance.CurrentScene;
            m_Options.PlayerName = InworldAI.User.Name;
            m_Options.ApiKey = m_APIKey;
            m_Options.ApiSecret = m_APISecret;
            InworldAI.Log("GetAppAuth with wrapper options");
            m_Callback = LoadSceneCallback;
            m_Options.Capabilities = InworldPacketConverter.To.Capabilities;

            byte[] serializedData = m_Options.ToByteArray();

            if (m_Token != null)
            {
                m_SessionInfo.Token = m_Token.token;
                m_SessionInfo.SessionId = m_Token.sessionId;
                InworldAI.Log("Connecting to previous session with token");
            }
            else
            {
                m_SessionInfo.Token = "";
                m_SessionInfo.SessionId = "";
            }

            m_SessionInfo.ExpirationTime = DateTime.UtcNow.AddHours(1).ToFileTime();
            m_SessionInfo.IsValid = true;
            byte[] serializedSessionInfo = m_SessionInfo.ToByteArray();

            InworldNDKBridge.ClientWrapper_StartClientWithCallback
            (
                m_Wrapper.instance, serializedData,
                serializedData.Length, serializedSessionInfo, serializedSessionInfo.Length, m_Callback, m_TokenCallbackType
            ); 
        }

        void LoadSceneCallback(IntPtr serializedAgentInfoArray, int serializedAgentInfoArraySize)
        {
            Status = InworldConnectionStatus.LoadingScene;
            byte[] serializedData = new byte[serializedAgentInfoArraySize];
            Marshal.Copy(serializedAgentInfoArray, serializedData, 0, serializedAgentInfoArraySize);
            m_AgentInfoArray = AgentInfoArray.Parser.ParseFrom(serializedData);

            // Now you can use agentInfoArray in your C# code

            if (m_AgentInfosFilled != null)
                m_AgentInfosFilled.TrySetResult(true);

            // Free the allocated buffer
            Marshal.FreeCoTaskMem(serializedAgentInfoArray);
        }

        async Task _LoadSceneAsync(string sceneName)
        {
            if (m_AgentInfoArray.AgentInfoList.Count == 0)
            {
                m_AgentInfosFilled = new TaskCompletionSource<bool>();
                await m_AgentInfosFilled.Task;
            }

            m_LoadSceneResponse = new Inworld.LoadSceneResponse();

            foreach (AgentInfo agentInfo in m_AgentInfoArray.AgentInfoList)
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
        bool _ReceiveCustomToken()
        {
            string[] parts = m_CustomToken.Split('|');

            if (parts.Length != 4) {
                Error = "NDK Token format invalid";
                return false;
            }

            string sessionId = parts[0];
            string token = parts[1];
            //For future use with saved session states
            //string sessionSavedState = parts[2];
            long expirationTime = long.Parse(parts[3]);

            if (!string.IsNullOrEmpty(sessionId) && !string.IsNullOrEmpty(token)) {
                m_Token = new Inworld.Token {
                    sessionId = parts[0],
                    token = parts[1],
                    type = "Bearer",
                    expirationTime = UnixTimestampToExpirationTime(expirationTime)
                };
                Status = InworldConnectionStatus.Initialized;
                return true;
            } else {
                Error = "Token Invalid";
                return false;
            }
        }
        
        public string UnixTimestampToExpirationTime(long unixTimestamp)
        {
            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime expirationDateTime = epochStart.AddSeconds(unixTimestamp);
            return expirationDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }
        
        void _ResetCommunicationData()
        {
            m_IncomingEventsQueue.Clear();
            m_OutgoingEventsQueue.Clear();
        }

        void OnApplicationQuit()
        {
            InworldNDKBridge.ClientWrapper_DestroyClient(m_Wrapper.instance);
        }
    }
}

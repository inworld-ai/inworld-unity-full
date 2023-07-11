using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Inworld.Packet;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;


namespace Inworld.NDK
{
    public class GenerateTokenRequest
    {
        public string key { get; set; }
        public List<string> resources { get; set; }
    }
    
    public class InworldNDKClient : InworldClient
    {
        [SerializeField] string m_APIKey;
        [SerializeField] string m_APISecret;
        [SerializeField] string m_CustomToken;
        
        public bool useAEC = false;
        InworldNDKBridge m_Wrapper;
        ConnectionStateCallbackType m_ConnectionCallback;
        PacketCallbackType m_PacketCallback;
        LoadSceneCallbackType callback;
        float lastPacketSendTime = 0f;
        private TaskCompletionSource<bool> agentInfosFilled;

    #region Wrapper variables
        SessionInfo m_SessionInfo = new SessionInfo();
        ClientOptions m_Options = new ClientOptions();
        AgentInfoArray agentInfoArray = new AgentInfoArray();
    #endregion
        
        ConcurrentQueue<InworldPacket> m_IncomingAudioQueue = new ConcurrentQueue<InworldPacket>();
        ConcurrentQueue<InworldPacket> m_IncomingEventsQueue = new ConcurrentQueue<InworldPacket>();
        ConcurrentQueue<InworldPacket> m_OutgoingEventsQueue = new ConcurrentQueue<InworldPacket>();
        Inworld.LoadSceneResponse m_LoadSceneResponse; // Yan: Grpc.LoadSceneResponse
        Channel m_Channel;
        Metadata m_Header;
        float m_CoolDown;

        string LastState { get; set; }
        
        protected override void Init()
        {
            base.Init();
            m_ConnectionCallback = new ConnectionStateCallbackType(ConnectionStateCallback);
            m_PacketCallback = new PacketCallbackType(PacketCallback);
            m_Wrapper = new InworldNDKBridge(m_ConnectionCallback, m_PacketCallback);
            m_Channel = new Channel(m_ServerConfig.RuntimeServer, new SslCredentials());
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
            ResolvePackets(response);
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
                if (Time.time - lastPacketSendTime > 0.1f)
                {
                    lastPacketSendTime = Time.time;

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
        async Task _EndSession()
        {
            _ResetCommunicationData();
            m_Token = null;
            InworldNDKBridge.ClientWrapper_StopClient(m_Wrapper.instance);
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
        
        internal async Task _StartSession()
        {
            if (!IsTokenValid)
                throw new ArgumentException("No sessionKey to start Inworld session, use CreateWorld first.");
            _ResetCommunicationData();
            if(Status == InworldConnectionStatus.Initialized)
                Status = InworldConnectionStatus.Connected;
            // try
            // {
            //     using (m_StreamingCall = m_WorldEngineClient.Session(m_Header))
            //     {
            //         // https://grpc.github.io/grpc/csharp/api/Grpc.Core.IAsyncStreamReader-1.html
            //         Task inputTask = Task.Run
            //         (
            //             async () =>
            //             {
            //                 while (Status == InworldConnectionStatus.Connected)
            //                 {
            //                     bool next;
            //                     try
            //                     {
            //                         // Waiting response for some time before checking if done.
            //                         next = await m_StreamingCall.ResponseStream.MoveNext();
            //                     }
            //                     catch (RpcException rpcException)
            //                     {
            //                         if (rpcException.StatusCode == StatusCode.Cancelled)
            //                         {
            //                             next = false;
            //                         }
            //                         else
            //                         {
            //                             // rethrowing other errors.
            //                             throw;
            //                         }
            //                     }
            //                     if (next)
            //                     {
            //                         m_IncomingEventsQueue.Enqueue(m_StreamingCall.ResponseStream.Current);
            //                     }
            //                     else
            //                     {
            //                         InworldAI.Log("Session is closed.");
            //                         break;
            //                     }
            //                 }
            //             }
            //         );
            //         Task outputTask = Task.Run
            //         (
            //             async () =>
            //             {
            //                 while (Status == InworldConnectionStatus.Connected)
            //                 {
            //                     Task.Delay(100).Wait();
            //                     // Sending all outgoing events.
            //                     while (m_OutgoingEventsQueue.TryDequeue(out InworldPacket e))
            //                     {
            //                         if (Status == InworldConnectionStatus.Connected)
            //                         {
            //                             await m_StreamingCall.RequestStream.WriteAsync(e);
            //                         }
            //                     }
            //                 }
            //             }
            //         );
            //         await Task.WhenAll(inputTask, outputTask);
            //     }
            // }
            // catch (Exception e)
            // {
            //     if (e.Message.Contains("inactivity"))
            //         Status = InworldConnectionStatus.LostConnect;
            //     else
            //         Error = e.ToString();
            // }
            // finally
            // {
            //     Status = InworldConnectionStatus.Idle;
            // }
        }

        public async Task<Inworld.Token> GenerateTokenAsync(GenerateTokenRequest tokenRequest)
        {
            TaskCompletionSource<Inworld.Token> tcs = new TaskCompletionSource<Inworld.Token>();

            UnityWebRequest request = UnityWebRequest.Get(m_ServerConfig.TokenServer);
            Status = InworldConnectionStatus.Initializing;

            AsyncOperation asyncOperation = request.SendWebRequest();

            asyncOperation.completed += (operation) =>
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Inworld.InworldLog.Log(request.error);
                    tcs.SetResult(null);
                    return;
                }

                var json = JObject.Parse(request.downloadHandler.text);

                Debug.Log("Get Token Response: " + request.downloadHandler.text);
                m_Token = new Inworld.Token();
                m_Token.token = json.GetValue("token").ToString();
                m_Token.type = json.GetValue("type").ToString();
                m_Token.sessionId = json.GetValue("sessionId").ToString();

                // Parse the expirationTime as Protobuf Timestamp
                var expirationTimeStr = json.GetValue("expirationTime").ToString();
                Debug.Log("Expiration time is " + expirationTimeStr);
                DateTime dateTime = DateTime.ParseExact(expirationTimeStr, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                Debug.Log("Expiration datetime is " + dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"));
                m_Token.expirationTime = dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");//dateTime.ToLongDateString();//expirationTime.ToString();//Timestamp.FromDateTime(DateTime.UtcNow.AddHours(1)).ToString();//expirationTime;
                
                Debug.Log("Access token values are " + m_Token.token + " " + m_Token.sessionId + " isvalid " + m_Token.IsValid);

                if (!IsTokenValid)
                {
                    Error = "Get Token Failed";
                    tcs.SetResult(null);
                    return;
                }

                tcs.SetResult(m_Token);
            };

            return await tcs.Task;
        }
        
        async void _GenerateAccessTokenAsync()
        {
            GenerateTokenRequest gtRequest = new GenerateTokenRequest();
            gtRequest.key = m_APIKey;
            gtRequest.resources = new List<string>();
            gtRequest.resources.Add(InworldController.Instance.CurrentWorkspace);
            m_Token = await GenerateTokenAsync(gtRequest);

            Authenticate(m_Token);
        }
        
        public void Authenticate(Inworld.Token sessionToken = null)
        {
#if UNITY_EDITOR && VSP
if (!string.IsNullOrEmpty(InworldAI.User.Account))
VSAttribution.VSAttribution.SendAttributionEvent("Login Runtime", InworldAI.k_CompanyName, InworldAI.User.Account);
#endif
        
            m_Options.AuthUrl = m_ServerConfig.studio;//"api-studio.inworld.ai";//
            m_Options.LoadSceneUrl = m_ServerConfig.RuntimeServer;//"api-engine.inworld.ai:443";//
            m_Options.SceneName = InworldController.Instance.m_SceneFullName;//"workspaces/artem_v_test/scenes/demo";//
            m_Options.PlayerName = InworldAI.User.Name;//"Player"; //
            m_Options.ApiKey = m_APIKey;//"xgV17Aur4k33Qoox8OxepATdBeS0Bamu";//
            m_Options.ApiSecret = m_APISecret;// "zSpQ2d0wObaPWCiRS2oioh8nDPVf1zkg2OISrdx8Zs47bm8rpdki3s6nejza0aki";//
            InworldAI.Log("GetAppAuth with wrapper options");
            callback = new LoadSceneCallbackType(LoadSceneCallback);
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
                                                                    serializedData.Length, serializedSessionInfo, serializedSessionInfo.Length, callback);
        }
        
        public void LoadSceneCallback(IntPtr serializedAgentInfoArray, int serializedAgentInfoArraySize)
        {
            byte[] serializedData = new byte[serializedAgentInfoArraySize];
            Marshal.Copy(serializedAgentInfoArray, serializedData, 0, serializedAgentInfoArraySize);
            agentInfoArray = AgentInfoArray.Parser.ParseFrom(serializedData);
        
            // Now you can use agentInfoArray in your C# code

            // Free the allocated buffer
            //Marshal.FreeHGlobal(serializedAgentInfoArray);
            if(agentInfosFilled != null)
                agentInfosFilled.TrySetResult(true);

            Marshal.FreeCoTaskMem(serializedAgentInfoArray);
            Status = InworldConnectionStatus.Initialized;
        }
        
        public async Task<Inworld.LoadSceneResponse> _LoadSceneAsync(string sceneName)
        {
            if(agentInfoArray.AgentInfoList.Count == 0)
            {
                agentInfosFilled = new TaskCompletionSource<bool>();
                await agentInfosFilled.Task;
            }

            m_LoadSceneResponse = new Inworld.LoadSceneResponse();

            foreach(AgentInfo agentInfo in agentInfoArray.AgentInfoList)
            {
                Inworld.InworldCharacterData agent = new Inworld.InworldCharacterData
                {
                    givenName = agentInfo.GivenName,
                    brainName = agentInfo.BrainName,
                    agentId = agentInfo.AgentId
                };
                m_LoadSceneResponse.agents.Add(agent);
            }
            agentInfoArray.AgentInfoList.Reverse();

            // Div: potentially redundant session key stuff
            // m_SessionKey = m_LoadSceneResponse.Key.Split(':')[1];
            // Debug.Log($"Session ID: {m_Token.sessionId}");
            // m_Header.Add("Authorization", $"Bearer {m_SessionKey}");
            // Debug.Log("after for loop in loadasync");

            Status = InworldConnectionStatus.LoadingSceneCompleted;
            //InvokeRuntimeEvent(RuntimeStatus.LoadSceneComplete, "");
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


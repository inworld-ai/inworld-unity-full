using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityWebSocket;
using UnityEngine;
using UnityEngine.Networking;
using Inworld.Packet;

namespace Inworld
{
    public class InworldController : SingletonBehavior<InworldController>
    {
        [SerializeField] InworldServerConfig m_ServerConfig;
        [SerializeField] string m_SceneFullName;
        [SerializeField] string m_PlayerName;
        [SerializeField] Texture2D m_DefaultThumbnail;
        [SerializeField] AudioCapture m_AudioCapture;
        [SerializeField] bool m_AutoStart;
        
        List<InworldCharacterData> m_Characters = new List<InworldCharacterData>();
        WebSocket m_Socket;
        InworldConnectionStatus m_ConnectionStatus;
        InworldCharacterData m_CurrentCharacter;
        InworldCharacterData m_LastCharacter;
        string m_SessionKey;
        string m_ErrorMsg;
        Token m_Token;

        bool _IsTokenValid => m_Token != null && m_Token.IsValid;
        bool _IsSceneValid => !string.IsNullOrEmpty(m_SessionKey) && m_Characters.Count > 0;
        public static AudioCapture Audio => Instance.m_AudioCapture;
        public InworldCharacterData CurrentCharacter
        {
            get => m_CurrentCharacter;
            set
            {
                if (m_CurrentCharacter?.brainName == value?.brainName)
                    return;
                m_LastCharacter = m_CurrentCharacter;
                m_CurrentCharacter = value;
                OnCharacterChanged?.Invoke(m_LastCharacter, m_CurrentCharacter);
            }
        }
        public static Texture2D DefaultThumbnail => Instance.m_DefaultThumbnail;
        public event Action<InworldConnectionStatus> OnStatusChanged;
        public event Action<InworldCharacterData> OnCharacterRegistered;
        public event Action<InworldCharacterData, InworldCharacterData> OnCharacterChanged;
        public event Action<InworldPacket> OnPacketReceived;
        public event Action<InworldPacket> OnCharacterInteraction;

        public static string Error
        {
            get => Instance ? Instance.m_ErrorMsg : "";
            set
            {
                if (Instance)
                    Instance.m_ErrorMsg = value;
            }
        }
        public static string Player => Instance.m_PlayerName;
        public static InworldConnectionStatus Status
        {
            get => Instance.m_ConnectionStatus;
            set
            {
                Instance.m_ConnectionStatus = value;
                Instance.OnStatusChanged?.Invoke(value);
            }
        }

        void Awake()
        {
            if (m_AutoStart)
                Init();
        }
        void OnDisable()
        {
            if (m_CurrentCharacter != null)
                StopAudio(m_CurrentCharacter.agentId);
        }
        public void SwitchConnect()
        {
            if (m_ConnectionStatus == InworldConnectionStatus.Idle)
                Reconnect();
            else if (m_ConnectionStatus == InworldConnectionStatus.Connected)
                Disconnect();
        }
        public void Reconnect() => StartCoroutine(_IsTokenValid ? _StartSession() : _GetAccessToken());
        public void Init() => StartCoroutine(_GetAccessToken());
        public void Disconnect() => StartCoroutine(_DisconnectAsync());
        public void CharacterInteract(InworldPacket packet) => OnCharacterInteraction?.Invoke(packet);
        IEnumerator _DisconnectAsync()
        {
#if !UNITY_WEBGL
            if (m_AudioCapture)
                m_AudioCapture.StopRecording();
#endif
            yield return new WaitForFixedUpdate();
            CurrentCharacter = null;
            m_Socket?.CloseAsync();
            yield return new WaitForFixedUpdate();
            Status = InworldConnectionStatus.Idle;
        }
        IEnumerator _GetAccessToken()
        {
            UnityWebRequest uwr = UnityWebRequest.Get(m_ServerConfig.TokenServer);
            Status = InworldConnectionStatus.Initializing;
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Error = uwr.error;
                Status = InworldConnectionStatus.Error;
                yield break;
            }
            string responseJson = uwr.downloadHandler.text;
            yield return _LoginInworld(responseJson);
        }
        
        IEnumerator _LoginInworld(string token)
        {
            m_Token = JsonUtility.FromJson<Token>(token);
            if (!_IsTokenValid)
            {
                Status = InworldConnectionStatus.Error;
                Error = "Get Token Failed";
                yield break;
            }
            Status = InworldConnectionStatus.Initialized;
            yield return _LoadScene();
        }
        
        IEnumerator _LoadScene()
        {
            LoadSceneRequest req = new LoadSceneRequest
            {
                client = new Client
                {
                    id = "unity"
                },
                user = new User
                {
                    name = string.IsNullOrEmpty(m_PlayerName) ? "DefaultInworldUser" : m_PlayerName
                },
                capabilities = new Capabilities
                {
                    audio = true,
                    emotions = true,
                    gestures = true,
                    interruptions = true,
                    narratedActions = true,
                    silence = true,
                    text = true,
                    triggers = true,
                    continuation = false,
                    turnBasedStt = false,
                    phonemeInfo = true
                }
            };
            string json = JsonUtility.ToJson(req);
            UnityWebRequest uwr = new UnityWebRequest(m_ServerConfig.LoadSceneURL(m_SceneFullName), "POST");
            uwr.SetRequestHeader("Grpc-Metadata-session-id", m_Token.sessionId);
            uwr.SetRequestHeader("Authorization", $"Bearer {m_Token.token}");
            uwr.SetRequestHeader("Content-Type", "application/json");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            Status = InworldConnectionStatus.LoadingScene;
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Error = $"Error loading scene {m_Token.sessionId}: {uwr.error}";
                Status = InworldConnectionStatus.Error;
                yield break;
            }
            string responseJson = uwr.downloadHandler.text;
            Status = InworldConnectionStatus.LoadingSceneCompleted;
            yield return _RegisterLiveSession(responseJson);
        }
        IEnumerator _RegisterLiveSession(string loadSceneResponse)
        {
            LoadSceneResponse response = JsonUtility.FromJson<LoadSceneResponse>(loadSceneResponse);
            m_SessionKey = response.key;
            m_Characters.AddRange(response.agents);
            
            foreach (InworldCharacterData agent in m_Characters)
            {
                if (string.IsNullOrEmpty(agent.agentId))
                    continue;
                string url = agent.characterAssets.URL;
                if (!string.IsNullOrEmpty(url))
                {
                    UnityWebRequest uwr = new UnityWebRequest(url);
                    uwr.downloadHandler = new DownloadHandlerTexture();
                    yield return uwr.SendWebRequest();
                    if (uwr.isDone && uwr.result == UnityWebRequest.Result.Success)
                    {
                        agent.thumbnail = (uwr.downloadHandler as DownloadHandlerTexture)?.texture;
                    }
                }
                OnCharacterRegistered?.Invoke(agent);
            }
            if (!_IsSceneValid)
                yield break;
            yield return _StartSession();
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        IEnumerator _StartSession()
        {
            if (!_IsTokenValid)
                yield break;
            yield return new WaitForFixedUpdate();
            string[] param = {m_Token.type, m_Token.token};
            m_Socket = new WebSocket(m_ServerConfig.SessionURL(m_Token.sessionId), param);
            m_Socket.OnOpen += Socket_OnOpen;
            m_Socket.OnMessage += Socket_OnMessage;
            m_Socket.OnClose += Socket_OnClose;
            m_Socket.OnError += Socket_OnError;
            Status = InworldConnectionStatus.Connecting;
            m_Socket.ConnectAsync();
        }
        
        void Socket_OnOpen(object sender, OpenEventArgs e)
        {
            Debug.Log($"Connect {m_Token.sessionId}");
            Status = InworldConnectionStatus.Connected;
            if (m_AutoStart && m_CurrentCharacter == null && m_Characters.Count > 0)
                CurrentCharacter = m_Characters[0];
        }
        
        void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            NetworkPacketResponse response = JsonUtility.FromJson<NetworkPacketResponse>(e.Data);
            if (response == null || response.result == null)
            {
                Debug.LogError("Error Processing packets");
                return;
            }
            InworldNetworkPacket packetReceived = response.result;

            if (packetReceived.Type == PacketType.UNKNOWN)
            {
                if (e.Data.Contains("error") && e.Data.Contains("inactivity") && m_AutoStart)
                    StartCoroutine(_StartSession());
                else
                    Debug.LogError($"Error Unknown {e.Data}");
            }
            OnPacketReceived?.Invoke(packetReceived.Packet);
        }
        void Socket_OnClose(object sender, CloseEventArgs e)
        {
            Debug.Log($"Closed: StatusCode: {e.StatusCode}, Reason: {e.Reason}");
        }

        void Socket_OnError(object sender, ErrorEventArgs e)
        {
            Error = e.Message;
            Status = InworldConnectionStatus.Error;
        }
        
        public bool IsRegistered(InworldCharacterData character) => m_Characters.Any(c => c.agentId == character?.agentId);
        public InworldCharacterData GetCharacter(string agentID) => m_Characters.FirstOrDefault(c => c.agentId == agentID);
        
        // ReSharper disable Unity.PerformanceAnalysis
        public void SendText(string txtToSend, string charID = "")
        {
            if (!IsRegistered(m_CurrentCharacter))
                return;
            // 1. Cancel the existing sentence.
            InworldPacket cancel = new CancelResponsePacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "CANCEL_RESPONSE",
                packetId = new PacketId(),
                routing = new Routing(string.IsNullOrEmpty(charID) ? m_CurrentCharacter.agentId : charID),
            };
            OnPacketReceived?.Invoke(cancel); // Send Locally. Let Interaction to Gather actual cancel components. 
            // 2. Send Text.
            InworldPacket packet = new TextPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "TEXT",
                packetId = new PacketId(),
                routing = new Routing(string.IsNullOrEmpty(charID) ? m_CurrentCharacter.agentId : charID),
                text = new TextEvent(txtToSend)
            };
            string jsonToSend = JsonUtility.ToJson(packet);
            OnPacketReceived?.Invoke(packet);
            m_Socket.SendAsync(jsonToSend);
        }
        public void SendCancelEvent(CancelResponsePacket packet)
        {
            string jsonToSend = JsonUtility.ToJson(packet);
            m_Socket.SendAsync(jsonToSend);
        }
        public void SendTrigger(string triggerName, string charID = "", Dictionary<string, string> parameters = null)
        {
            InworldPacket packet = new CustomPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "CUSTOM",
                packetId = new PacketId(),
                routing = new Routing(string.IsNullOrEmpty(charID) ? m_CurrentCharacter.agentId : charID),
                custom = new CustomEvent(triggerName, parameters)
            };
            string jsonToSend = JsonUtility.ToJson(packet);
            Debug.Log($"Send Trigger {triggerName}");
            m_Socket.SendAsync(jsonToSend);
        }
        public void StartAudio(string charID = "")
        {
            Debug.Log("Start Audio Event");
            if (!IsRegistered(m_CurrentCharacter))
                return;
            InworldPacket packet = new ControlPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "TEXT",
                packetId = new PacketId(),
                routing = new Routing(string.IsNullOrEmpty(charID) ? m_CurrentCharacter.agentId : charID),
                control = new ControlEvent
                {
                    action = "AUDIO_SESSION_START"
                }
            };
            string jsonToSend = JsonUtility.ToJson(packet);
            m_Socket.SendAsync(jsonToSend);
        }
        public void StopAudio(string charID = "")
        {
            if (!IsRegistered(m_CurrentCharacter))
                return;
            InworldPacket packet = new ControlPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "TEXT",
                packetId = new PacketId(),
                routing = new Routing(string.IsNullOrEmpty(charID) ? m_CurrentCharacter.agentId : charID),
                control = new ControlEvent
                {
                    action = "AUDIO_SESSION_END"
                }
            };
            string jsonToSend = JsonUtility.ToJson(packet);
            m_Socket.SendAsync(jsonToSend);
        }
        public void SendAudio(string base64, string charID = "")
        {
            if (!IsRegistered(m_CurrentCharacter))
                return;
            InworldPacket packet = new AudioPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "AUDIO",
                packetId = new PacketId(),
                routing = new Routing(string.IsNullOrEmpty(charID) ? m_CurrentCharacter.agentId : charID),
                dataChunk = new DataChunk
                {
                    type = "AUDIO",
                    chunk = base64
                }
            };
            string jsonToSend = JsonUtility.ToJson(packet);
            m_Socket.SendAsync(jsonToSend);
        }
        public string GetLiveSessionID(string brainName) => m_Characters.FirstOrDefault(c => c.brainName == brainName)?.agentId;
    }
}

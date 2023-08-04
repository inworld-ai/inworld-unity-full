using Inworld.Packet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityWebSocket;

namespace Inworld
{
    public class InworldWebSocketClient : InworldClient
    {
        WebSocket m_Socket;
        LoadSceneResponse m_CurrentSceneData;
        public override void GetAccessToken() => StartCoroutine(_GetAccessToken());
        public override void LoadScene(string sceneFullName) => StartCoroutine(_LoadScene(sceneFullName));
        public override void StartSession() => StartCoroutine(_StartSession());
        public override void Disconnect() => StartCoroutine(_DisconnectAsync());
        public override LoadSceneResponse GetLiveSessionInfo() => m_CurrentSceneData;

        public override void SendText(string characterID, string textToSend)
        {
            if (string.IsNullOrEmpty(characterID) || string.IsNullOrEmpty(textToSend))
                return;
            InworldPacket packet = new TextPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "TEXT",
                packetId = new PacketId(),
                routing = new Routing(characterID),
                text = new TextEvent(textToSend)
            };
            string jsonToSend = JsonUtility.ToJson(packet);
            Dispatch(packet);
            m_Socket.SendAsync(jsonToSend);
        }
        public override void SendCancelEvent(string characterID, string interactionID)
        {
            if (string.IsNullOrEmpty(characterID))
                return;
            MutationPacket cancelPacket = new MutationPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "CANCEL_RESPONSE",
                packetId = new PacketId(),
                routing = new Routing(characterID)
            };
            cancelPacket.mutation = new MutationEvent
            {
                cancelResponses = new CancelResponse
                {
                    interactionId = interactionID
                }
            };
            m_Socket.SendAsync(JsonUtility.ToJson(cancelPacket));
        }
        public override void SendTrigger(string charID, string triggerName, Dictionary<string, string> parameters)
        {
            if (string.IsNullOrEmpty(charID))
                return;
            InworldPacket packet = new CustomPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "CUSTOM",
                packetId = new PacketId(),
                routing = new Routing(charID),
                custom = new CustomEvent(triggerName, parameters)
            };
            string jsonToSend = JsonUtility.ToJson(packet);
            InworldAI.Log($"Send Trigger {triggerName}");
            m_Socket.SendAsync(jsonToSend);
        }
        public override void StartAudio(string charID)
        {
            if (string.IsNullOrEmpty(charID))
                return;

            InworldPacket packet = new ControlPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "TEXT",
                packetId = new PacketId(),
                routing = new Routing(charID),
                control = new ControlEvent
                {
                    action = "AUDIO_SESSION_START"
                }
            };
            string jsonToSend = JsonUtility.ToJson(packet);
            m_Socket.SendAsync(jsonToSend);
            if (!m_AudioCapture.IsCapturing)
                m_AudioCapture.StartRecording();
        }
        public override void StopAudio(string charID)
        {
            if (string.IsNullOrEmpty(charID))
                return;
            InworldPacket packet = new ControlPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "TEXT",
                packetId = new PacketId(),
                routing = new Routing(charID),
                control = new ControlEvent
                {
                    action = "AUDIO_SESSION_END"
                }
            };
            string jsonToSend = JsonUtility.ToJson(packet);
            m_Socket.SendAsync(jsonToSend);
        }
        public override void SendAudio(string charID, string base64)
        {
            if (string.IsNullOrEmpty(charID))
                return;
            InworldPacket packet = new AudioPacket
            {
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                type = "AUDIO",
                packetId = new PacketId(),
                routing = new Routing(charID),
                dataChunk = new DataChunk
                {
                    type = "AUDIO",
                    chunk = base64
                }
            };
            string jsonToSend = JsonUtility.ToJson(packet);
            m_Socket.SendAsync(jsonToSend);
        }
        IEnumerator _GetAccessToken()
        {
            Status = InworldConnectionStatus.Initializing;
            string responseJson = m_CustomToken;
            if (string.IsNullOrEmpty(responseJson))
            {
                if (string.IsNullOrEmpty(m_APIKey))
                {
                    Error = "Please fill API Key!";
                    yield break;
                }
                if (string.IsNullOrEmpty(m_APISecret))
                {
                    Error = "Please fill API Secret!";
                    yield break;
                }
                string header = InworldAuth.GetHeader(m_ServerConfig.runtime, m_APIKey, m_APISecret);
                UnityWebRequest uwr = new UnityWebRequest(m_ServerConfig.TokenServer, "POST");
                Status = InworldConnectionStatus.Initializing;

                uwr.SetRequestHeader("Authorization", header);
                uwr.SetRequestHeader("Content-Type", "application/json");
                AccessTokenRequest req = new AccessTokenRequest
                {
                    api_key = m_APIKey
                };
                string json = JsonUtility.ToJson(req);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
                uwr.downloadHandler = new DownloadHandlerBuffer();
                yield return uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Error = $"Error Get Token: {uwr.error}";
                }
                responseJson = uwr.downloadHandler.text;
            }
            m_Token = JsonUtility.FromJson<Token>(responseJson);
            if (!IsTokenValid)
            {
                Error = "Get Token Failed";
                yield break;
            }
            Status = InworldConnectionStatus.Initialized;
        }
        IEnumerator _LoadScene(string sceneFullName)
        {
            LoadSceneRequest req = new LoadSceneRequest
            {
                client = InworldAI.UnitySDK,
                user = InworldAI.User.Request,
                userSetting = InworldAI.User.Setting,
                capabilities = InworldAI.Capabilities
            };
            string json = JsonUtility.ToJson(req);
            UnityWebRequest uwr = new UnityWebRequest(m_ServerConfig.LoadSceneURL(sceneFullName), "POST");
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
                yield break;
            }
            string responseJson = uwr.downloadHandler.text;
            m_CurrentSceneData = JsonUtility.FromJson<LoadSceneResponse>(responseJson);
            Status = InworldConnectionStatus.LoadingSceneCompleted;
        }
        IEnumerator _StartSession()
        {
            if (!IsTokenValid)
                yield break;
            yield return new WaitForFixedUpdate();
            string[] param = {m_Token.type, m_Token.token};
            m_Socket = new WebSocket(m_ServerConfig.SessionURL(m_Token.sessionId), param);
            m_Socket.OnOpen += OnSocketOpen;
            m_Socket.OnMessage += OnMessageReceived;
            m_Socket.OnClose += OnSocketClosed;
            m_Socket.OnError += OnSocketError;
            Status = InworldConnectionStatus.Connecting;
            m_Socket.ConnectAsync();
        }
        IEnumerator _DisconnectAsync()
        {
#if !UNITY_WEBGL
            if (m_AudioCapture)
                m_AudioCapture.StopRecording();
#endif
            yield return new WaitForFixedUpdate();
            m_Socket?.CloseAsync();
            yield return new WaitForFixedUpdate();
            Status = InworldConnectionStatus.Idle;
        }
        void OnSocketOpen(object sender, OpenEventArgs e)
        {
            InworldAI.Log($"Connect {m_Token.sessionId}");
            Status = InworldConnectionStatus.Connected;
        }
        
        void OnMessageReceived(object sender, MessageEventArgs e)
        {
            NetworkPacketResponse response = JsonUtility.FromJson<NetworkPacketResponse>(e.Data);
            if (response == null || response.result == null)
            {
                InworldAI.LogError($"Error Processing packets {e.Data}");
                return;
            }
            InworldNetworkPacket packetReceived = response.result;

            if (packetReceived.Type == PacketType.UNKNOWN)
            {
                if (e.Data.Contains("error"))
                {
                    if (e.Data.Contains("inactivity")) // && m_AutoStart)
                        Status = InworldConnectionStatus.LostConnect;
                    else
                        Error = e.Data;
                }
            }
            Dispatch(packetReceived.Packet);
        }
        void OnSocketClosed(object sender, CloseEventArgs e)
        {
            InworldAI.Log($"Closed: StatusCode: {e.StatusCode}, Reason: {e.Reason}");
        }

        void OnSocketError(object sender, ErrorEventArgs e)
        {
            Error = e.Message;
        }
    }
}

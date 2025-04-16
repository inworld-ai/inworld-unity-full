/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Packet;
using Inworld.Entities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;


namespace Inworld
{
    public class InworldClient : MonoBehaviour
    {
#region Inspector Variables 
        [SerializeField] protected string m_SceneName;
        [Tooltip("If checked, we'll automatically find the first scene belonged to the characters.")] 
        [SerializeField] protected bool m_AutoScene = false;
        [SerializeField] protected int m_MaxWaitingListSize = 100;
        [Space(10)]
        [Header("Conversation history:")]
        [SerializeField] protected Continuation m_Continuation;
        [Space(10)]
        [Header("MultiAgents settings:")]
        [Tooltip("Toggle this will enable group chat. Characters will be in the same conversation")]
        [SerializeField] protected bool m_EnableGroupChat = true;
        [Tooltip("Toggle this will enable auto chat. Characters will talk to each other. (Must enable group chat first)")]
        [SerializeField] protected bool m_AutoChat = false;
#endregion

#region Events
        public event Action<InworldConnectionStatus> OnStatusChanged;
        public event Action<InworldError> OnErrorReceived;
        public event Action<InworldPacket> OnPacketSent;
        public event Action<InworldPacket> OnGlobalPacketReceived;
        public event Action<LogPacket> OnLogReceived;
        public event Action<InworldPacket> OnPacketReceived;
        public event Action<bool> OnGroupChatChanged; 
        public event Action<bool> OnAutoChatChanged;
#endregion

#region Private variables
        // These data will always be updated once session is refreshed and character ID is fetched. 
        // key by character's brain ID. Value contains its live session ID.
        protected readonly Dictionary<string, InworldCharacterData> m_LiveSessionData = new Dictionary<string, InworldCharacterData>();
        protected readonly Dictionary<string, Feedback> m_Feedbacks = new Dictionary<string, Feedback>();
        protected readonly ConcurrentQueue<InworldPacket> m_Prepared = new ConcurrentQueue<InworldPacket>();
        protected readonly List<InworldPacket> m_Sent = new List<InworldPacket>();
        protected WebSocket m_Socket;
        protected LiveInfo m_LiveInfo = new LiveInfo();
        protected const string k_DisconnectMsg = "The remote party closed the WebSocket connection without completing the close handshake.";

        protected IEnumerator m_OutgoingCoroutine;
        protected InworldConnectionStatus m_Status;
        protected InworldError m_Error;
        float m_ReconnectTimer;
        int m_CurrentReconnectThreshold = 1;
        int m_ReconnectThreshold = 1;
        int m_PingpongLatency = 20;

#endregion

#region Properties
        /// <summary>
        /// Gets the current Live Info. 
        /// </summary>
        public LiveInfo Current => m_LiveInfo;
        /// <summary>
        /// Gets the live session data.
        /// key by character's full name (aka brainName) value by its agent ID.
        /// </summary>
        public Dictionary<string, InworldCharacterData> LiveSessionData => m_LiveSessionData;
        /// <summary>
        /// Get/Set if group chat is enabled.
        /// </summary>
        public bool EnableGroupChat
        {
            get => m_EnableGroupChat;
            set
            {
                if (m_EnableGroupChat == value)
                    return;
                m_EnableGroupChat = value;
                OnGroupChatChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// Get/Set if the group is in AutoChat mode.
        /// </summary>
        public bool AutoChat
        {
            get => m_AutoChat;
            set
            {
                if (m_AutoChat == value)
                    return;
                m_AutoChat = value;
                OnAutoChatChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// Get/Set if the current interaction is ended by player's interruption
        /// </summary>
        public bool IsPlayerCancelling {get; set;}
        /// <summary>
        /// Gets if it's sampling audio latency.
        /// </summary>
        public bool EnableAudioLatencyReport { get; set; } = false;
        /// <summary>
        /// Get/Set the session history.
        /// </summary>
        public string SessionHistory { get; set; }
        /// <summary>
        /// Get/Set if client will automatically search for a scene for the selected characters.
        /// </summary>
        public bool AutoSceneSearch
        {
            get => m_AutoScene;
            set => m_AutoScene = value;
        }
        /// <summary>
        /// Gets the ping pong latency.
        /// </summary>
        public int Ping => m_PingpongLatency;
        
        /// <summary>
        /// Gets/Sets the current full name of the Inworld scene.
        /// </summary>
        public string CurrentScene
        {
            get
            {
                if (string.IsNullOrEmpty(m_SceneName))
                    return "";
                string[] splits = m_SceneName.Split('/');
                if (splits.Length == 4)
                    return m_SceneName;
                if (!InworldController.Instance || !InworldController.Instance.GameData)
                    return "";
                return InworldAI.GetSceneFullName(InworldController.Instance.GameData.workspaceName, m_SceneName);
            }
            set => m_SceneName = value;
        }

        /// <summary>
        /// Gets/Sets the current status of Inworld client.
        /// If set, it'll invoke OnStatusChanged events.
        /// </summary>
        public virtual InworldConnectionStatus Status
        {
            get => m_Status;
            set
            {
                if (m_Status == value)
                    return;
                m_Status = value;
                OnStatusChanged?.Invoke(value);
            }
        }
        /// <summary>
        /// Gets/Sets the error message.
        /// </summary>
        public virtual string ErrorMessage
        {
            get => m_Error?.message ?? "";
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    m_Error = null;
                    return;
                }
                Error = new InworldError(value);
            }
        }
        /// <summary>
        /// Gets/Sets the error.
        /// If the error is no retry, it'll also set the status of this client to be error.
        /// </summary>
        public virtual InworldError Error
        {
            get => m_Error;
            set
            {
                m_Error = value;
                if (m_Error == null || !m_Error.IsValid)
                    return;
                InworldAI.LogError(m_Error.message);
                OnErrorReceived?.Invoke(m_Error);
                m_CurrentReconnectThreshold *= 2;
                m_ReconnectTimer = m_CurrentReconnectThreshold;
                if (m_Error.RetryType == ReconnectionType.UNDEFINED || m_Error.RetryType == ReconnectionType.NO_RETRY)
                    Status = InworldConnectionStatus.Error; 
            }
        }

        internal string SceneFullName
        {
            get
            {
                string[] splits = m_SceneName.Split('/');
                if (splits.Length == 4)
                    return m_SceneName;
                if (!InworldController.Instance || !InworldController.Instance.GameData)
                    return "";
                return InworldAI.GetSceneFullName(InworldController.Instance.GameData.workspaceName, m_SceneName);
            }
            set => m_SceneName = value;
        }

        #endregion

#region Unity LifeCycle
        protected virtual void OnEnable()
        {
            if (InworldController.Instance)
                InworldController.Instance.OnControllerStatusChanged += OnTokenStatusChanged;
            m_OutgoingCoroutine = OutgoingCoroutine();
            m_CurrentReconnectThreshold = m_ReconnectThreshold;
            StartCoroutine(m_OutgoingCoroutine);
        }
        protected virtual void OnDisable()
        {
            if (InworldController.Instance)
                InworldController.Instance.OnControllerStatusChanged -= OnTokenStatusChanged;
        }
        void Update()
        {
            if (Status == InworldConnectionStatus.Error)
                m_ReconnectTimer = m_ReconnectTimer - Time.unscaledDeltaTime < 0 ? 0 : m_ReconnectTimer - Time.unscaledDeltaTime;
            if (Status == InworldConnectionStatus.Error && m_ReconnectTimer <= 0)
            {
                m_ReconnectTimer = m_CurrentReconnectThreshold;
                Status = InworldConnectionStatus.Idle;
            }
        }
        void OnDestroy()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            WebSocketManager webSocketManager = FindFirstObjectByType<WebSocketManager>();
            if(webSocketManager)
                Destroy(webSocketManager.gameObject);
#endif
        }
#endregion

#region APIs
        /// <summary>
        /// Get the InworldCharacterData by characters' full name.
        /// </summary>
        /// <param name="characterFullName">the request characters' Brain ID.</param>
        public InworldCharacterData GetLiveSessionCharacterDataByFullName(string characterFullName)
        {
            return m_LiveSessionData.TryGetValue(characterFullName, out InworldCharacterData value) ? value : null;
        }
        /// <summary>
        /// Get the InworldCharacterData by characters' full name.
        /// </summary>
        /// <param name="characterFullNames">the request characters' Brain ID.</param>
        public Dictionary<string, string> GetLiveSessionCharacterDataByFullNames(List<string> characterFullNames)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (characterFullNames == null || characterFullNames.Count == 0)
                return result;
            foreach (string brainID in characterFullNames)
            {
                if (!EnableGroupChat && result.Count == 1)
                    break;
                if (m_LiveSessionData.TryGetValue(brainID, out InworldCharacterData value))
                    result[brainID] = value.agentId;
                else
                    result[brainID] = "";
            }
            return result;
        }
        /// <summary>
        /// Disconnect Inworld Async.
        /// Will wait until Status is reset to idle.
        /// </summary>
        public IEnumerator DisconnectAsync()
        {
            yield return new WaitForEndOfFrame();
            m_Socket?.CloseAsync();
            yield return new WaitUntil(() => Status == InworldConnectionStatus.Idle);
        }
        /// <summary>
        /// Gets the InworldCharacterData by the given agentID.
        /// Usually used when processing packets, but don't know it's sender/receiver of characters.
        /// </summary>
        public InworldCharacterData GetCharacterDataByID(string agentID) => 
            LiveSessionData.Values.FirstOrDefault(c => !string.IsNullOrEmpty(agentID) && c.agentId == agentID);
        /// <summary>
        /// Gets the scene name by the given target characters.
        /// </summary>
        /// <returns></returns>
        public List<string> GetSceneNameByCharacter()
        {
            if (m_Prepared.Count == 0)
                return null;
            List<string> characterFullNames = m_Prepared.FirstOrDefault()?.OutgoingTargets.Keys.ToList();
            List<string> result = new List<string>();
            foreach (InworldWorkspaceData wsData in InworldAI.User.Workspace)
            {
                string output = wsData.GetSceneNameByCharacters(characterFullNames);
                if (!string.IsNullOrEmpty(output))
                {
                    result.Add(output); // Currently, we can only support loading 1 scene per session.
                    return result;
                }
            }
            return characterFullNames;
        }
        /// <summary>
        /// Prepare the session. If the session is freshly established. Please call this.
        /// </summary>
        /// <param name="loadHistory">check if you're trying to load the history</param>
        /// <param name="gameSessionID">Add your customized gameSessionID for better user data control.</param>
        public virtual IEnumerator PrepareSession(bool loadHistory = true, string gameSessionID = "")
        {
            SendSessionConfig(loadHistory, gameSessionID);
            yield return null;
            LoadScene(CurrentScene);
        }
        /// <summary>
        /// Send Feedback data to server.
        /// </summary>
        /// <param name="interactionID">The feedback bubble's interactionID</param>
        /// <param name="correlationID">The feedback bubble's correlationID</param>
        /// <param name="feedback">The actual feedback content</param>
        public virtual void SendFeedbackAsync(string interactionID, string correlationID, Feedback feedback)
        {
            StartCoroutine(_SendFeedBack( interactionID, correlationID, feedback));
        }
        /// <summary>
        /// Get the session history data. Stored at property SessionHistory.
        /// By default, it'll be stored in the memory only, Please store it to your local storage for future use.
        /// </summary>
        /// <param name="sceneFullName">the related scene</param>
        public virtual void GetHistoryAsync(string sceneFullName) => StartCoroutine(_GetHistoryAsync(sceneFullName));
        /// <summary>
        /// Generally send packets.
        /// Will automatically be called in outgoing queue.
        /// 
        /// Can be called directly by API. 
        /// </summary>
        public virtual bool SendPackets()
        {
            if (!m_Prepared.TryDequeue(out InworldPacket pkt))
                return false;
            if (!pkt.PrepareToSend())
                return false;
            if (string.IsNullOrEmpty(pkt.packetId.correlationId))
                pkt.packetId.correlationId = InworldAuth.Guid();
            else
                m_Sent.Add(pkt);
            if (EnableAudioLatencyReport && pkt is AudioPacket)
                OnPacketSent?.Invoke(pkt); 
            m_Socket.SendAsync(pkt.ToJson);
            return true;
        }

        /// <summary>
        /// Reconnect session or start a new session if the current session is invalid.
        /// </summary>
        public void Reconnect()
        {
            Status = InworldConnectionStatus.Initializing;
            if (InworldController.IsTokenValid)
                StartSession();
            else
                InworldController.Instance.GetAccessToken();
        }

        /// <summary>
        /// Start the session by the session ID.
        /// </summary>
        public virtual void StartSession() => StartCoroutine(_StartSession());
        /// <summary>
        /// Disconnect Inworld Server.
        /// </summary>
        public virtual void Disconnect() => StartCoroutine(DisconnectAsync());
        /// <summary>
        /// Unload current scene. Make sure to be called before loading another scene.
        /// </summary>
        public virtual void UnloadScene()
        {
            foreach (KeyValuePair<string, InworldCharacterData> data in m_LiveSessionData)
            {
                data.Value.agentId = "";
            }
        }
        /// <summary>
        /// Send LoadScene request to Inworld Server.
        /// </summary>
        /// <param name="sceneFullName">the full string of the scene to load.</param>
        public virtual bool LoadScene(string sceneFullName = "")
        {
            UnloadScene();
            InworldAI.LogEvent("Login_Runtime");
            if (!string.IsNullOrEmpty(sceneFullName))
            {
                InworldAI.Log($"Load Scene: {sceneFullName}");
                m_SceneName = sceneFullName;
                m_Socket.SendAsync(MutationPacket.LoadScene(m_SceneName));
            }
            else
            {
                List<string> result = AutoSceneSearch ? GetSceneNameByCharacter() : m_Prepared.FirstOrDefault()?.OutgoingTargets.Keys.ToList();
                if (result == null || result.Count == 0)
                {
                    InworldAI.LogError("Characters not found in the workspace");
                    return false;
                }
                if (result.Count == 1 && result[0].Split(new[] { "/scenes/" }, StringSplitOptions.None).Length > 0)
                {
                    m_SceneName = result[0];
                    InworldAI.Log($"Load Scene: {m_SceneName}");
                    m_Socket.SendAsync(MutationPacket.LoadScene(m_SceneName));
                }
                else
                {
                    InworldAI.Log($"Load Characters directly.");
                    m_Socket.SendAsync(MutationPacket.LoadCharacters(result));
                }
            }
            return true;
        }
        /// <summary>
        /// Send SessionConfig, send it immediately after session started to start conversation. 
        /// </summary>
        /// <param name="loadHistory">check if history is loaded.</param>
        /// <param name="gameSessionID">send the user's customized game session ID for analyze report.</param>
        public virtual void SendSessionConfig(bool loadHistory = true, string gameSessionID = "")
        {
            if (loadHistory)
            {
                if (!m_Continuation.IsValid && !string.IsNullOrEmpty(SessionHistory))
                {
                    m_Continuation.continuationType = ContinuationType.CONTINUATION_TYPE_EXTERNALLY_SAVED_STATE;
                    m_Continuation.externallySavedState = SessionHistory;
                }
            }
            InworldController.GameSessionID = gameSessionID;
            ControlPacket ctrlPacket = new ControlPacket
            {
                timestamp = InworldDateTime.UtcNow,
                packetId = new PacketId(),
                routing = new Routing("WORLD"),
                control = new SessionControlEvent
                {
                    sessionConfiguration = new SessionConfigurationPayload
                    {
                        capabilitiesConfiguration = InworldAI.Capabilities,
                        sessionConfiguration = new SessionConfiguration(InworldController.GameSessionID),
                        clientConfiguration = InworldAI.UnitySDK,
                        userConfiguration = InworldAI.User.Request,
                        continuation = loadHistory ? m_Continuation : null
                    }
                }
            };
            if (InworldAI.IsDebugMode)
            {
                InworldAI.Log($"Sending Capabilities: {InworldAI.Capabilities}");
                InworldAI.Log($"Sending Session Info. {InworldController.GameSessionID}"); 
                InworldAI.Log($"Sending Client Config: {InworldAI.UnitySDK}");
                InworldAI.Log($"Sending User Config: {InworldAI.User.Request}");
                if (loadHistory)
                    InworldAI.Log("Sending History data.");
            }
            InworldAI.Log("Prepare Session...");
            m_Socket.SendAsync(ctrlPacket.ToJson);
        }
        /// <summary>
        /// Send Perceived Latency Report to server.
        /// </summary>
        /// <param name="precisionToSend"></param>
        /// <param name="latencyPerceived"></param>
        public virtual void SendPerceivedLatencyReport(float latencyPerceived, Precision precisionToSend = Precision.FINE)
        {
            LatencyReportPacket latencyReport = new LatencyReportPacket
            {
                timestamp = InworldDateTime.UtcNow,
                packetId = new PacketId(),
                routing = new Routing("WORLD"),
                latencyReport = new PerceivedLatencyEvent
                {
                    perceivedLatency = new PerceivedLatency
                    {
                        precision = precisionToSend,
                        latency = $"{latencyPerceived}s"
                    }
                }
            };
            latencyReport.packetId.correlationId = InworldAuth.Guid();
            m_Socket.SendAsync(latencyReport.ToJson);
        }
        /// <summary>
        /// Send PingPong Response for latency Test.
        /// </summary>
        /// <param name="packetID">the ping packet's packet ID.</param>
        /// <param name="packetTimeStamp">the ping packet's timestamp</param>
        public virtual void SendLatencyTestResponse(PacketId packetID, string packetTimeStamp)
        {
            LatencyReportPacket latencyReport = new LatencyReportPacket
            {
                timestamp = InworldDateTime.UtcNow,
                packetId = new PacketId(),
                routing = new Routing("WORLD"),
                latencyReport = new PingPongEvent
                {
                    pingPong = new PingPong
                    {
                        type = PingPongType.PONG,
                        pingPacketId = packetID,
                        pingTimestamp = packetTimeStamp
                    }
                }
            };
            m_Socket.SendAsync(latencyReport.ToJson);
        }
        /// <summary>
        /// New Send messages to an InworldCharacter in this current scene.
        /// NOTE: 1. New method uses brain ID (aka character's full name) instead of live session ID
        /// </summary>
        /// <param name="textToSend">the message to send.</param>
        /// <param name="brainName">the list of the characters full name.</param>
        /// <param name="immediate">if this packet needs to send immediately without order (Need to make sure client is connected first).</param>
        /// <param name="resendOnReconnect">if this packet will be resent if connection terminated</param>
        public virtual bool SendTextTo(string textToSend, string brainName = null, bool immediate = false, bool resendOnReconnect = true)
        {
            if (string.IsNullOrEmpty(textToSend))
                return false;
            if (!Current.UpdateLiveInfo(brainName))
                return false;
            InworldPacket rawPkt = new TextPacket(textToSend);
            if (resendOnReconnect)
                rawPkt.packetId.correlationId = InworldAuth.Guid();
            PreparePacketToSend(rawPkt, immediate);
            return true;
        }
        /// <summary>
        /// Legacy Send messages to an InworldCharacter in this current scene.
        /// </summary>
        /// <param name="characterID">the live session ID of the single character to send</param>
        /// <param name="textToSend">the message to send.</param>
        public virtual bool SendText(string characterID, string textToSend)
        {
            if (string.IsNullOrEmpty(characterID) || string.IsNullOrEmpty(textToSend))
                return false; 
            InworldPacket packet = new TextPacket
            {
                timestamp = InworldDateTime.UtcNow,
                packetId = new PacketId(),
                routing = new Routing(characterID),
                text = new TextEvent(textToSend)
            };
            OnPacketSent?.Invoke(packet);
            m_Socket.SendAsync(packet.ToJson);
            return true;
        }
        /// <summary>
        /// New Send narrative action to an InworldCharacter in this current scene.
        /// 
        /// NOTE: 1. New method uses brain ID (aka character's full name) instead of live session ID
        ///       2. New method support broadcasting to multiple characters (Would be implemented in future).
        /// </summary>
        /// <param name="narrativeAction">the narrative action to send.</param>
        /// <param name="brainName">the list of the characters full name.</param>
        /// <param name="immediate">if this packet needs to send immediately without order (Need to make sure client is connected first).</param>
        /// <param name="resendOnReconnect">if this packet will be resent if connection terminated</param>
        public virtual bool SendNarrativeActionTo(string narrativeAction, string brainName = null, bool immediate = false, bool resendOnReconnect = true)
        {
            if (string.IsNullOrEmpty(narrativeAction))
                return false;
            if (!Current.UpdateLiveInfo(brainName))
                return false;
            InworldPacket rawPkt = new ActionPacket(narrativeAction);
            if (resendOnReconnect)
                rawPkt.packetId.correlationId = InworldAuth.Guid();
            PreparePacketToSend(rawPkt, immediate);
            return true;
        }
        /// <summary>
        /// Legacy Send a narrative action to an InworldCharacter in this current scene.
        /// </summary>
        /// <param name="characterID">the live session ID of the character to send</param>
        /// <param name="narrativeAction">the narrative action to send.</param>
        public virtual bool SendNarrativeAction(string characterID, string narrativeAction)
        {
            if (string.IsNullOrEmpty(characterID) || string.IsNullOrEmpty(narrativeAction))
                return false;
            InworldPacket packet = new ActionPacket
            {
                timestamp = InworldDateTime.UtcNow,
                packetId = new PacketId(),
                routing = new Routing(characterID),
                action = new ActionEvent
                {
                    narratedAction = new NarrativeAction
                    {
                        content = narrativeAction
                    }
                }
            };
            OnPacketSent?.Invoke(packet);
            m_Socket.SendAsync(packet.ToJson);
            return true;
        }
        /// <summary>
        /// New Send the CancelResponse Event to InworldServer to interrupt the character's speaking.
        /// NOTE: 1. New method uses brain ID (aka character's full name) instead of live session ID
        ///       2. New method support broadcasting to multiple characters.
        /// </summary>
        /// <param name="interactionID">the handle of the dialog context that needs to be cancelled.</param>
        /// <param name="utteranceID">the current utterance ID that needs to be cancelled.</param>
        /// <param name="brainName">the full name of the characters in the scene.</param>
        /// <param name="immediate">if this packet needs to send immediately without order. By default it's true (Need to make sure client is connected first).</param>
        public virtual bool SendCancelEventTo(string interactionID = "", string utteranceID = "", string brainName = null, bool immediate = true)
        {
            if (!Current.UpdateLiveInfo(brainName))
                return false;
            CancelResponse cancelResponses = new CancelResponse();
            if (!string.IsNullOrEmpty(interactionID))
                cancelResponses.interactionId = interactionID;
            if (!string.IsNullOrEmpty(utteranceID))
                cancelResponses.utteranceId = new List<string> {utteranceID};
            CancelResponseEvent mutation = new CancelResponseEvent
            {
                cancelResponses = cancelResponses
            };
            InworldPacket rawPkt = new MutationPacket(mutation);
            IsPlayerCancelling = true;
            PreparePacketToSend(rawPkt, immediate);
            return true;
        }
        /// <summary>
        /// Legacy Send the CancelResponse Event to InworldServer to interrupt the character's speaking.
        /// </summary>
        /// <param name="characterID">the live session ID of the character to send</param>
        /// <param name="utteranceID">the current utterance ID that needs to be cancelled.</param>
        /// <param name="interactionID">the handle of the dialog context that needs to be cancelled.</param>
        public virtual bool SendCancelEvent(string characterID, string interactionID = "", string utteranceID = "")
        {
            if (string.IsNullOrEmpty(characterID))
                return false;
            CancelResponse cancelResponses = new CancelResponse();
            if (!string.IsNullOrEmpty(interactionID))
                cancelResponses.interactionId = interactionID;
            if (!string.IsNullOrEmpty(utteranceID))
                cancelResponses.utteranceId = new List<string> {utteranceID};
            MutationPacket cancelPacket = new MutationPacket
            {
                timestamp = InworldDateTime.UtcNow,
                packetId = new PacketId(),
                routing = new Routing(characterID),
                mutation = new CancelResponseEvent
                {
                    cancelResponses = cancelResponses
                }
            };
            OnPacketSent?.Invoke(cancelPacket); 
            m_Socket.SendAsync(cancelPacket.ToJson);
            return true;
        }
        /// <summary>
        /// Immediately send regenerate response to the specific interaction
        /// </summary>
        /// <param name="characterID">The live session ID of the character.</param>
        /// <param name="interactionID"></param>
        public virtual bool SendRegenerateEvent(string characterID, string interactionID)
        {
            if (string.IsNullOrEmpty(characterID) || string.IsNullOrEmpty(interactionID))
                return false;
            MutationPacket regenPacket = new MutationPacket
            {
                timestamp = InworldDateTime.UtcNow,
                packetId = new PacketId(),
                routing = new Routing(characterID), 
                mutation = new RegenerateResponseEvent
                {
                    regenerateResponse = new RegenerateResponse
                    {
                        interactionId = interactionID
                    }
                }
            };
            OnPacketSent?.Invoke(regenPacket); 
            m_Socket.SendAsync(regenPacket.ToJson);
            return true;
        }
        /// <summary>
        /// Select a packet from all the responses to continue conversation.
        /// Call it only if you have multiple responses based on the current interaction.
        /// </summary>
        /// <param name="characterID">The live session ID of the character.</param>
        /// <param name="regenResponsePid">The packet ID that you want to continue.</param>
        public virtual bool SendApplyResponseEvent(string characterID, PacketId regenResponsePid)
        {
            if (string.IsNullOrEmpty(characterID))
                return false;
            MutationPacket regenPacket = new MutationPacket
            {
                timestamp = InworldDateTime.UtcNow,
                packetId = new PacketId(),
                routing = new Routing(characterID),
                mutation = new ApplyResponseEvent
                {
                    applyResponse = new ApplyResponse
                    {
                        packetId = regenResponsePid
                    }
                }
            };
            OnPacketSent?.Invoke(regenPacket); 
            m_Socket.SendAsync(regenPacket.ToJson);
            return true;
        }
        /// <summary>
        /// New Send the trigger to an InworldCharacter in the current scene.
        /// NOTE: 1. New method uses brain ID (aka character's full name) instead of live session ID
        ///       2. New method support broadcasting to multiple characters.
        /// </summary>
        /// <param name="triggerName">the name of the trigger to send.</param>
        /// <param name="parameters">the parameters and their values for the triggers.</param>
        /// <param name="brainName">the full name of the characters in the scene.</param>
        /// <param name="immediate">if this packet needs to send immediately without order. By default it's true (Need to make sure client is connected first).</param>
        /// <param name="resendOnReconnect">if this packet will be resent if connection terminated</param>
        public virtual bool SendTriggerTo(string triggerName, Dictionary<string, string> parameters = null, string brainName = null, bool immediate = false, bool resendOnReconnect = true)
        {
            if (string.IsNullOrEmpty(triggerName))
                return false;
            if (!Current.UpdateLiveInfo(brainName))
                return false;
            InworldPacket rawPkt = new CustomPacket(triggerName, parameters);
            if (resendOnReconnect)
                rawPkt.packetId.correlationId = InworldAuth.Guid();
            PreparePacketToSend(rawPkt, immediate);
            return true;
        }
        /// <summary>
        /// Legacy Send the trigger to an InworldCharacter in the current scene.
        /// </summary>
        /// <param name="charID">the live session ID of the character to send.</param>
        /// <param name="triggerName">the name of the trigger to send.</param>
        /// <param name="parameters">the parameters and their values for the triggers.</param>
        public virtual bool SendTrigger(string charID, string triggerName, Dictionary<string, string> parameters = null)
        {
            if (string.IsNullOrEmpty(charID))
                return false;
            InworldPacket packet = new CustomPacket
            {
                timestamp = InworldDateTime.UtcNow,
                packetId = new PacketId(),
                routing = new Routing(charID),
                custom = new CustomEvent(triggerName, parameters)
            };
            InworldAI.Log($"Send Trigger {triggerName}");
            m_Socket.SendAsync(packet.ToJson);
            return true;
        }
        /// <summary>
        /// New Send AUDIO_SESSION_START control events to server.
        /// NOTE: 1. New method uses brain ID (aka character's full name) instead of live session ID
        ///       2. New method support broadcasting to multiple characters.
        /// </summary>
        /// <param name="brainName">the full name of the characters to send.</param>
        /// <param name="micMode">If you'd like to enable the character interrupt you, check this option to OPEN_MIC.</param>
        /// <param name="understandingMode">By default is FULL if you'd like the server to also return the response.</param>
        /// <param name="immediate">if sending immediately (need to make sure client has connected)</param>
        public virtual bool StartAudioTo(string brainName = null, 
            MicrophoneMode micMode = MicrophoneMode.OPEN_MIC, 
            UnderstandingMode understandingMode = UnderstandingMode.FULL,
            bool immediate = false)
        {
            if (Current.AudioSession.IsSameSession(brainName))
                return true;
            StopAudioTo();
            if (!Current.UpdateLiveInfo(brainName))
                return false;
            ControlEvent control = new AudioControlEvent
            {
                action = ControlType.AUDIO_SESSION_START,
                audioSessionStart = new AudioSessionPayload
                {
                    mode = micMode,
                    understandingMode = understandingMode
                }
            };
            InworldPacket rawPkt = new ControlPacket(control);
            Current.StartAudioSession(rawPkt.packetId.packetId);
            PreparePacketToSend(rawPkt, immediate);
            InworldAI.Log($"Start talking to {Current.Name}");
            return true;
        }
        /// <summary>
        /// Legacy Send AUDIO_SESSION_START control events to server.
        /// Without sending this message, all the audio data would be discarded by server.
        /// However, if you send this event twice in a row, without sending `StopAudio()`, Inworld server will also through exceptions and terminate the session.
        /// </summary>
        /// <param name="charID">the live session ID of the character to send.</param>
        /// <param name="micMode">If you'd like to enable the character interrupt you, check this option to OPEN_MIC.</param>
        /// <param name="understandingMode">By default is FULL if you'd like the server to also return the response.</param>
        public virtual bool StartAudio(string charID, 
            MicrophoneMode micMode = MicrophoneMode.EXPECT_AUDIO_END, 
            UnderstandingMode understandingMode = UnderstandingMode.FULL)
        {
            if (string.IsNullOrEmpty(charID))
                return false;

            InworldPacket packet = new ControlPacket
            {
                timestamp = InworldDateTime.UtcNow,
                packetId = new PacketId(),
                routing = new Routing(charID),
                control = new AudioControlEvent
                {
                    action = ControlType.AUDIO_SESSION_START,
                    audioSessionStart = new AudioSessionPayload
                    {
                        mode = micMode,
                        understandingMode = understandingMode
                    }
                }
            };
            m_Socket.SendAsync(packet.ToJson);
            return true;
        }
        /// <summary>
        /// New Send AUDIO_SESSION_END control events to server to.
        /// NOTE: 1. New method uses brain ID (aka character's full name) instead of live session ID
        ///       2. New method support broadcasting to multiple characters.
        /// </summary>
        /// <param name="immediate">If immediately send message (needs connected to server first).</param>
        public virtual bool StopAudioTo(bool immediate = false)
        {
            if (!Current.AudioSession.HasStarted)
                return true;
            if (!Current.UpdateLiveInfo(Current.IsConversation ? "" : Current.AudioSession.Target))
                return false;
            ControlEvent control = new ControlEvent
            {
                action = ControlType.AUDIO_SESSION_END,
            };
            InworldPacket rawPkt = new ControlPacket(control);
            PreparePacketToSend(rawPkt, immediate);
            Current.StopAudioSession();
            InworldAI.Log($"Stop talking to {Current.Name}");
            return true;
        }
        /// <summary>
        /// Legacy Send AUDIO_SESSION_END control events to server to.
        /// </summary>
        /// <param name="charID">the live session ID of the character to send.</param>
        public virtual bool StopAudio(string charID)
        {
            if (string.IsNullOrEmpty(charID))
            {
                return false;
            }
            InworldPacket packet = new ControlPacket
            {
                timestamp = InworldDateTime.UtcNow,
                packetId = new PacketId(),
                routing = new Routing(charID),
                control = new ControlEvent
                {
                    action = ControlType.AUDIO_SESSION_END,
                }
            };
            m_Socket.SendAsync(packet.ToJson);
            return true;
        }
        /// <summary>
        /// New Send the wav data to server to a specific character.
        /// Need to make sure that AUDIO_SESSION_START control event has been sent to server.
        /// NOTE: 1. New method uses brain ID (aka character's full name) instead of live session ID
        ///       2. New method support broadcasting to multiple characters.
        /// Only the base64 string of the wave data is supported by Inworld server.
        /// Additionally, the sample rate of the wave data has to be 16000, mono channel.
        /// </summary>
        /// <param name="base64">the base64 string of the wave data to send.</param>
        /// <param name="brainName">the character's full name.</param>
        /// <param name="immediate">if you want to send the data immediately (Need connected first).</param>
        public virtual bool SendAudioTo(string base64, string brainName = null, bool immediate = false)
        {
            if (string.IsNullOrEmpty(base64))
                return false;
            
            DataChunk dataChunk = new DataChunk
            {
                type = DataType.AUDIO,
                chunk = base64
            };
            InworldPacket output = new AudioPacket(dataChunk);
            if (!immediate)
                m_Prepared.Enqueue(output);
            else if (Status == InworldConnectionStatus.Connected)
            {
                output.PrepareToSend();
                m_Socket.SendAsync(output.ToJson);
            }
            return true;
        }

        /// <summary>
        /// Legacy Send the wav data to server to a specific character.
        /// Need to make sure that AUDIO_SESSION_START control event has been sent to server.
        ///
        /// Only the base64 string of the wave data is supported by Inworld server.
        /// Additionally, the sample rate of the wave data has to be 16000, mono channel.
        /// </summary>
        /// <param name="charID">the live session ID of the character to send.</param>
        /// <param name="base64">the base64 string of the wave data to send.</param>
        public virtual bool SendAudio(string charID, string base64)
        {
            if (Status != InworldConnectionStatus.Connected)
                return false;
            if (string.IsNullOrEmpty(charID))
                return false;
            InworldPacket packet = new AudioPacket
            {
                timestamp = InworldDateTime.UtcNow,
                packetId = new PacketId(),
                routing = new Routing(charID),
                dataChunk = new DataChunk
                {
                    type = DataType.AUDIO,
                    chunk = base64
                }
            };
            OnPacketSent?.Invoke(packet);
            m_Socket.SendAsync(packet.ToJson);
            return true;
        }
        /// <summary>
        /// Send Next Turn trigger.
        /// It's not valid in Single Target mode.
        /// </summary>
        public virtual bool NextTurn()
        {
            if (AutoChat && !IsPlayerCancelling && Current.IsConversation && Current.Conversation.BrainNames.Count > 1)
                return SendTriggerTo(InworldMessenger.NextTurn);
            return false;
        }
        /// <summary>
        /// Update conversation info. 
        /// </summary>
        /// <param name="conversationID">The target conversation ID.</param>
        /// <param name="brainNames">the list of the character's brainNames</param>
        /// <param name="immediate">if it should be sent immediately. By default, it's true.</param>
        public virtual bool UpdateConversation(string conversationID = "", List<string> brainNames = null, bool immediate = true)
        {
            if (string.IsNullOrEmpty(conversationID))
                conversationID = InworldController.CharacterHandler.ConversationID;
            brainNames ??= InworldController.CharacterHandler.CurrentCharacterNames;
            if (brainNames?.Count < 1)
                return false;
            if (!Current.UpdateMultiTargets(conversationID, brainNames))
                return false;
            if (!EnableGroupChat)
                return false;
            Dictionary<string, string> characterTable = GetLiveSessionCharacterDataByFullNames(brainNames);
            ControlEvent control = new ConversationControlEvent
            {
                action = ControlType.CONVERSATION_UPDATE,
                conversationUpdate = new ConversationUpdatePayload
                {
                    participants = characterTable.Select(data => new Source(data.Value)).ToList()
                }
            };
            InworldPacket rawPkt = new ControlPacket(control, characterTable);
            PreparePacketToSend(rawPkt, immediate);
            return true;
        }
        #region Entities
        /// <summary>
        /// Create new items or update existing items.
        /// If an item with the given id already exists, it will update it instead of creating a new one.
        /// </summary>
        /// <param name="items">Set of items to create or update. Each item has to have an id. If an item with the
        ///     given id already exists on the server, it will be rewritten with new values; otherwise, a new item is created.</param>
        /// <param name="addToEntities">Set of entities names to add created entities to.</param>
        public virtual bool CreateOrUpdateItems(List<EntityItem> items, List<string> addToEntities)
        {
            if (items == null || items.Count == 0 || addToEntities == null || addToEntities.Count == 0)
                return false;
            InworldPacket rawPkt = new ItemOperationPacket()
            {
                entitiesItemsOperation = new CreateOrUpdateItemsOperationEvent(items, addToEntities)
            };
            string log = "Create Entity Items: \n";
            foreach (EntityItem entityItem in items)
                log += entityItem.ToString();
            log += "Add to Entities:";
            foreach (string entity in addToEntities)
                log += $" {entity}";
            InworldAI.Log(log);
            return PreparePacketToSend(rawPkt);
        }
        /// <summary>
        /// Add given items to given entities. If an item is already part of an entity, then does nothing. If an item
        ///     or entity does not exist, then it fails.
        /// </summary>
        /// <param name="itemIDs">Operation item ids.</param>
        /// <param name="entityNames">Operation entity names.</param>
        public virtual bool AddItemsToEntities(List<string> itemIDs, List<string> entityNames)
        {
            if (itemIDs == null || itemIDs.Count == 0 || entityNames == null || entityNames.Count == 0)
                return false;
            InworldPacket rawPkt = new ItemOperationPacket()
            {
                entitiesItemsOperation = new ItemsInEntitiesOperationEvent(ItemsInEntitiesOperation.Type.ADD, itemIDs, entityNames)
            };
            string log = "Add Items to Entities: ";
            foreach (string itemID in itemIDs)
                log += $" {itemID}";
            log += "\nAdd to Entities:";
            foreach (string entity in entityNames)
                log += $" {entity}";
            InworldAI.Log(log);
            return PreparePacketToSend(rawPkt);
        }
        /// <summary>
        /// Remove given items from given entities. If an item or entity does not exist, then errors out.
        ///     If an item is not part of an entity, then does nothing.
        /// </summary>
        /// <param name="itemIDs">Operation item ids.</param>
        /// <param name="entityNames">Operation entity names.</param>
        public virtual bool RemoveItemsFromEntities(List<string> itemIDs, List<string> entityNames)
        {
            if (itemIDs == null || itemIDs.Count == 0 || entityNames == null || entityNames.Count == 0)
                return false;
            InworldPacket rawPkt = new ItemOperationPacket()
            {
                entitiesItemsOperation = new ItemsInEntitiesOperationEvent(ItemsInEntitiesOperation.Type.REMOVE, itemIDs, entityNames)
            };
            string log = "Remove Items from Entities: ";
            foreach (string itemID in itemIDs)
                log += $" {itemID}";
            log += "\nRemove from Entities:";
            foreach (string entity in entityNames)
                log += $" {entity}";
            InworldAI.Log(log);
            return PreparePacketToSend(rawPkt);
        }
        /// <summary>
        /// Remove all items in given entities and then replace them with a new set. Errors out if an entity or item does not exist.
        /// </summary>
        /// <param name="itemIDs">Operation item ids.</param>
        /// <param name="entityNames">Operation entity names.</param>
        public virtual bool ReplaceItemsEntities(List<string> itemIDs, List<string> entityNames)
        {
            if (itemIDs == null || itemIDs.Count == 0 || entityNames == null || entityNames.Count == 0)
                return false;
            InworldPacket rawPkt = new ItemOperationPacket()
            {
                entitiesItemsOperation = new ItemsInEntitiesOperationEvent(ItemsInEntitiesOperation.Type.REPLACE, itemIDs, entityNames)
            };
            string log = "Replace Entities of Items: ";
            foreach (string itemID in itemIDs)
                log += $" {itemID}";
            log += "\nReplace with Entities:";
            foreach (string entity in entityNames)
                log += $" {entity}";
            InworldAI.Log(log);
            return PreparePacketToSend(rawPkt);
        }
        /// <summary>
        /// Removes items by id. Items are automatically removed from all entities on removal.
        /// </summary>
        /// <param name="items">Set of ids to remove items.</param>
        public virtual bool DestroyItems(List<string> itemIDs)
        {
            if (itemIDs == null || itemIDs.Count == 0)
                return false;
            InworldPacket rawPkt = new ItemOperationPacket()
            {
                entitiesItemsOperation = new RemoveItemsOperationEvent(itemIDs)
            };
            string log = "Destroy Entity Items: ";
            foreach (string itemID in itemIDs)
                log += $" {itemID}";
            InworldAI.Log(log);
            return PreparePacketToSend(rawPkt);
        }
        #endregion
#endregion

#region Private Functions
        protected virtual IEnumerator OutgoingCoroutine()
        {
            while (true)
            {
                if (m_Prepared.Count > 0)
                {
                    if (Status == InworldConnectionStatus.Connected)
                    {
                        SendPackets();
                    }
                    if (Status == InworldConnectionStatus.Idle)
                    {
                        Reconnect();
                    }
                    if (Status == InworldConnectionStatus.Initialized)
                    {
                        StartSession();
                    }
                }
                if (m_Sent.Count > m_MaxWaitingListSize)
                    m_Sent.RemoveAt(0);
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
        protected virtual void _RegisterLiveSession(List<InworldCharacterData> agents)
        {
            m_LiveSessionData.Clear();
            // YAN: Fetch all the characterData in the current session.
            foreach (InworldCharacterData agent in agents.Where(agent => !string.IsNullOrEmpty(agent.agentId) && !string.IsNullOrEmpty(agent.brainName)))
            {
                InworldCharacter character = InworldController.CharacterHandler[agent.brainName];
                m_LiveSessionData[agent.brainName] = agent;
                StartCoroutine(agent.UpdateThumbnail(character ? character.Data.thumbnail : null));
            }
        }
        protected virtual void OnTokenStatusChanged(InworldConnectionStatus status, string detail)
        {
            if (status == InworldConnectionStatus.Error)
                ErrorMessage = detail;
            else
                Status = status;
        }
        protected string _GetCallbackReference(string sessionFullName, string interactionID, string correlationID)
        {
            return $"{sessionFullName}/interactions/{interactionID}/groups/{correlationID}";
        }
        protected IEnumerator _StartSession()
        {
            if (Status == InworldConnectionStatus.Connected)
                yield break;
            Status = InworldConnectionStatus.Connecting;
            string url = InworldController.WebsocketSessionURL;
            if (string.IsNullOrEmpty(url))
                yield break;
            string[] param = {InworldController.TokenType, InworldController.Token};
            m_Socket = WebSocketManager.GetWebSocket(url);
            if (m_Socket == null)
                m_Socket = new WebSocket(url, param);
            m_Socket.OnOpen += OnSocketOpen;
            m_Socket.OnMessage += OnMessageReceived;
            m_Socket.OnClose += OnSocketClosed;
            m_Socket.OnError += OnSocketError;
            m_Socket.ConnectAsync();
        }


        void OnSocketOpen(object sender, OpenEventArgs e)
        {
            InworldAI.Log($"Connect {InworldController.SessionID}");
            StartCoroutine(PrepareSession());
        }
        /// <summary>
        /// Handle the raw packets received from server.
        /// </summary>
        /// <param name="receivedPacket"></param>
        /// <returns>True if need dispatch, False if error or discard.</returns>
        bool _HandleRawPackets(InworldPacket receivedPacket)
        {
            if (receivedPacket is LatencyReportPacket latencyReportPacket)
            {
                if (latencyReportPacket.latencyReport is PingPongEvent)
                {
                    m_PingpongLatency = InworldDateTime.ToLatency(receivedPacket.timestamp);
                    SendLatencyTestResponse(latencyReportPacket.packetId, receivedPacket.timestamp);
                }
                return false;
            }
            if (receivedPacket is SessionResponsePacket)
                return false;
            if (receivedPacket is ControlPacket controlPacket)
            {
                switch (controlPacket.Action)
                {
                    case ControlType.WARNING:
                        InworldAI.LogWarning(controlPacket.control.description);
                        return false;
                    case ControlType.INTERACTION_END:
                        _FinishInteraction(controlPacket.packetId.correlationId);
                        break;
                    case ControlType.CURRENT_SCENE_STATUS:
                        if (controlPacket.control is CurrentSceneStatusEvent currentSceneStatusEvent)
                        {
                            _RegisterLiveSession(currentSceneStatusEvent.currentSceneStatus.agents);
                            Status = InworldConnectionStatus.Connected;
                            UpdateConversation(InworldController.CharacterHandler.ConversationID, m_LiveSessionData.Keys.ToList());
                            foreach (InworldPacket pkt in m_Sent)
                            {
                                m_Prepared.Enqueue(pkt);
                            }
                            m_ReconnectThreshold = m_CurrentReconnectThreshold = 1;
                            return true;
                        }
                        InworldAI.LogError($"Load Scene Error: {controlPacket.control}");
                        break;
                }
            }
            return true;
        }

        void OnMessageReceived(object sender, MessageEventArgs e)
        {
            NetworkPacketResponse response = JsonConvert.DeserializeObject<NetworkPacketResponse>(e.Data.Replace('’', '\'')
                .Replace('‘', '\'')
                .Replace('“', '\"')
                .Replace('”', '\"'));
            if (response == null)
            {
                ErrorMessage = e.Data;
                return;
            }
            if (response.error != null && !string.IsNullOrEmpty(response.error.message))
            {
                Error = response.error;
                return;
            }
            if (response.result == null)
            {
                ErrorMessage = e.Data;
                return;
            }
            if (response.result is LogPacket logPacket)
            {
                OnLogReceived?.Invoke(logPacket);
                logPacket.Display();
                return;
            }
            InworldPacket packetReceived = response.result;
            if (!_HandleRawPackets(packetReceived))
                return;
            if (packetReceived.Source == SourceType.WORLD)
                OnGlobalPacketReceived?.Invoke(packetReceived);
            OnPacketReceived?.Invoke(packetReceived);
        }
        void OnSocketClosed(object sender, CloseEventArgs e)
        {
            InworldAI.Log($"Closed: StatusCode: {e.StatusCode}, Reason: {e.Reason}");
            // YAN: We won't store the external saved state. Please use enable loadHistory and SessionHistory for load previous history data.
            if (m_Continuation != null)
                m_Continuation.externallySavedState = ""; 
            if (Status != InworldConnectionStatus.Error)
                Status = InworldConnectionStatus.Idle;
        }
        void OnSocketError(object sender, ErrorEventArgs e)
        {
            if (e.Message != k_DisconnectMsg)
                ErrorMessage = e.Message;
        }

        protected bool PreparePacketToSend(InworldPacket rawPkt, bool immediate = false, bool needCallback = true)
        {
            if (!immediate)
                m_Prepared.Enqueue(rawPkt);
            else if (Status != InworldConnectionStatus.Connected)
                return false;
            else
            {
                rawPkt.PrepareToSend();
                m_Socket.SendAsync(rawPkt.ToJson);
            }
            if (needCallback)
                OnPacketSent?.Invoke(rawPkt);
            return true;
        }
        protected IEnumerator _GetHistoryAsync(string sceneFullName)
        {
            UnityWebRequest uwr = new UnityWebRequest(InworldController.Instance.LoadSessionURL(sceneFullName), "GET");
            uwr.SetRequestHeader("Grpc-Metadata-session-id", InworldController.SessionID);
            uwr.SetRequestHeader("Authorization", $"Bearer {InworldController.Token}");
            uwr.SetRequestHeader("Content-Type", "application/json");
            uwr.downloadHandler = new DownloadHandlerBuffer();
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                ErrorMessage = $"Error loading scene {InworldController.SessionID}: {uwr.error} {uwr.downloadHandler.text}";
                uwr.uploadHandler.Dispose();
                yield break;
            }
            string responseJson = uwr.downloadHandler.text;
            PreviousSessionResponse response = JsonUtility.FromJson<PreviousSessionResponse>(responseJson);
            SessionHistory = response.state;
            InworldAI.Log($"Get Previous Content Encrypted: {SessionHistory}");
        }
        IEnumerator _SendFeedBack(string interactionID, string correlationID, Feedback feedback)
        {
            if (string.IsNullOrEmpty(interactionID))
            {
                InworldAI.LogError("No interaction ID for feedback");
                yield break;
            }
            if (m_Feedbacks.ContainsKey(interactionID))
                yield return _PatchFeedback(interactionID, correlationID, feedback); // Patch
            else
                yield return _PostFeedback(interactionID, correlationID, feedback);
        }
        IEnumerator _PostFeedback(string interactionID, string correlationID, Feedback feedback)
        {
            string sessionFullName = InworldController.Instance.GetSessionFullName(m_SceneName);
            string callbackRef = _GetCallbackReference(sessionFullName, interactionID, correlationID);
            UnityWebRequest uwr = new UnityWebRequest(InworldController.Server.FeedbackURL(callbackRef), "POST");
            uwr.SetRequestHeader("Grpc-Metadata-session-id", InworldController.SessionID);
            uwr.SetRequestHeader("Authorization", $"Bearer {InworldController.Token}");
            uwr.SetRequestHeader("Content-Type", "application/json");
            string json = JsonUtility.ToJson(feedback);
            Debug.Log($"SEND: {json}");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            yield return uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                ErrorMessage = $"Error Posting feedbacks {uwr.downloadHandler.text} Error: {uwr.error}";
                uwr.uploadHandler.Dispose();
                uwr.downloadHandler.Dispose();
                yield break;
            }
            string responseJson = uwr.downloadHandler.text;
            InworldAI.Log($"Received: {responseJson}");
        }
        IEnumerator _PatchFeedback(string interactionID, string correlationID, Feedback feedback)
        {
            yield return _PostFeedback(interactionID, correlationID, feedback); //TODO(Yan): Use Patch instead of Post for detailed json.
        }
        void _FinishInteraction(string correlationID)
        {
            m_Sent.RemoveAll(p => p.packetId.correlationId == correlationID);
            if (IsPlayerCancelling)
                IsPlayerCancelling = false;
        }
#endregion
    }
}

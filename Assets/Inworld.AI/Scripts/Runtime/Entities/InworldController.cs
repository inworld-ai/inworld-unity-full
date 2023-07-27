/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Google.Protobuf;
using Inworld.Grpc;
using Inworld.Packets;
using Inworld.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using AudioChunk = Inworld.Packets.AudioChunk;
using ControlEvent = Inworld.Packets.ControlEvent;
using GrpcPacket = Inworld.Grpc.InworldPacket;
using InworldPacket = Inworld.Packets.InworldPacket;
using Random = UnityEngine.Random;
using Routing = Inworld.Packets.Routing;
using TextEvent = Inworld.Packets.TextEvent;


namespace Inworld
{
    public class InworldController : SingletonBehavior<InworldController>
    {
        #region Inspector Variables
        [SerializeField] bool m_AutoStart;
        [SerializeField] InworldSceneData m_Data;
        [SerializeField] GameObject m_InworldPlayer;
        [SerializeField] AudioCapture m_Capture;
        #endregion

        #region Events
        public event Action<ControllerStates> OnStateChanged;
        public event Action<InworldPacket> OnPacketReceived;
        public event Action<InworldCharacter, InworldCharacter> OnCharacterChanged;
        #endregion

        #region Private Variables
        ControllerStates m_State = ControllerStates.Idle;
        InworldClient m_Client;
        InworldCharacter m_CurrentCharacter;
        InworldCharacter m_LastCharacter;
        string m_CurrentRecordingID;
        float m_BackOffTime = 0.2f;
        float m_CurrentCountDown;
        string m_TTSInteractionID;
        bool m_StartToQuit;
        Dictionary<string, string> m_CharacterRegistration = new Dictionary<string, string>();
        List<InworldCharacter> m_Characters = new List<InworldCharacter>();
        #endregion

        #region Properties
        /// <summary>
        ///     Get/Set Audio Recording.
        ///     Can only be used when audio capture has been established.
        /// </summary>
        public static bool IsCapturing
        {
            get
            {
                if (!Instance)
                    return false;
                AudioCapture capture = Instance.m_Capture;
                return capture != null && capture.IsCapturing;
            }
            set
            {
                if (!Instance)
                    return;
                AudioCapture capture = Instance.m_Capture;
                if (!capture)
                    return;
                if (value)
                    capture.StartRecording();
                else
                    capture.StopRecording();
            }
        }
        /// <summary>
        ///     Get/Set if it's AutoStart.
        ///     Inworld Controller would immediately start session once token received if `Auto Start` is checked.
        /// </summary>
        public static bool AutoStart
        {
            get => Instance.m_AutoStart;
            set => Instance.m_AutoStart = value;
        }
        /// <summary>
        ///     Get/Set the Current Inworld Scene Data.
        /// </summary>
        public static InworldSceneData CurrentScene
        {
            get => Instance.m_Data;
            set => Instance.m_Data = value;
        }
        /// <summary>
        ///     Get/Set the current Player Controller.
        /// </summary>
        public static GameObject Player
        {
            get => Instance.m_InworldPlayer;
            set => Instance.m_InworldPlayer = value;
        }
        public static List<InworldCharacter> Characters => Instance.m_Characters;
        /// <summary>
        ///     Get/Set the current Inworld Character. Could be Null.
        ///     Usually it's set by CheckPriority().
        /// </summary>
        public InworldCharacter CurrentCharacter
        {
            get => m_CurrentCharacter;
            set
            {
                if (m_CurrentCharacter == value)
                    return;
                m_LastCharacter = m_CurrentCharacter;
                m_CurrentCharacter = value;
                if (enabled)
                    StartCoroutine(SwitchAudioCapture());
                OnCharacterChanged?.Invoke(m_LastCharacter, m_CurrentCharacter);
            }
        }
        /// <summary>
        ///     Check if Runtime session token has been received
        ///     and Inworld Client has been initialized.
        /// </summary>
        public bool HasInit => m_Client.HasInit;
        /// <summary>
        ///     Get/Set the Inworld Controller's current State.
        ///     NOTE:
        ///     Once the State has been set, its OnStateChanged event would be invoked.
        /// </summary>
        public static ControllerStates State
        {
            get => Instance.m_State;
            private set => Instance._SetState(value);
        }
        /// <summary>
        ///     Check if all the data is correct.
        /// </summary>
        public static bool IsValid
        {
            get
            {
                if (InworldAI.Game.currentWorkspace == null)
                {
                    Debug.LogError("Cannot Find Workspace!");
                    return false;
                }
                if (InworldAI.Game.APIKey == null)
                {
                    Debug.LogError("Please Input API Key!");
                    return false;
                }
                return CurrentScene && Instance.CurrentCharacter;
            }
        }
        #endregion

        #region Monobehavior Functions
        void Awake()
        {
            m_Client = new InworldClient();
            InworldAI.User.LoadData();
        }
        void Start()
        {
            if (InworldAI.Settings.SaveConversation && PlayerPrefs.HasKey(CurrentScene.fullName))
                m_Client.LastState = PlayerPrefs.GetString(CurrentScene.fullName);
            if (m_AutoStart)
                Init();
        }
        void Update()
        {
            if (m_StartToQuit && State == ControllerStates.Idle)
            {
                Application.Quit();
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
            }
            if (State == ControllerStates.LostConnect)
            {
                m_CurrentCountDown += Time.deltaTime;
                if (m_CurrentCountDown > m_BackOffTime)
                {
                    m_CurrentCountDown = 0;
                    _StartSession();
                }
            }
        }
        void OnDisable()
        {
#pragma warning disable CS4014
            Disconnect();
#pragma warning restore CS4014
            if (m_Client != null)
            {
                m_Client.RuntimeEvent -= OnRuntimeEvents;
                m_Client.Destroy();
            }
            if (m_Capture)
                m_Capture.StopRecording();
        }
        #endregion
        
        #region Callbacks
        // Translate Runtime Events to public Controller Events.
        async void OnRuntimeEvents(RuntimeStatus status, string msg)
        {
            switch (status)
            {
                case RuntimeStatus.InitSuccess:
                    State = ControllerStates.Initialized;
                    await LoadScene();
                    break;
                case RuntimeStatus.InitFailed:
                    Debug.LogError(msg);
                    State = ControllerStates.InitFailed;
                    break;
                case RuntimeStatus.LoadSceneFailed:
                    Debug.LogError(msg);
                    State = ControllerStates.Error;
                    break;
            }
        }
        #endregion
        
        #region Private Functions
        void _SetState(ControllerStates state)
        {
            if (m_State == state)
                return;
            m_State = state;
            OnStateChanged?.Invoke(state);
        }
        void _SelectCharacter()
        {
            float fPriority = float.MaxValue;
            InworldCharacter targetCharacter = null;
            foreach (InworldCharacter iwChar in Characters.Where(iwChar => iwChar.Priority >= 0 && iwChar.Priority < fPriority))
            {
                fPriority = iwChar.Priority;
                targetCharacter = iwChar;
            }
            CurrentCharacter = targetCharacter;
       }
        void _StartAudioCapture(string characterID)
        {
            if (m_CurrentRecordingID == characterID)
                return;
            m_CurrentRecordingID = characterID;
            m_Client.StartAudio(Routing.FromPlayerToAgent(characterID));
            m_Capture.StartRecording(); 
            InworldAI.Log("Capture started.");
        }
        string _GetFullNameToLoad(string fullNameToLoad)
        {
            string strResult = "";
            if (string.IsNullOrEmpty(fullNameToLoad))
            {
                if (m_Data != null)
                    strResult = m_Data.fullName;
                else
                {
                    InworldCharacter firstChild = GetFirstChild(true);
                    if (firstChild)
                    {
                        strResult = firstChild.BrainName;
                    }
                }
            }
            return strResult;
        }
        void _ListCharactersFromServer(List<LoadSceneResponse.Types.Agent> characters)
        {
            foreach (LoadSceneResponse.Types.Agent characterInfo in characters)
            {
                m_CharacterRegistration[characterInfo.BrainName] = characterInfo.AgentId;
            }
            if (characters.Count != 0)
                return;
            InworldAI.LogError("Cannot Find Characters. Need Init first.");
            State = ControllerStates.Error;
        }
        void _GetIncomingEvents()
        {
            while (m_Client.GetIncomingEvent(out InworldPacket packet))
            {
                OnPacketReceived?.Invoke(packet);
            }
            if (m_Client.GetAudioChunk(out AudioChunk audioChunkEvent))
            {
                OnPacketReceived?.Invoke(audioChunkEvent);
            }
        }

        // Handling events coming from client.
        IEnumerator InteractionCoroutine()
        {
            while (State == ControllerStates.Connected)
            {
                _GetIncomingEvents();
                if (InworldAI.Settings.AutoSelectCharacter)
                    _SelectCharacter();
                // Client stopped.
                if (!m_Client.SessionStarted && !m_Client.Errors.IsEmpty)
                {
                    while (m_Client.Errors.TryDequeue(out Exception exception))
                    {
                        if (exception.Message.Contains("inactivity"))
                        {
                            //YAN: Filter it.
                            m_BackOffTime = Random.Range(m_BackOffTime, m_BackOffTime * 2);
                            CurrentCharacter = null;
                            State = ControllerStates.LostConnect;
                            break;
                        }
                        InworldAI.LogException($"{m_Client.SessionID}: {exception.Message}");
                        State = ControllerStates.Error;
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
        void _StartSession()
        {
#pragma warning disable 4014
            // Do not await this.
            m_Client.StartSession();
#pragma warning restore 4014
            State = ControllerStates.Connected;
            InworldAI.Log($"InworldController Connected {m_Client.SessionID}");
            StartCoroutine(InteractionCoroutine());
        }
        IEnumerator SwitchAudioCapture()
        {
            if (m_LastCharacter)
            {
                InworldAI.Log($"End Audio Capture {m_LastCharacter.CharacterName}: {m_LastCharacter.ID}");
                EndAudioCapture(m_LastCharacter.ID);
            }
            yield return new WaitForFixedUpdate();
            if (m_CurrentCharacter)
            {
                InworldAI.Log($"Start Audio Capture {m_CurrentCharacter.CharacterName}: {m_CurrentCharacter.ID}");
                _StartAudioCapture(m_CurrentCharacter.ID);
            }
        }
        IEnumerator SwitchAudioCapture(string incomingID)
        {
            if (m_CurrentCharacter && m_CurrentCharacter.ID != incomingID)
            {
                InworldAI.Log($"End Audio Capture {m_CurrentCharacter.CharacterName}: {m_CurrentCharacter.ID}");
                EndAudioCapture(m_CurrentCharacter.ID);
            }
            yield return new WaitForFixedUpdate();
            if (m_CurrentCharacter)
            {
                InworldAI.Log($"Start Audio Capture {incomingID}");
                _StartAudioCapture(incomingID);
            }
        }
        internal void SendAudio(ByteString incomingChunk)
        {
            if (string.IsNullOrEmpty(m_CurrentRecordingID))
                return;
            Routing routing = Routing.FromPlayerToAgent(m_CurrentRecordingID);
            m_Client.SendAudio(new AudioChunk(incomingChunk, routing));
        }
        internal void TTSStart(string ID)
        {
            ControlEvent controlEvent = new ControlEvent(Grpc.ControlEvent.Types.Action.TtsPlaybackStart, Routing.FromAgentToPlayer(ID));
            if (m_TTSInteractionID != null)
                controlEvent.PacketId.InteractionId = m_TTSInteractionID;
            else
                m_TTSInteractionID = controlEvent.PacketId.InteractionId;
            SendEvent(controlEvent);
        }
        internal void TTSEnd(string ID)
        {
            if (string.IsNullOrEmpty(m_TTSInteractionID))
                return;
            ControlEvent controlEvent = new ControlEvent(Grpc.ControlEvent.Types.Action.TtsPlaybackEnd, Routing.FromAgentToPlayer(ID));
            controlEvent.PacketId.InteractionId = m_TTSInteractionID;
            SendEvent(controlEvent);
        }
        #endregion

        #region APIs
        /// <summary>
        ///     Initialize the SDK.
        ///     Make sure there's a valid ServerConfig (Has URI of both RuntimeServer and StudioServer)
        ///     and a valid pair of valid API Key/Secret
        /// </summary>
        public void Init(string sessionToken = "")
        {
            State = ControllerStates.Initializing;
            m_Client.RuntimeEvent += OnRuntimeEvents;
            m_Client.GetAppAuth(InworldAI.Game.APIKey, InworldAI.Game.APISecret, sessionToken);
        }
        /// <summary>
        ///     Initialize the SDK.
        ///     Make sure there's a valid ServerConfig (Has URI of both RuntimeServer and StudioServer)
        ///     and a valid pair of valid API Key/Secret
        /// </summary>
        public void InitWithCustomKey(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            string decoded = System.Text.Encoding.UTF8.GetString(bytes);
            string[] result = decoded.Split(':');
            if (result.Length <= 1)
                return;
            State = ControllerStates.Initializing;
            m_Client.RuntimeEvent += OnRuntimeEvents;
            m_Client.GetAppAuth(result[0], result[1]);
        }
        public void InitWithCustomKey(string key, string secret)
        {
            State = ControllerStates.Initializing;
            m_Client.RuntimeEvent += OnRuntimeEvents;
            m_Client.GetAppAuth(key, secret);
        }
        /// <summary>
        /// Start Recording
        /// </summary>
        /// <param name="autoPush">If autopush, whenever you finished talking, the data would be sent to server.
        /// by default is true</param>
        public void StartRecording(bool autoPush = true)
        {
            AudioCapture capture = m_Capture;
            if (!capture)
                return;
            capture.StartRecording(autoPush);
        }
        /// <summary>
        /// Stop Recording
        /// </summary>
        public void StopRecording()
        {
            AudioCapture capture = m_Capture;
            if (!capture)
                return;
            capture.StopRecording();
        }
        /// <summary>
        /// Manually Push Audio. Called when autoPush of AudioCapture is false
        /// </summary>
        public void PushAudio()
        {
            AudioCapture capture = m_Capture;
            if (!capture)
                return;
            capture.PushAudio();
        }

        public async Task LoadScene(string sceneOrCharFullName = "")
        {
            string fullNameToLoad = _GetFullNameToLoad(sceneOrCharFullName);
            if (string.IsNullOrEmpty(fullNameToLoad))
            {
                Debug.LogError("Please attach a correct Inworld Scene or a correct Inworld Character.");
                return;
            }
            try
            {
                InworldAI.Log($"InworldController Connecting {fullNameToLoad}.");
                switch (State)
                {
                    case ControllerStates.Initialized:
                    {
                        State = ControllerStates.Connecting;
                        LoadSceneResponse response = await m_Client.LoadScene(fullNameToLoad);
                        if (response != null)
                        {
                            _ListCharactersFromServer(response.Agents.ToList());
                            if (InworldAI.Settings.SaveConversation)
                            {
                                _LoadPreviousData(response);
                            }
                            _StartSession();
                        }
                        break;
                    }
                    case ControllerStates.LostConnect:
                        Debug.Log("Start Reconnecting");
                        if (m_AutoStart)
                            _StartSession();
                        break;
                }
            }
            catch (Exception e)
            {
                State = ControllerStates.Error;
                Debug.LogError(e);
            }
        }

        void _LoadPreviousData(LoadSceneResponse response)
        {
            if (response.PreviousState == null)
                return;
            foreach (PreviousState.Types.StateHolder stateHolder in response.PreviousState.StateHolders)
            {
                if (stateHolder.Packets.Count != 0)
                    InworldAI.Log(" ======= Previous Dialog: ======= ");
                foreach (GrpcPacket packet in stateHolder.Packets)
                {
                    TextEvent packets = m_Client.ResolvePreviousPackets(packet);
                    if (packets != null)
                    {
                        InworldAI.Log($">>>> {packet.Text.Text}");
                    }
                }
            }
        }
        /// <summary>
        ///     Reconnect
        /// </summary>
        public void Reconnect()
        {
            _StartSession();
        }
        /// <summary>
        ///     Disconnect
        ///     Ends server based NPC interaction.
        /// </summary>
        public async Task Disconnect()
        {
            EndAudioCapture(CurrentCharacter.ID);
            if (m_Client != null)
                await m_Client.EndSession();

            StopCoroutine(nameof(InteractionCoroutine));
            CurrentCharacter = null;
            if (InworldAI.Settings.SaveConversation)
                PlayerPrefs.SetString(CurrentScene.fullName, m_Client.LastState);
            State = ControllerStates.Idle;
        }

        /// <summary>
        ///     Send an InworldPacket to Inworld Server
        /// </summary>
        /// <param name="packet">InworldPacket</param>
        public void SendEvent(InworldPacket packet)
        {
            m_Client.SendEvent(packet);
        }

        /// <summary>
        ///     Start Communicating with target Character via Audio
        /// </summary>
        /// <param name="characterID">
        ///     string of Character ID, would be generated only after InworldScene is loaded and session is
        ///     started
        /// </param>
        public void StartAudioCapture(string characterID) => StartCoroutine(SwitchAudioCapture(characterID));


        /// <summary>
        ///     Stop Communicating with target Character via Audio
        /// </summary>
        /// <param name="characterID">
        ///     string of Character ID, would be generated only after InworldScene is loaded and session is
        ///     started
        /// </param>
        public void EndAudioCapture(string characterID = null)
        {
            if (string.IsNullOrEmpty(characterID) || characterID.Equals(m_CurrentRecordingID))
            {
                m_Client.EndAudio(Routing.FromPlayerToAgent(m_CurrentRecordingID));
                if (m_Capture)
                    m_Capture.IsCapturing = false; 
                m_CurrentRecordingID = null;
                InworldAI.Log("Capture ended.");
            }
        }

        public InworldCharacter GetFirstChild(bool isActive)
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeSelf == isActive)
                    return child.GetComponent<InworldCharacter>();
            }
            return null;
        }
        #endregion

        public void StartTerminate()
        {
            m_StartToQuit = true;
#pragma warning disable CS4014
            Disconnect();
#pragma warning restore CS4014
        }
        public string GetLiveSessionID(string brainName) => m_CharacterRegistration.ContainsKey(brainName) ? m_CharacterRegistration[brainName] : "";
    }
}

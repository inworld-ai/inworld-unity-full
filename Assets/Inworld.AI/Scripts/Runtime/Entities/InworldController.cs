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
using UnityEngine;
using AudioChunk = Inworld.Packets.AudioChunk;
using ControlEvent = Inworld.Packets.ControlEvent;
using GrpcPacket = Inworld.Grpc.InworldPacket;
using InworldPacket = Inworld.Packets.InworldPacket;
using Random = UnityEngine.Random;
using Routing = Inworld.Packets.Routing;


namespace Inworld
{
    public class InworldController : SingletonBehavior<InworldController>
    {
        #region Callbacks
        // Translate Runtime Events to public Controller Events.
        async void OnRuntimeEvents(RuntimeStatus status, string msg)
        {
            switch (status)
            {
                case RuntimeStatus.InitSuccess:
                    State = ControllerStates.Initialized;
                    if (m_Data != null)
                        await LoadScene(m_Data);
                    else
                    {
                        // YAN: Try load the first character in its children.
                        await LoadCharacter(GetFirstChild(true));
                    }
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
        string m_CurrentRecordingID;
        float m_BackOffTime = 0.2f;
        float m_CurrentCountDown;
        string m_WaitingRecordingID;
        string m_TTSInteractionID;
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
                if (capture)
                    capture.IsCapturing = value;
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

        /// <summary>
        ///     Get/Set the current Inworld Character. Could be Null.
        ///     Usually it's set by CheckPriority().
        /// </summary>
        public InworldCharacter CurrentCharacter
        {
            get => m_CurrentCharacter ? m_CurrentCharacter : Characters.Count > 0 ? Characters[0] : null;
            private set
            {
                if (m_CurrentCharacter != value)
                    OnCharacterChanged?.Invoke(m_CurrentCharacter, value);
                m_CurrentCharacter = value;
            }
        }
        /// <summary>
        ///     Get/Set all the Characters this Inworld Scene contains.
        /// </summary>
        public List<InworldCharacter> Characters { get; private set; }
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
            Characters ??= new List<InworldCharacter>();
            InworldAI.User.LoadData();
        }
        void Start()
        {
            if (m_AutoStart)
                Init();
        }
        void Update()
        {
            if (State == ControllerStates.LostConnect)
            {
                m_CurrentCountDown += Time.deltaTime;
                if (m_CurrentCountDown > m_BackOffTime)
                {
                    m_CurrentCountDown = 0;
                    _StartSession();
                }
            }
            if (string.IsNullOrEmpty(m_CurrentRecordingID) && !string.IsNullOrEmpty(m_WaitingRecordingID))
            {
                m_CurrentRecordingID = m_WaitingRecordingID;
                m_WaitingRecordingID = null;
                _StartAudioCapture(m_CurrentRecordingID);
            }

            if (Input.GetKeyUp(KeyCode.Escape))
                Application.Quit();
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

        #region Private Functions
        void _SetState(ControllerStates state)
        {
            if (m_State == state)
                return;
            m_State = state;
            OnStateChanged?.Invoke(state);
        }
        void _StartAudioCapture(string characterID)
        {
            m_CurrentRecordingID = characterID;
            m_Client.StartAudio(Routing.FromPlayerToAgent(characterID));
            m_Capture.IsCapturing = true; //Isenabled
            InworldAI.Log("Capture started.");
        }
        void _BindCharacterFromServer
        (
            InworldCharacter character,
            LoadSceneResponse.Types.Agent characterInfo
        )
        {
            if (Characters == null)
                Characters = new List<InworldCharacter>();
            Characters.Clear();
            Characters.Add(character);
            InworldCharacterData data = character.Data;
            if (!data)
                data = ScriptableObject.CreateInstance<InworldCharacterData>();
            data.characterID = characterInfo.AgentId;
            data.characterName = characterInfo.GivenName;
            data.brain = characterInfo.BrainName;
            data.modelUri = characterInfo.CharacterAssets.RpmModelUri;
            data.posUri = characterInfo.CharacterAssets.RpmImageUriPosture;
            data.previewImgUri = characterInfo.CharacterAssets.RpmImageUriPortrait;
            InworldAI.Log($"Register {character.CharacterName}: {characterInfo.AgentId}");
            character.LoadCharacter(data);
        }

        void _ListCharactersFromServer(List<LoadSceneResponse.Types.Agent> characters)
        {
            Dictionary<string, string> responseData = new Dictionary<string, string>();
            foreach (LoadSceneResponse.Types.Agent characterInfo in characters)
            {
                responseData[characterInfo.BrainName] = characterInfo.AgentId;
            }
            // Yan: They may return more Characters from server.
            //      But we only need the Characters in Unity Scene to register.
            if (characters.Count == 0)
            {
                InworldAI.LogError("Cannot Find Characters. Need Init first.");
                State = ControllerStates.Error;
                return;
            }
            foreach (InworldCharacter character in Characters.Where(character => responseData.ContainsKey(character.BrainName)))
            {
                InworldAI.Log($"Register {character.CharacterName}: {responseData[character.BrainName]}");
                character.RegisterLiveSession(responseData[character.BrainName]);
            }
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
            if (m_Client.GetAnimationChunk(out AnimationChunk animChunkEvent))
            {
                OnPacketReceived?.Invoke(animChunkEvent);
            }
        }
        void _SelectCharacter()
        {
            float fPriority = float.MaxValue;
            InworldCharacter targetCharacter = null;
            foreach (InworldCharacter iwChar in Characters.Where(iwChar => iwChar.Priority > 0 && iwChar.Priority < fPriority))
            {
                fPriority = iwChar.Priority;
                targetCharacter = iwChar;
            }
            CurrentCharacter = targetCharacter;
        }
        // Handling events coming from client.
        IEnumerator InteractionCoroutine()
        {
            while (State == ControllerStates.Connected)
            {
                _GetIncomingEvents();
                _SelectCharacter();
                // Client stopped.
                if (!m_Client.IsInteracting && !m_Client.Errors.IsEmpty)
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
            InworldAI.Log("InworldController Connected");
            StartCoroutine(InteractionCoroutine());
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
        public void Init()
        {
            State = ControllerStates.Initializing;
            m_Client.RuntimeEvent += OnRuntimeEvents;
            m_Client.GetAppAuth();
        }

        /// <summary>
        ///     Start session with target InworldCharacter directly
        ///     If success, it'll create a default InworldScene of that character,
        ///     with a valid SessionKey and character's AgentID.
        /// </summary>
        /// <param name="character">A GameObject or Prefab that has InworldCharacter Component</param>
        public async Task LoadCharacter(InworldCharacter character)
        {
            try
            {
                Debug.Log($"InworldController Connecting {character.BrainName}.");

                if (character == null || string.IsNullOrEmpty(character.BrainName))
                {
                    Debug.LogError("Please attach a correct Inworld Character.");
                    return;
                }
                Debug.Log($"Current State: {State}");
                switch (State)
                {
                    case ControllerStates.Initialized:
                    {
                        State = ControllerStates.Connecting;
                        LoadSceneResponse response = await m_Client.LoadScene(character.BrainName);
                        if (response != null && response.Agents.Count == 1)
                        {
                            _BindCharacterFromServer(character, response.Agents[0]);
                        }
                        _StartSession();
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
                Debug.LogError(e.ToString());
                //CurrentScene.Event.Invoke(InworldSceneStatus.LoadSceneFailed, null);
            }
        }
        /// <summary>
        ///     Start session with a InworldScene
        ///     If success, Set current InworldScene to this,
        ///     with a valid SessionKey and a list of its InworldCharacters with valid AgentID.
        /// </summary>
        /// <param name="inworldSceneData">The InworldScene to load</param>
        public async Task LoadScene(InworldSceneData inworldSceneData)
        {
            try
            {
                InworldAI.Log($"InworldController Connecting {inworldSceneData.fullName}.");

                if (inworldSceneData == null || string.IsNullOrEmpty(inworldSceneData.fullName))
                {
                    Debug.LogError("Please attach a correct Inworld Scene.");
                    return;
                }
                switch (State)
                {
                    case ControllerStates.Initialized:
                    {
                        State = ControllerStates.Connecting;
                        LoadSceneResponse response = await m_Client.LoadScene(inworldSceneData.fullName);
                        if (response != null)
                        {
                            _ListCharactersFromServer(response.Agents.ToList());
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
            foreach (InworldCharacter iwChar in Characters)
            {
                EndAudioCapture(iwChar.ID);
            }

            if (m_Client != null)
                await m_Client.EndSession();

            StopCoroutine(nameof(InteractionCoroutine));
            CurrentCharacter = null;
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
        public void StartAudioCapture(string characterID)
        {
            m_WaitingRecordingID = characterID;
        }

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
                    m_Capture.IsCapturing = false; //IsEnabled
                m_CurrentRecordingID = null;
                InworldAI.Log("Capture ended.");
            }
            else if (m_WaitingRecordingID == characterID)
                m_WaitingRecordingID = null;
        }
        public void RegisterCharacter(InworldCharacter character)
        {
            Characters ??= new List<InworldCharacter>();
            if (!Characters.Contains(character))
                Characters.Add(character);
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
    }
}

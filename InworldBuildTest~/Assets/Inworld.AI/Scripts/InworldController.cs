/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;
using Inworld.Packet;
using UnityEditor;

namespace Inworld
{
    [RequireComponent(typeof(InworldClient), typeof(AudioCapture), typeof(CharacterHandler))]
    public class InworldController : SingletonBehavior<InworldController>
    {
        [SerializeField] protected InworldClient m_Client;
        [SerializeField] protected AudioCapture m_AudioCapture;
        [SerializeField] protected CharacterHandler m_CharacterHandler;
        [SerializeField] protected InworldGameData m_GameData;
        [SerializeField] protected string m_SceneFullName;
        [Space(10)][SerializeField] protected bool m_AutoStart;
        
        protected string m_CurrentAudioID;
        public event Action<InworldPacket> OnCharacterInteraction;
        /// <summary>
        /// Gets the AudioCapture of the InworldController.
        /// </summary>
        public static AudioCapture Audio => Instance ? Instance.m_AudioCapture : null;
        /// <summary>
        /// Gets the CharacterHandler of the InworldController.
        /// </summary>
        public static CharacterHandler CharacterHandler => Instance ? Instance.m_CharacterHandler : null;
        /// <summary>
        /// Gets/Sets the current interacting character.
        /// </summary>
        public static InworldCharacter CurrentCharacter
        {
            get => Instance ? Instance.m_CharacterHandler ? Instance.m_CharacterHandler.CurrentCharacter : null : null;
            set
            {
                if (Instance && Instance.m_CharacterHandler)
                    Instance.m_CharacterHandler.CurrentCharacter = value;
            }
        }
        /// <summary>
        /// Gets/Sets this InworldController's protocol client.
        /// </summary>
        public static InworldClient Client
        {
            get => Instance ? Instance.m_Client : null;
            set
            {
                if (!Instance)
                    return;
                Instance.m_Client = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
                AssetDatabase.SaveAssets();
#endif
            }
        }
        /// <summary>
        /// Gets the current connection status.
        /// </summary>
        public static InworldConnectionStatus Status => Instance.m_Client.Status;
        /// <summary>
        /// Gets if Auto Start is enabled.
        /// </summary>
        public static bool IsAutoStart => Instance.m_AutoStart;
        /// <summary>
        /// Gets the current workspace's full name.
        /// </summary>
        public string CurrentWorkspace
        {
            get
            {
                string[] data = m_SceneFullName.Split(new[] { "/scenes/", "/characters/" }, StringSplitOptions.None);
                return data.Length > 1 ? data[0] : m_SceneFullName;
            }
        }
        /// <summary>
        /// Gets the current InworldScene's full name.
        /// </summary>
        public string CurrentScene => m_SceneFullName;
        /// <summary>
        /// Gets/Sets the InworldGameData of the InworldController.
        /// </summary>
        public InworldGameData GameData
        {
            get => m_GameData;
            set
            {
                m_GameData = value;
                #if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
                #endif
            }
        }
        /// <summary>
        /// Use the input json string of token instead of API key/secret to load scene.
        /// This token can be fetched by other applications such as InworldWebSDK.
        /// </summary>
        /// <param name="token">the custom token to init.</param>
        public void InitWithCustomToken(string token) => m_Client.InitWithCustomToken(token);
        /// <summary>
        /// Load InworldGameData. Set client's related data if game data is not null.
        /// </summary>
        /// <param name="gameData">the InworldGameData to load</param>
        public void LoadData(InworldGameData gameData)
        {
            if (!string.IsNullOrEmpty(gameData.apiKey))
                m_Client.APIKey = gameData.apiKey;
            if (!string.IsNullOrEmpty(gameData.apiSecret))
                m_Client.APISecret = gameData.apiSecret;
            if (!string.IsNullOrEmpty(gameData.sceneFullName))
                m_SceneFullName = gameData.sceneFullName;
            if (gameData.capabilities != null)
                InworldAI.Capabilities = gameData.capabilities;
        }
        /// <summary>
        /// Reconnect session or start a new session if the current session is invalid.
        /// </summary>
        public void Reconnect() => m_Client.Reconnect();
        /// <summary>
        /// Initializes the SDK.
        /// </summary>
        public void Init() => m_Client.GetAccessToken();
        /// <summary>
        /// Send LoadScene request to Inworld Server.
        /// </summary>
        /// <param name="sceneFullName">the full string of the scene to load.</param>
        public void LoadScene(string sceneFullName = "", string history = "")
        {
            string sceneToLoad = string.IsNullOrEmpty(sceneFullName) ? m_SceneFullName : sceneFullName;
            string historyToLoad = string.IsNullOrEmpty(history) ? Client.SessionHistory : history;
            m_Client.LoadScene(sceneToLoad, historyToLoad);
        }
        /// <summary>
        /// Disconnect Inworld Server.
        /// </summary>
        public void Disconnect()
        {
            m_Client.Disconnect();
        }
        /// <summary>
        /// Broadcast the received packets to characters.
        /// </summary>
        /// <param name="packet">The target InworldPacket to dispatch.</param>
        public void CharacterInteract(InworldPacket packet) => OnCharacterInteraction?.Invoke(packet);
        /// <summary>
        /// Send messages to an InworldCharacter in this current scene.
        /// </summary>
        /// <param name="charID">the live session ID of the character to send</param>
        /// <param name="text">the message to send.</param>
        public void SendText(string charID, string text)
        {
            if (Client.Status != InworldConnectionStatus.Connected)
                InworldAI.LogException($"Tried to send text to {charID}, but not connected to server.");
            m_Client.SendText(charID, text);
        }
        /// <summary>
        /// Send the CancelResponse Event to InworldServer to interrupt the character's speaking.
        /// </summary>
        /// <param name="charID">the live session ID of the character to send</param>
        /// <param name="interactionID">the handle of the dialog context that needs to be cancelled.</param>
        public void SendCancelEvent(string charID, string interactionID)
        {
            if (Client.Status != InworldConnectionStatus.Connected)
                InworldAI.LogException($"Tried to send cancel event to {charID}, but not connected to server.");
            m_Client.SendCancelEvent(charID, interactionID);
        } 
        /// <summary>
        /// Send the trigger to an InworldCharacter in the current scene.
        /// </summary>
        /// <param name="charID">the live session ID of the character to send.</param>
        /// <param name="triggerName">the name of the trigger to send.</param>
        /// <param name="parameters">the parameters and their values for the triggers.</param>
        public void SendTrigger(string triggerName, string charID, Dictionary<string, string> parameters = null)
        {
            if (Client.Status != InworldConnectionStatus.Connected)
                InworldAI.LogException($"Tried to send trigger to {charID}, but not connected to server.");
            if (string.IsNullOrEmpty(charID))
                throw new ArgumentException("Character ID is empty.");
            m_Client.SendTrigger(charID, triggerName, parameters);
        }
        /// <summary>
        /// Send AUDIO_SESSION_START control events to server.
        /// Without sending this message, all the audio data would be discarded by server.
        /// However, if you send this event twice in a row, without sending `StopAudio()`, Inworld server will also through exceptions and terminate the session.
        /// </summary>
        /// <param name="charID">the live session ID of the character to send.</param>
        /// <exception cref="ArgumentException">If the charID is not legal, this function will throw exception.</exception>
        public virtual void StartAudio(string charID = "")
        {
            if (Client.Status != InworldConnectionStatus.Connected)
                InworldAI.LogException($"Tried to start audio for {charID}, but not connected to server.");
            if (string.IsNullOrEmpty(charID))
            {
                if (CurrentCharacter && !string.IsNullOrEmpty(CurrentCharacter.ID))
                    charID = CurrentCharacter.ID;
                else
                    throw new ArgumentException("Character ID is empty.");
            }
            if (m_CurrentAudioID == charID)
                return;
            if (InworldAI.IsDebugMode)
                InworldAI.Log($"Start Audio Event {charID}");
            if (!m_CharacterHandler.IsRegistered(charID))
                return;
            
            m_CurrentAudioID = charID;
            m_AudioCapture.StartRecording();
            m_Client.StartAudio(charID);
        }
        /// <summary>
        /// Send AUDIO_SESSION_END control events to the current character.
        /// </summary>
        public virtual void StopAudio()
        {
            if (CurrentCharacter)
                StopAudio(CurrentCharacter.ID);
        }
        /// <summary>
        /// Send AUDIO_SESSION_END control events to server.
        /// </summary>
        /// <param name="charID">the live session ID of the character to send.</param>
        public virtual void StopAudio(string charID)
        {
            if (string.IsNullOrEmpty(charID))
                throw new ArgumentException("Character ID is empty.");
            if (m_CurrentAudioID != charID)
                return;
            if (InworldAI.IsDebugMode)
                InworldAI.Log($"Stop Audio Event {charID}");
            
            ResetAudio();
            
            if (!m_CharacterHandler.IsRegistered(charID) || Client.Status != InworldConnectionStatus.Connected)
                return;
            m_Client.StopAudio(charID);
        }
        /// <summary>
        /// Send the wav data to the current character.
        /// Need to make sure that AUDIO_SESSION_START control event has been sent to server.
        ///
        /// Only the base64 string of the wave data is supported by Inworld server.
        /// Additionally, the sample rate of the wave data has to be 16000, mono channel.
        /// </summary>
        /// <param name="base64">the base64 string of the wave data to send.</param>
        public virtual void SendAudio(string base64)
        {
            if (string.IsNullOrEmpty(m_CurrentAudioID) || !m_CharacterHandler.IsRegistered(m_CurrentAudioID) || !Audio.IsAudioAvailable)
                return;
            m_Client.SendAudio(m_CurrentAudioID, base64);
        }
        /// <summary>
        /// Manually push the audio wave data to server.
        /// </summary>
        public void PushAudio()
        {
            if (Client.Status != InworldConnectionStatus.Connected)
                InworldAI.LogException($"Tried to push audio, but not connected to server.");
            m_AudioCapture.PushAudio();
            StopAudio();
        }
        protected virtual void ResetAudio()
        {
            if (InworldAI.IsDebugMode)
                InworldAI.Log($"Audio Reset");
            m_AudioCapture.StopRecording();
            m_CurrentAudioID = null;
        }
        

        protected virtual void Awake()
        {
            if (!m_Client)
                m_Client = GetComponent<InworldClient>();
            if(!m_AudioCapture)
                m_AudioCapture = GetComponent<AudioCapture>();
            if(!m_CharacterHandler)
                m_CharacterHandler = GetComponent<CharacterHandler>();
        }
        protected virtual void OnEnable()
        {
            _Setup();
        }
        protected virtual void OnDisable()
        {
            m_Client.OnStatusChanged -= OnStatusChanged;
        }
        protected virtual void Start()
        {
            if (m_GameData)
                LoadData(m_GameData);
            if (m_AutoStart)
                Init();
        }
        protected void _Setup()
        {
            if (!m_Client)
                m_Client = GetComponent<InworldClient>();
            m_Client.OnStatusChanged += OnStatusChanged;
        }
        protected virtual void OnStatusChanged(InworldConnectionStatus incomingStatus)
        {
            switch (incomingStatus)
            {
                case InworldConnectionStatus.Initialized:
                    if (m_AutoStart)
                        LoadScene(m_SceneFullName);
                    break;
                case InworldConnectionStatus.LoadingSceneCompleted:
                    _StartSession();
                    break;
                case InworldConnectionStatus.LostConnect:
                    ResetAudio();
                    if (m_AutoStart)
                        Reconnect();
                    break;
                case InworldConnectionStatus.Error:
                case InworldConnectionStatus.Idle:
                    ResetAudio();
                    break;
            }
        }

        protected void _StartSession() => m_Client.StartSession();
    }
}

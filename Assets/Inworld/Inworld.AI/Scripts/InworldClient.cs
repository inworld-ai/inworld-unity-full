/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Packet;
using Inworld.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inworld
{
    public class InworldClient : MonoBehaviour
    {
        [SerializeField] protected InworldServerConfig m_ServerConfig;
        [SerializeField] protected string m_APIKey;
        [SerializeField] protected string m_APISecret;
        [SerializeField] protected string m_CustomToken;
        [SerializeField] protected string m_PublicWorkspace;
        public event Action<InworldConnectionStatus> OnStatusChanged;
        internal event Action<InworldPacket> OnPacketReceived;
        const string k_NotImplented = "No InworldClient found. Need at least one connection protocol";
        protected Token m_Token;
        protected string m_SessionKey;
        InworldConnectionStatus m_Status;
        protected string m_Error;
        
        /// <summary>
        /// Get/Set the session history.
        /// Session History is a string
        /// </summary>
        public string SessionHistory { get; set; }
        /// <summary>
        /// Gets/Sets the current Inworld server this client is connecting.
        /// </summary>
        public InworldServerConfig Server
        {
            get => m_ServerConfig;
            internal set => m_ServerConfig = value;
        }
        /// <summary>
        /// Gets/Sets the token used to login Runtime server of Inworld.
        /// </summary>
        public Token Token
        {
            get => m_Token;
            set => m_Token = value;
        }
        /// <summary>
        /// Gets if the current token is valid.
        /// </summary>
        public virtual bool IsTokenValid => m_Token != null && m_Token.IsValid;
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
        /// If set, it'll also set the status of this client.
        /// </summary>
        public virtual string Error
        {
            get => m_Error;
            protected set
            {
                Status = InworldConnectionStatus.Error;
                m_Error = value;
                InworldAI.LogError(m_Error);
            }
        }
        public virtual void GetHistoryAsync(string sceneFullName) => Error = k_NotImplented;
        /// <summary>
        /// Gets the access token. Would be implemented by child class.
        /// </summary>
        public virtual void GetAccessToken()
        {
            Error = k_NotImplented;
        }
        /// <summary>
        /// Reconnect session or start a new session if the current session is invalid.
        /// </summary>
        public void Reconnect() 
        {
            if (IsTokenValid)
                StartSession();
            else
                GetAccessToken();
        }
        /// <summary>
        /// Gets the live session info once load scene completed.
        /// The returned LoadSceneResponse contains the session ID and all the live session ID for each InworldCharacters in this InworldScene.
        /// </summary>
        public virtual LoadSceneResponse GetLiveSessionInfo()
        {
            Error = k_NotImplented;
            return null;
        }
        /// <summary>
        /// Use the input json string of token instead of API key/secret to load scene.
        /// This token can be fetched by other applications such as InworldWebSDK.
        /// </summary>
        /// <param name="token">the custom token to init.</param>
        public virtual bool InitWithCustomToken(string token)
        {
            m_Token = JsonUtility.FromJson<Token>(token);
            if (!IsTokenValid)
            {
                Error = "Get Token Failed";
                return false;
            }
            Status = InworldConnectionStatus.Initialized;
            return true;
        }
        /// <summary>
        /// Start the session by the session ID.
        /// </summary>
        public virtual void StartSession() => Error = k_NotImplented;
        /// <summary>
        /// Disconnect Inworld Server.
        /// </summary>
        public virtual void Disconnect() => Error = k_NotImplented;
        /// <summary>
        /// Send LoadScene request to Inworld Server.
        /// </summary>
        /// <param name="sceneFullName">the full string of the scene to load.</param>
        /// <param name="history">the full string of the encrypted history content to send.</param>
        public virtual void LoadScene(string sceneFullName, string history = "") => Error = k_NotImplented;
        /// <summary>
        /// Send messages to an InworldCharacter in this current scene.
        /// </summary>
        /// <param name="characterID">the live session ID of the character to send</param>
        /// <param name="textToSend">the message to send.</param>
        public virtual void SendText(string characterID, string textToSend) => Error = k_NotImplented;
        /// <summary>
        /// Send the CancelResponse Event to InworldServer to interrupt the character's speaking.
        /// </summary>
        /// <param name="characterID">the live session ID of the character to send</param>
        /// <param name="interactionID">the handle of the dialog context that needs to be cancelled.</param>
        public virtual void SendCancelEvent(string characterID, string interactionID) => Error = k_NotImplented;
        /// <summary>
        /// Send the trigger to an InworldCharacter in the current scene.
        /// </summary>
        /// <param name="charID">the live session ID of the character to send.</param>
        /// <param name="triggerName">the name of the trigger to send.</param>
        /// <param name="parameters">the parameters and their values for the triggers.</param>
        public virtual void SendTrigger(string charID, string triggerName, Dictionary<string, string> parameters) => Error = k_NotImplented;
        /// <summary>
        /// Send AUDIO_SESSION_START control events to server.
        /// Without sending this message, all the audio data would be discarded by server.
        /// However, if you send this event twice in a row, without sending `StopAudio()`, Inworld server will also through exceptions and terminate the session.
        /// </summary>
        /// <param name="charID">the live session ID of the character to send.</param>
        public virtual void StartAudio(string charID) => Error = k_NotImplented;
        /// <summary>
        /// Send AUDIO_SESSION_END control events to server.
        /// </summary>
        /// <param name="charID">the live session ID of the character to send.</param>
        public virtual void StopAudio(string charID) => Error = k_NotImplented;
        /// <summary>
        /// Send the wav data to server.
        /// Need to make sure that AUDIO_SESSION_START control event has been sent to server.
        ///
        /// Only the base64 string of the wave data is supported by Inworld server.
        /// Additionally, the sample rate of the wave data has to be 16000, mono channel.
        /// </summary>
        /// <param name="charID">the live session ID of the character to send.</param>
        /// <param name="base64">the base64 string of the wave data to send.</param>
        public virtual void SendAudio(string charID, string base64) => Error = k_NotImplented;
        /// <summary>
        /// Change the current status of the Inworld client.
        /// </summary>
        /// <param name="status">the new status to change.</param>
        public void ChangeStatus(InworldConnectionStatus status) => OnStatusChanged?.Invoke(status);
        /// <summary>
        /// Dispatch the packet to Inworld server.
        /// </summary>
        /// <param name="packet">the packet to send.</param>
        public void Dispatch(InworldPacket packet) => OnPacketReceived?.Invoke(packet);
        /// <summary>
        /// Copy the filling data from another Inworld client.
        /// </summary>
        /// <param name="rhs">the Inworld client's data to copy.</param>
        public void CopyFrom(InworldClient rhs)
        {
            Server = rhs.Server;
            APISecret = rhs.APISecret;
            APIKey = rhs.APIKey;
            CustomToken = rhs.CustomToken;
            InworldController.Client = this;
        }
        protected virtual void Init() {}

        internal string APIKey
        {
            get => m_APIKey;
            set => m_APIKey = value;
        }
        internal string APISecret
        {
            get => m_APISecret;
            set => m_APISecret = value;
        }
        internal string CustomToken
        {
            get => m_CustomToken;
            set => m_CustomToken = value;
        }

        void Awake()
        {
            Init();
        }
        void OnEnable()
        {
            Init();
        }
    }
}

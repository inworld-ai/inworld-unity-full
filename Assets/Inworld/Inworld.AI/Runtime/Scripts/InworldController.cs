/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Entities;
using Inworld.Audio;
using Inworld.LLM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

namespace Inworld
{
    /// <summary>
    /// The InworldController acts as an API hub within this Unity application, primarily designed for backward compatibility with previous versions.
    /// It serves as a central point for managing API interfaces, ensuring that the system maintains its interfaces with older versions
    /// while delegating the actual execution of each API call to subordinate scripts.
    /// </summary>

    public class InworldController : SingletonBehavior<InworldController>
    {
#region Inspector Variables
        [Header("NOTE: Using game data will overwrite manual input API key/secrets.")][Space(10)]
        [SerializeField] protected InworldGameData m_GameData;
        [SerializeField] protected string m_APIKey;
        [SerializeField] protected string m_APISecret;
        [SerializeField] protected InworldServerConfig m_ServerConfig;
        [Space(10)][Header("Advanced:")]
        [SerializeField] protected string m_CustomToken;
        [SerializeField] protected string m_PublicWorkspace;
        [SerializeField] protected string m_GameSessionID;
        
        protected Token m_Token;
        protected InworldClient m_Client;
        protected InworldAudioManager m_AudioManager;
        protected CharacterHandler m_CharacterHandler;
        protected LLMRuntime m_LLMRuntime;
#endregion

#region Properties
        public static bool HasError => Client && (Client.Error?.IsValid ?? false);
        /// <summary>
        /// Gets the AudioCapture of the InworldController.
        /// </summary>
        public static InworldAudioManager Audio
        {
            get
            {
                if (!Instance) 
                    return null;
                if (Instance.m_AudioManager)
                    return Instance.m_AudioManager;
                Instance.m_AudioManager = Instance.GetComponentInChildren<InworldAudioManager>();
                return Instance.m_AudioManager;
            }
        }
        /// <summary>
        /// Gets the CharacterHandler of the InworldController.
        /// </summary>
        public static CharacterHandler CharacterHandler
        {
            get
            {
                if (!Instance)
                    return null;
                if (Instance.m_CharacterHandler)
                    return Instance.m_CharacterHandler;
                Instance.m_CharacterHandler = Instance.GetComponentInChildren<CharacterHandler>();
                return Instance.m_CharacterHandler;
            }
        }
        /// <summary>
        /// Gets/Sets this InworldController's protocol client.
        /// </summary>
        public static InworldClient Client
        {
            get
            {
                if (!Instance) 
                    return null;

                if (Instance.m_Client)
                    return Instance.m_Client;

                Instance.m_Client = Instance.GetComponentInChildren<InworldClient>();
                return Instance.m_Client;
            }
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
        /// Gets/Sets this InworldController's protocol client.
        /// </summary>
        public static LLMRuntime LLM
        {
            get
            {
                if (!Instance) 
                    return null;

                if (Instance.m_LLMRuntime)
                    return Instance.m_LLMRuntime;

                Instance.m_LLMRuntime = Instance.GetComponentInChildren<LLMRuntime>();
                return Instance.m_LLMRuntime;
            }
            set
            {
                if (!Instance)
                    return;
                Instance.m_LLMRuntime = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
                AssetDatabase.SaveAssets();
#endif
            }
        }
        /// <summary>
        /// Gets/Sets the current GameSessionID used for dialog continuation.
        /// </summary>
        public static string GameSessionID
        {
            get
            {
                return !Instance ? null : Instance.GetGameSessionID();
            }
            internal set
            {
                if (!Instance)
                    return;
                Instance.m_GameSessionID = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
                AssetDatabase.SaveAssets();
#endif
            }
        }
        /// <summary>
        /// Gets/Sets the current Inworld server this client is connecting.
        /// </summary>
        public static InworldServerConfig Server
        {
            get
            {
                if (Instance)
                    return Instance.m_ServerConfig;
                return null;
            }
            internal set
            {
                if (!Instance)
                    return;
                Instance.m_ServerConfig = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
                AssetDatabase.SaveAssets();
#endif
            }
        }
        /// <summary>
        /// Gets/Sets the token used to login Runtime server of Inworld.
        /// </summary>
        public static string Token
        {
            get
            {
                if (Instance && Instance.m_Token != null)
                    return Instance.m_Token.token;
                return null;
            }
        }
        /// <summary>
        /// Gets if the current token is valid.
        /// </summary>
        public static bool IsTokenValid
        {
            get
            {
                if (Instance)
                    return Instance.m_Token != null && Instance.m_Token.IsValid;
                return false;
            }
        }
        /// <summary>
        /// Gets the current connection InworldClient's status.
        /// Used for backwards compatibility.
        /// </summary>
        public static InworldConnectionStatus Status => Client ? Client.Status : 
            IsTokenValid ? InworldConnectionStatus.Initialized : InworldConnectionStatus.Idle;
        /// <summary>
        /// Gets the current InworldScene's full name.
        /// </summary>
        public string CurrentScene => Client ? Client.CurrentScene : "";
        /// <summary>
        /// Gets/Sets the current interacting character.
        /// </summary>
        public static InworldCharacter CurrentCharacter
        {
            get
            {
                if (CharacterHandler && CharacterHandler.CurrentCharacter)
                    return CharacterHandler.CurrentCharacter;
                return null;
            }

            set
            {
                if (!CharacterHandler)
                    return;
                if (!CharacterHandler.CurrentCharacters.Contains(value))
                    CharacterHandler.Register(value);
                CharacterHandler.CurrentCharacter = value;
            }
        }
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
        /// Gets/Sets the custom Token generated by client's own method.
        /// </summary>
        public string CustomToken
        {
            get => m_CustomToken;
            set => m_CustomToken = value;
        }
        /// <summary>
        /// Gets the WebSocket start session URL.
        /// </summary>
        public static string WebsocketSessionURL
        {
            get
            {
                if (!Server || !IsTokenValid)
                    return null;
                return Server.SessionURL(SessionID);
            }
        }
        /// <summary>
        /// Gets the current session ID.
        /// </summary>
        public static string SessionID
        {
            get
            {
                if (!IsTokenValid)
                    return null;
                return Instance.m_Token.sessionId;
            }
        }
        /// <summary>
        /// Gets the token type responsed from Inworld server.
        /// </summary>
        public static string TokenType
        {
            get
            {
                if (!IsTokenValid)
                    return null;
                return Instance.m_Token.type;
            }
        }
#endregion

#region Event
        public event Action<InworldConnectionStatus, string> OnControllerStatusChanged;
#endregion
        
#region Connection Management
        /// <summary>
        /// Gets the access token. Would be implemented by child class.
        /// </summary>
        public virtual void GetAccessToken() => StartCoroutine(_GetAccessToken(m_PublicWorkspace));
        /// <summary>
        /// Initializes the SDK.
        /// </summary>
        public void Init() => GetAccessToken();
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
                OnControllerStatusChanged?.Invoke(InworldConnectionStatus.Error, "Get Token Failed");
                return false;
            }
            OnControllerStatusChanged?.Invoke(InworldConnectionStatus.Initialized, "Initialized");
            return true;
        }
        /// <summary>
        /// Load InworldGameData. Set client's related data if game data is not null.
        /// </summary>
        /// <param name="gameData">the InworldGameData to load</param>
        public bool LoadData(InworldGameData gameData)
        {
            if (gameData == null)
                return false;
            if (!string.IsNullOrEmpty(gameData.apiKey))
                m_APIKey = gameData.apiKey;
            if (!string.IsNullOrEmpty(gameData.apiSecret))
                m_APISecret = gameData.apiSecret;
            if (!string.IsNullOrEmpty(gameData.sceneName) && m_Client)
                m_Client.SceneFullName = InworldAI.GetSceneFullName(gameData.workspaceName, gameData.sceneName);
            if (gameData.capabilities != null)
                InworldAI.Capabilities = gameData.capabilities;
            return true;
        }
#endregion

#region Client 
        /// <summary>
        /// Gets the Load Session URL
        /// </summary>
        /// <param name="sceneFullName"></param>
        /// <returns></returns>
        public string LoadSessionURL(string sceneFullName)
        {
            return m_ServerConfig.LoadSessionStateURL(GetSessionFullName(sceneFullName));
        }
        /// <summary>
        /// Gets the SessionName.
        /// TODO(Yan): Decouple with scene's full Name, so that it can be used generally.
        /// </summary>
        /// <param name="sceneFullName">the current scene name.</param>
        /// <returns></returns>
        public string GetSessionFullName(string sceneFullName)
        {
            string[] data = sceneFullName.Split('/');
            return data.Length != 4 ? "" : $"workspaces/{data[1]}/sessions/{m_Token.sessionId}";
        }
        /// <summary>
        /// Reconnect session or start a new session if the current session is invalid.
        /// </summary>
        public void Reconnect() => m_Client.Reconnect();
        /// <summary>
        /// Send LoadScene request to Inworld Server.
        /// In ver 3.3 or further, the session must be connected first.
        /// </summary>
        /// <param name="sceneFullName">the full string of the scene to load.</param>
        public bool LoadScene(string sceneFullName = "")
        {
            InworldAI.LogEvent("Login_Runtime");
            string sceneToLoad = string.IsNullOrEmpty(sceneFullName) ? m_Client.CurrentScene : sceneFullName;
            return m_Client.LoadScene(sceneToLoad);
        }
        /// <summary>
        /// Sent after the ClientStatus is set to Connected.
        /// Sequentially sends Session Config parameters, enabling this session to become interactive.
        /// </summary>
        public void PrepareSession() => StartCoroutine(Client.PrepareSession());
        /// <summary>
        /// Disconnect Inworld Server.
        /// </summary>
        public void Disconnect()
        {
            CancelResponses();
            m_Client.Disconnect();
        }
        /// <summary>
        /// Send messages to an InworldCharacter in this current scene.
        /// If there's a current character, it'll be sent to the specific character,
        /// otherwise, it'll be sent as broadcast.
        /// </summary>
        /// <param name="text">the message to send.</param>
        /// <param name="interruptible">if this text will interrupt the current character, by default is true.</param>
        public bool SendText(string text, bool interruptible = true)
        {
            if (!m_Client)
                return false;
            if (interruptible)
                CancelResponses();
            return m_Client.SendTextTo(text, CharacterHandler.CurrentCharacter? CharacterHandler.CurrentCharacter.BrainName : "");
        }
        /// <summary>
        /// Send Text to LLM Service
        /// </summary>
        /// <param name="text">the text to send</param>
        public bool SendLLMText(string text)
        {
            return IsTokenValid && m_LLMRuntime && m_LLMRuntime.SendText(text);
        }
        /// <summary>
        /// Send a narrative action to an InworldCharacter in this current scene.
        /// </summary>
        /// <param name="charID">the live session ID of the character to send</param>
        /// <param name="narrativeAction">the narrative action to send.</param>
        /// <param name="interruptible">if this text will interrupt the current character, by default is true.</param>
        public bool SendNarrativeAction(string narrativeAction, bool interruptible = true)
        {
            if (!m_Client)
                return false;
            if (interruptible)
                CancelResponses();
            return m_Client.SendNarrativeActionTo(narrativeAction, CharacterHandler.CurrentCharacter? CharacterHandler.CurrentCharacter.BrainName : "");
        }
        /// <summary>
        /// Cancel all the current character's generating responses.
        /// Automatically used when sending Text.
        /// For other sending data, such as sending trigger, please consider to use according to your scenario.
        /// </summary>
        public void CancelResponses()
        {
            if (CharacterHandler.CurrentCharacters.Count > 0)
                CharacterHandler.CurrentCharacters.ForEach(c => c.CancelResponse());
        }
        /// <summary>
        /// Send the CancelResponse Event to InworldServer to interrupt the character's speaking.
        /// </summary>
        /// <param name="interactionID">the handle of the dialog context that needs to be cancelled.</param>
        /// <param name="utteranceID">the handle of the current utterance that needs to be cancelled.</param>
        public bool SendCancelEvent(string interactionID, string utteranceID = "")
        {
            return m_Client.SendCancelEventTo(interactionID, utteranceID, SourceType.WORLD.ToString());
        } 
        /// <summary>
        /// Send the trigger to the whole session.
        /// </summary>
        /// <param name="triggerName">the name of the trigger to send.</param>
        public bool SendWorldTrigger(string triggerName)
        {
            if (Client && Client.Status == InworldConnectionStatus.Connected)
                return m_Client.SendTriggerTo(triggerName, null,SourceType.WORLD.ToString());
            return false;
        }
        /// <summary>
        /// Legacy Send the trigger to an InworldCharacter in the current scene.
        /// </summary>
        /// <param name="charID">the live session ID of the character to send. Send to World if it's empty</param>
        /// <param name="triggerName">the name of the trigger to send.</param>
        /// <param name="parameters">the parameters and their values for the triggers.</param>
        public bool SendTrigger(string triggerName, string charID = "", Dictionary<string, string> parameters = null)
        {
            if (!Client || Client.Status != InworldConnectionStatus.Connected)
            {
                InworldAI.LogError($"Tried to send trigger to {charID}, but not connected to server.");
                return false;
            }
            if (!string.IsNullOrEmpty(charID))
                return m_Client.SendTrigger(charID, triggerName, parameters);
            return CurrentCharacter 
                ? m_Client.SendTriggerTo(triggerName, parameters, CurrentCharacter.BrainName) 
                : m_Client.SendTriggerTo(triggerName, parameters);
        }
        /// <summary>
        /// Send AUDIO_SESSION_START control events to server.
        /// Without sending this message, all the audio data would be discarded by server.
        /// However, if you send this event twice in a row, without sending `StopAudio()`, Inworld server will also through exceptions and terminate the session.
        /// </summary>
        public virtual bool StartAudio()
        {
            if (!Client || Client.Status != InworldConnectionStatus.Connected)
            {
                InworldAI.LogError("Tried to start audio, but not connected to server.");
                return false;
            }
            if (CharacterHandler.CurrentCharacterNames.Count > 0)
                return Audio.StartMicrophone();
            InworldAI.LogError("No characters in the session.");
            return false;
        }
        /// <summary>
        /// Send AUDIO_SESSION_END control events to server.
        /// </summary>
        public virtual bool StopAudio()
        {
            return Audio && Audio.StopMicrophone();
        }
        /// <summary>
        /// Send the wav data to the current character.
        /// Need to make sure that AUDIO_SESSION_START control event has been sent to server.
        ///
        /// Only the base64 string of the wave data is supported by Inworld server.
        /// Additionally, the sample rate of the wave data has to be 16000, mono channel.
        /// </summary>
        /// <param name="base64">the base64 string of the wave data to send.</param>
        public virtual bool SendAudio(string base64)
        {
            if (!Audio)
                return false;
            if (CurrentCharacter && !string.IsNullOrEmpty(CurrentCharacter.ID))
                return m_Client.SendAudio(CurrentCharacter.ID, base64);
            return m_Client.SendAudioTo(base64);
        }
        /// <summary>
        /// Manually push the audio wave data to server.
        /// </summary>
        public void PushAudio()
        {
            if (Client.Status != InworldConnectionStatus.Connected)
                InworldAI.LogException($"Tried to push audio, but not connected to server.");
            StartCoroutine(m_AudioManager.PushAudio());
            StopAudio();
        }
#endregion

        protected virtual void OnEnable()
        {
            InworldAI.InputActions.Enable();
        }
        protected virtual void OnDisable()
        {
            InworldAI.InputActions.Disable();
        }
        protected virtual void Start() => LoadData(m_GameData);
        
        protected string GetGameSessionID()
        {
            if (string.IsNullOrEmpty(m_GameSessionID) && m_Token != null && !string.IsNullOrEmpty(m_Token.sessionId))
                m_GameSessionID = m_Token.sessionId;
            return m_GameSessionID;
        }
        protected IEnumerator _GetAccessToken(string workspaceFullName = "")
        {
            string responseJson = m_CustomToken;
            if (string.IsNullOrEmpty(responseJson))
            {
                if (string.IsNullOrEmpty(m_APIKey))
                {
                    OnControllerStatusChanged?.Invoke(InworldConnectionStatus.Error, "Please fill API Key!");
                    yield break;
                }
                if (string.IsNullOrEmpty(m_APISecret))
                {
                    OnControllerStatusChanged?.Invoke(InworldConnectionStatus.Error, "Please fill API Secret!");
                    yield break;
                }
                string header = InworldAuth.GetHeader(m_ServerConfig.runtime, m_APIKey, m_APISecret);
                UnityWebRequest uwr = new UnityWebRequest(m_ServerConfig.TokenServer, "POST");
                uwr.SetRequestHeader("Authorization", header);
                uwr.SetRequestHeader("Content-Type", "application/json");
                AccessTokenRequest req = new AccessTokenRequest
                {
                    api_key = m_APIKey,
                    resource_id = workspaceFullName
                };
                string json = JsonUtility.ToJson(req);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
                uwr.downloadHandler = new DownloadHandlerBuffer();
                yield return uwr.SendWebRequest();
                if (uwr.result != UnityWebRequest.Result.Success)
                    OnControllerStatusChanged?.Invoke(InworldConnectionStatus.Error, $"Error Get Token: {uwr.error}");
                uwr.uploadHandler.Dispose();
                responseJson = uwr.downloadHandler.text;
            }
            m_Token = JsonUtility.FromJson<Token>(responseJson);
            if (!IsTokenValid)
            {
                OnControllerStatusChanged?.Invoke(InworldConnectionStatus.Error, "Get Token Failed");
                yield break;
            }
            OnControllerStatusChanged?.Invoke(InworldConnectionStatus.Initialized, "Initialized");
        }
    }
}

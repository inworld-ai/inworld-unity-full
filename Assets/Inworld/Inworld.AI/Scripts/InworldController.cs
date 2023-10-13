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

        public static AudioCapture Audio => Instance ? Instance.m_AudioCapture : null;
        public static CharacterHandler CharacterHandler => Instance ? Instance.m_CharacterHandler : null;
        public static InworldCharacter CurrentCharacter
        {
            get => Instance ? Instance.m_CharacterHandler ? Instance.m_CharacterHandler.CurrentCharacter : null : null;
            set
            {
                if (Instance && Instance.m_CharacterHandler)
                    Instance.m_CharacterHandler.CurrentCharacter = value;
            }
        }
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
        public static InworldConnectionStatus Status => Instance.m_Client.Status;
        public static bool IsAutoStart => Instance.m_AutoStart;
        public void InitWithCustomToken(string token) => m_Client.InitWithCustomToken(token);
        public string CurrentWorkspace
        {
            get
            {
                string[] data = m_SceneFullName.Split(new[] { "/scenes/", "/characters/" }, StringSplitOptions.None);
                return data.Length > 1 ? data[0] : m_SceneFullName;
            }
        }
        public string CurrentScene => m_SceneFullName;
        public event Action<InworldPacket> OnCharacterInteraction;

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
        public void Reconnect() => m_Client.Reconnect();
        public void Init() => m_Client.GetAccessToken();
        public void LoadScene(string sceneFullName = "") => m_Client.LoadScene(string.IsNullOrEmpty(sceneFullName) ? m_SceneFullName : sceneFullName);
        public void Disconnect()
        {
            m_Client.Disconnect();
        }
        public void CharacterInteract(InworldPacket packet) => OnCharacterInteraction?.Invoke(packet);

        public void SendText(string charID, string text)
        {
            if (Client.Status != InworldConnectionStatus.Connected)
                InworldAI.LogException($"Tried to send text to {charID}, but not connected to server.");
            m_Client.SendText(charID, text);
        }
        public void SendCancelEvent(string charID, string interactionID)
        {
            if (Client.Status != InworldConnectionStatus.Connected)
                InworldAI.LogException($"Tried to send cancel event to {charID}, but not connected to server.");
            m_Client.SendCancelEvent(charID, interactionID);
        } 
        public void SendTrigger(string triggerName, string charID, Dictionary<string, string> parameters = null)
        {
            if (Client.Status != InworldConnectionStatus.Connected)
                InworldAI.LogException($"Tried to send trigger to {charID}, but not connected to server.");
            if (string.IsNullOrEmpty(charID))
                throw new ArgumentException("Character ID is empty.");
            m_Client.SendTrigger(charID, triggerName, parameters);
        }
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
        public virtual void StopAudio()
        {
            if (CurrentCharacter)
                StopAudio(CurrentCharacter.ID);
        }

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

        public virtual void SendAudio(string base64)
        {
            if (string.IsNullOrEmpty(m_CurrentAudioID) || !m_CharacterHandler.IsRegistered(m_CurrentAudioID))
                return;
            m_Client.SendAudio(m_CurrentAudioID, base64);
        }
        protected virtual void ResetAudio()
        {
            if (InworldAI.IsDebugMode)
                InworldAI.Log($"Audio Reset");
            m_AudioCapture.StopRecording();
            m_CurrentAudioID = null;
        }
        
        public void PushAudio()
        {
            if (Client.Status != InworldConnectionStatus.Connected)
                InworldAI.LogException($"Tried to push audio, but not connected to server.");
            m_AudioCapture.PushAudio();
            StopAudio();
        }

        protected void _Setup()
        {
            m_Client ??= GetComponent<InworldClient>();
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Inworld.Packet;

namespace Inworld
{
    [RequireComponent(typeof(InworldClient))]
    public class InworldController : SingletonBehavior<InworldController>
    {
        [SerializeField] InworldServerConfig m_ServerConfig;
        [SerializeField] string m_SceneFullName;
        [SerializeField] string m_PlayerName;
        [SerializeField] Texture2D m_DefaultThumbnail;
        [SerializeField] bool m_AutoStart;
        [SerializeField] InworldClient m_Client;
        // YAN: Now LiveSessionID is handled by InworldController Only. To prevent unable to chat.
        //      Both Keys are BrainNames
        readonly Dictionary<string, string> m_LiveSession = new Dictionary<string, string>();
        readonly Dictionary<string, InworldCharacter> m_Characters = new Dictionary<string, InworldCharacter>();

        InworldCharacter m_CurrentCharacter;
        InworldCharacter m_LastCharacter;

        public static InworldClient Client => Instance.m_Client;
        public static InworldConnectionStatus Status => Instance.m_Client.Status;
        public InworldCharacter CurrentCharacter
        {
            get => m_CurrentCharacter;
            set
            {
                string oldBrainName = m_CurrentCharacter ? m_CurrentCharacter.BrainName : "";
                string newBrainName = value ? value.BrainName : "";
                if (oldBrainName == newBrainName)
                    return;
                m_LastCharacter = m_CurrentCharacter;
                m_CurrentCharacter = value;
                OnCharacterChanged?.Invoke(m_LastCharacter, m_CurrentCharacter);
            }
        }
        public event Action<InworldCharacterData> OnCharacterRegistered;
        public event Action<InworldCharacter, InworldCharacter> OnCharacterChanged;
        public event Action<InworldPacket> OnCharacterInteraction;

        void Awake()
        {
            m_Client = GetComponent<InworldClient>();
        }
        void OnEnable()
        {
            _Setup();
        }
        void OnDisable()
        {
            m_Client.OnStatusChanged -= OnStatusChanged;
            if (m_CurrentCharacter != null)
                StopAudio(m_CurrentCharacter.ID);
        }
        void Start()
        {
            if (m_AutoStart)
                Init();
        }
        public void SwitchConnect()
        {
            if (m_Client.Status == InworldConnectionStatus.Idle)
                Reconnect();
            else if (m_Client.Status == InworldConnectionStatus.Connected)
                Disconnect();
        }
        public void Reconnect() => m_Client.Reconnect();
        public void Init() => m_Client.GetAccessToken();
        public void LoadScene(string sceneFullName) => m_Client.LoadScene(sceneFullName);
        public void Disconnect()
        {
            m_Client.Disconnect();
            CurrentCharacter = null;
        }
        public void CharacterInteract(InworldPacket packet) => OnCharacterInteraction?.Invoke(packet);
        public string GetLiveSessionID(string brainName) => m_LiveSession.ContainsKey(brainName) ? m_LiveSession[brainName] : "";
        public bool IsRegistered(string characterID) => !string.IsNullOrEmpty(characterID) && m_LiveSession.ContainsKey(characterID);
        public InworldCharacter GetCharacter(string agentID)
        {
            if (!m_LiveSession.ContainsValue(agentID))
            {
                InworldAI.LogError($"{agentID} Not Registered!");
                return null;
            }
            string key = m_LiveSession.First(kvp => kvp.Value == agentID).Key;
            if (m_Characters.ContainsKey(key))
                return m_Characters[key];
            InworldAI.LogError($"{key} Not Registered!");
            return null;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void SendText(string txtToSend)
        {
            // 1. Interrupt current speaking.
            m_CurrentCharacter.CancelResponse();
            // 2. Send Text.
            m_Client.SendText( m_CurrentCharacter.ID, txtToSend);
        }
        public void SendText(string charID, string txtToSend) => m_Client.SendText(charID, txtToSend);
        public void SendCancelEvent(string charID, string interactionID, List<string> utteranceID) => m_Client.SendCancelEvent(charID, interactionID, utteranceID);
        public void SendTrigger(string triggerName, string charID = "", Dictionary<string, string> parameters = null)
        {
            string charIDToSend = string.IsNullOrEmpty(charID) ? m_CurrentCharacter.ID : charID;
            m_Client.SendTrigger(charIDToSend, triggerName, parameters);
        }
        public void StartAudio(string charID = "")
        {
            string charIDToSend = string.IsNullOrEmpty(charID) ? m_CurrentCharacter.ID : charID;
            InworldAI.Log($"Start Audio Event {charIDToSend}");
            if (!IsRegistered(charIDToSend))
                return;
            m_Client.StartAudio(charIDToSend);
        }
        public void StopAudio(string charID = "")
        {
            string charIDToSend = string.IsNullOrEmpty(charID) ? m_CurrentCharacter.ID : charID;
            InworldAI.Log($"Start Audio Event {charIDToSend}");
            if (!IsRegistered(charIDToSend))
                return;
            m_Client.StopAudio(charIDToSend);
        }
        public void SendAudio(string base64, string charID = "")
        {
            string charIDToSend = string.IsNullOrEmpty(charID) ? m_CurrentCharacter.ID : charID;
            if (!IsRegistered(charIDToSend))
                return;
            m_Client.SendAudio(charIDToSend, base64);
        }
        void _Setup()
        {
            m_Client ??= GetComponent<InworldClient>();
            m_Client.OnStatusChanged += OnStatusChanged;
        }
        void OnStatusChanged(InworldConnectionStatus incomingStatus)
        {
            switch (incomingStatus)
            {
                case InworldConnectionStatus.Initialized:
                    if (m_AutoStart)
                        LoadScene(m_SceneFullName);
                    break;
                case InworldConnectionStatus.LoadingSceneCompleted:
                    StartCoroutine(_RegisterLiveSession());
                    break;
                case InworldConnectionStatus.Connected:
                    // if (m_AutoStart && m_CurrentCharacter == null && m_Characters.Count > 0)
                    //     CurrentCharacter = m_Characters[0];
                    break;
            }
        }
        IEnumerator _RegisterLiveSession()
        {
            Debug.Log("YAN REGISTER!!!");
            LoadSceneResponse response = m_Client.GetLiveSessionInfo();
            if (response == null)
                yield break;
            m_LiveSession.Clear();
            foreach (InworldCharacterData agent in response.agents.Where(agent => !string.IsNullOrEmpty(agent.agentId) && !string.IsNullOrEmpty(agent.brainName)))
            {
                m_LiveSession[agent.brainName] = agent.agentId;
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
            _StartSession();
        }
        void _StartSession() => m_Client.StartSession();
    }
}

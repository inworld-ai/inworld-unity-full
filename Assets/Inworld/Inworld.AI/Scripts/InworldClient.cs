﻿using Inworld.Packet;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
namespace Inworld
{
    [RequireComponent(typeof(AudioCapture))]
    public class InworldClient : MonoBehaviour
    {
        [SerializeField] protected InworldServerConfig m_ServerConfig;
        [SerializeField] protected string m_APIKey;
        [SerializeField] protected string m_APISecret;
        [SerializeField] protected string m_CustomToken;
        public event Action<InworldConnectionStatus> OnStatusChanged;
        public event Action<InworldPacket> OnPacketReceived;
        const string k_NotImplented = "No InworldClient found. Need at least one connection protocol";
        protected Token m_Token;
        protected AudioCapture m_AudioCapture;
        protected string m_SessionKey;
        InworldConnectionStatus m_Status;
        protected string m_Error;

        public void SamplePlayingWave(float[] data, int channels)
        {
            if (!m_AudioCapture || data == null || data.Length == 0)
                return;
            m_AudioCapture.SamplePlayingWavData(data, channels);
        }
        public bool IsRecording
        {
            get => m_AudioCapture.IsCapturing;
            set => m_AudioCapture.IsCapturing = value;
        }
        public InworldServerConfig Server
        {
            get => m_ServerConfig;
            internal set => m_ServerConfig = value;
        }
        public Token Token
        {
            get => m_Token;
            set => m_Token = value;
        }
        public bool IsSpeaking =>  m_AudioCapture.IsSpeaking;
        public virtual bool IsTokenValid => m_Token != null && m_Token.IsValid;
        public virtual void GetAccessToken()
        {
            Error = k_NotImplented;
        }
        public virtual InworldConnectionStatus Status
        {
            get => m_Status;
            protected set
            {
                m_Status = value;
                OnStatusChanged?.Invoke(value);
            }
        }
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
        public void Reconnect() 
        {
            if (IsTokenValid)
                StartSession();
            else
                GetAccessToken();
        }
        public virtual LoadSceneResponse GetLiveSessionInfo()
        {
            Error = k_NotImplented;
            return null;
        }
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
        public virtual void StartSession() => Error = k_NotImplented;
        public virtual void Disconnect() => Error = k_NotImplented;
        public virtual void LoadScene(string sceneFullName) => Error = k_NotImplented;
        public virtual void SendText(string characterID, string textToSend) => Error = k_NotImplented;
        public virtual void SendCancelEvent(string characterID, string interactionID) => Error = k_NotImplented;
        public virtual void SendTrigger(string charID, string triggerName, Dictionary<string, string> parameters) => Error = k_NotImplented;
        public virtual void StartAudio(string charID) => Error = k_NotImplented;
        public virtual void StopAudio(string charID) => Error = k_NotImplented;
        public virtual void SendAudio(string charID, string base64) => Error = k_NotImplented;
        protected virtual void Init()
        {
            m_AudioCapture = GetComponent<AudioCapture>();
        }
        public void Dispatch(InworldPacket packet) => OnPacketReceived?.Invoke(packet);

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
        public void CopyFrom(InworldClient rhs)
        {
            Server = rhs.Server;
            APISecret = rhs.APISecret;
            APIKey = rhs.APIKey;
            CustomToken = rhs.CustomToken;
            InworldController.Client = this;
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
﻿using Inworld.Packet;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Inworld
{
    [RequireComponent(typeof(AudioCapture))]
    public class InworldClient : MonoBehaviour
    {
        [SerializeField] protected InworldServerConfig m_ServerConfig;
        public event Action<InworldConnectionStatus> OnStatusChanged;
        public event Action<InworldPacket> OnPacketReceived;
        const string k_NotImplented = "No InworldClient found. Need at least one connection protocol";
        protected Token m_Token;
        protected AudioCapture m_AudioCapture;
        protected string m_SessionKey;
        InworldConnectionStatus m_Status;
        protected string m_Error;

        public bool IsRecording => m_AudioCapture.IsCapturing;
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
/*************************************************************************************************
* Copyright 2022-2024 Theai, Inc. dba Inworld AI
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Interactions;
using Inworld.Runtime.RPM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace Inworld.Sample.RPM
{
    public class SessionCanvas : DemoCanvas
    {
        [SerializeField] InworldCharacter m_Character;
        [Header("UI")]
        [Header("Ping:")]
        [SerializeField] Gradient m_ColorGraph;
        [SerializeField] Image m_Indicator;
        [SerializeField] float m_PingDuration = 1f;
        [Header("Toggles")]
        [SerializeField] Button m_CalibrateAudio;
        [SerializeField] SwitchButton m_SwitchMic;
        [SerializeField] SwitchButton m_Speaker;
        [Header("Session")]
        [SerializeField] Button m_NewGame;
        [SerializeField] Button m_SaveGame;
        [SerializeField] Button m_LoadGame;
        [SerializeField] InworldAudioInteraction m_Interaction;

        float m_CurrentDuration;
        bool m_IsLoad;
        bool m_IsConnecting;
        IEnumerator m_CurrentCoroutine;
        readonly Queue<float> m_LagQueue = new Queue<float>(12);
        
        /// <summary>
        /// Mute/Unmute the microphone.
        /// </summary>
        public void MicrophoneControl(bool isOn)
        {
            // TODO(Yan): Replaced with DetecterPlayerSpeakingModule.
            // InworldController.Audio.AutoDetectPlayerSpeaking = !isOn;
        }

        /// <summary>
        /// Clear the saved data
        /// </summary>
        public void NewGame(bool loadHistory)
        {
            if (!loadHistory)
                InworldController.Client.SessionHistory = "";
            InworldController.CharacterHandler.Register(m_Character);
            InworldController.CharacterHandler.CurrentCharacter = m_Character;
        }

        public void QuitGame()
        {
            m_Character.CancelResponse();
            InworldController.CharacterHandler.Unregister(m_Character);
            InworldController.Instance.Disconnect();
        }

        /// <summary>
        /// Mute/Unmute the speaker.
        /// </summary>
        public void SwitchVolume(bool isOn)
        {
            if (!m_Interaction)
                return;
            m_Interaction.IsMute = isOn;
        }
        /// <summary>
        /// Save game
        /// </summary>
        public void SaveGame() => InworldController.Client.GetHistoryAsync(InworldController.Instance.CurrentScene);
        protected void Awake()
        {
            _SessionButtonReadyToStart();
        }
        protected override void Start()
        {
            base.Start();
            StartCoroutine(_PingInworld());
        }
        protected IEnumerator _RestartAsync()
        {
            if (InworldController.Status == InworldConnectionStatus.Connected)
            {
                InworldController.Instance.Disconnect(); 
            }
            while (InworldController.Status != InworldConnectionStatus.Idle) 
            {
                yield return new WaitForFixedUpdate();
            }
            InworldController.Instance.Init();
        }
        protected override void OnStatusChanged(InworldConnectionStatus incomingStatus)
        {
            base.OnStatusChanged(incomingStatus);
            switch (incomingStatus)
            {
                case InworldConnectionStatus.Idle:
                    m_Indicator.color = Color.white;
                    _SessionButtonReadyToStart();
                    m_IsConnecting = false;
                    break;
                case InworldConnectionStatus.Connecting:
                    _SessionButtonConnecting();
                    m_IsConnecting = true;
                    break;
                case InworldConnectionStatus.Connected:
                    _SessionButtonConnected();
                    break;
                case InworldConnectionStatus.Initializing:
                    m_Indicator.color = m_ColorGraph.Evaluate(0.5f);
                    break;
                case InworldConnectionStatus.Error:
                    m_Indicator.color = m_ColorGraph.Evaluate(1f);
                    m_IsConnecting = false;
                    break;
            }
        }
        protected override void OnCharacterJoined(InworldCharacter character)
        {
            base.OnCharacterJoined(character);
            m_Title.text = $"{character.Name} joined";
        }
        
        protected override void OnCharacterLeft(InworldCharacter character)
        {
            base.OnCharacterLeft(character);
            m_Title.text = $"{character.Name} left";
        }

        void _SessionButtonReadyToStart()
        {
            m_NewGame.interactable = true;
            m_LoadGame.interactable = true;
            m_SaveGame.interactable = false;
            m_CalibrateAudio.interactable = false;
            m_SwitchMic.interactable = false;
            m_Speaker.interactable = false;
        }
        void _SessionButtonConnecting()
        {
            m_NewGame.interactable = false;
            m_LoadGame.interactable = false;
            m_SaveGame.interactable = false;
            m_CalibrateAudio.interactable = false;
            m_SwitchMic.interactable = false;
            m_Speaker.interactable = false;
        }
        void _SessionButtonConnected()
        {
            m_NewGame.interactable = true;
            m_LoadGame.interactable = true;
            m_SaveGame.interactable = true;
            m_CalibrateAudio.interactable = true;
            m_SwitchMic.interactable = true;
            m_Speaker.interactable = true;
        }
        void _UpdatePing(int nPingTime)
        {
            m_LagQueue.Enqueue(nPingTime);
            if (m_LagQueue.Count > 10)
                m_LagQueue.Dequeue();
            float avg = m_LagQueue.Average();
            m_Indicator.color = m_ColorGraph.Evaluate(avg * 0.002f); //YAN: 500ms is RED.
            m_Content.text = $"Ping: {avg}ms";
        }
        IEnumerator _PingInworld()
        {
            while (enabled)
            {
                if (m_IsConnecting)
                {
                    _UpdatePing(InworldController.Instance ? InworldController.Client.Ping : 1000);
                }
                yield return new WaitForSeconds(m_PingDuration);
            }
        }
    }
}

/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Util;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
namespace Inworld.Sample
{
    public class SessionCanvas : DemoCanvas
    {
        [Header("UI")]
        [SerializeField] Gradient m_ColorGraph;
        [SerializeField] Image m_Indicator;
        [SerializeField] Toggle m_PlayPause;
        [SerializeField] Toggle m_SwitchMic;
        [SerializeField] Toggle m_Mute;

        [SerializeField] Button m_NewGame;
        [SerializeField] Button m_SaveGame;
        [SerializeField] Button m_LoadGame;
        [SerializeField] Button m_Disconnect;

        
        [SerializeField] float m_PingDuration = 1f;
        string ipv4;
        float m_CurrentDuration;
        bool m_IsConnecting;
        bool m_HasInit;
        readonly Queue<float> m_LagQueue = new Queue<float>(12);

        public bool EnableCtrl => m_PlayPause && m_SwitchMic && m_Mute;
        void Awake()
        {
            if (string.IsNullOrEmpty(ipv4))
                ipv4 = Dns.GetHostAddresses(InworldAI.Game.currentServer.runtime)[0].ToString();
            _SwitchToggles(true, true);
            _ResetButtons(false);
        }
        void Start()
        {
            InworldController.Instance.OnStateChanged += OnStatusChanged;
            InworldController.Instance.OnCharacterChanged += OnCharacterChanged;
            StartCoroutine(_PingInworld());
        }

        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnStateChanged -= OnStatusChanged;
            InworldController.Instance.OnCharacterChanged -= OnCharacterChanged;
        }
        public async void PlayPause()
        {
            if (m_PlayPause.isOn)
                StartGame(true);
            else
                await InworldController.Instance.Disconnect();
        }
        public void StartGame(bool loadSaveData)
        {
            InworldController.Instance.LoadSaveData = loadSaveData;
            if (!m_HasInit)
                InworldController.Instance.Init();
            else
                InworldController.Instance.Reconnect();
        }
        public async void Disconnect() => await InworldController.Instance.Disconnect();
        public void SaveGame() => InworldController.Instance.SaveGame();
        public void MicrophoneControl()
        {
	        if (!m_SwitchMic)
		        return;
			InworldController.Audio.IsBlocked = !m_SwitchMic.isOn;
        }
        public void SwitchVolume()
        {
	        if (InworldController.Instance.CurrentCharacter == null)
		        return;
	        if (!m_Mute)
		        return;
            InworldController.Instance.CurrentCharacter.IsMute = m_Mute.isOn;
        }
        protected override void OnCharacterChanged(InworldCharacter oldCharacter, InworldCharacter newCharacter)
        {
	        base.OnCharacterChanged(oldCharacter, newCharacter);
	        MicrophoneControl();
	        SwitchVolume();
        }
        protected override void OnStatusChanged(ControllerStates incomingStatus)
        {
            base.OnStatusChanged(incomingStatus);
            switch (incomingStatus)
            {
                case ControllerStates.Idle:
                case ControllerStates.LostConnect:
                    m_Indicator.color = Color.white;
                    m_IsConnecting = false;
                    _SwitchToggles(true, true);
                    _ResetButtons(false);
                    break;
                case ControllerStates.Initialized:
                    m_HasInit = true;
                    break;
                case ControllerStates.Connecting:
                    _SwitchToggles(false);
                    break;
                case ControllerStates.Connected:
                    _SwitchToggles(true);
                    _ResetButtons(true);
                    break;
                case ControllerStates.Initializing:
                    m_Indicator.color = m_ColorGraph.Evaluate(0.5f);
                    m_IsConnecting = true;
                    break;
                case ControllerStates.Error:
                    m_Indicator.color = m_ColorGraph.Evaluate(1f);
                    m_IsConnecting = false;
                    _ResetButtons(false);
                    break;
            }

        }
        void _ResetButtons(bool isConnected)
        {
            m_NewGame.interactable = !isConnected;
            m_LoadGame.interactable = !isConnected && InworldController.Instance.HasSavedData;
            m_SaveGame.interactable = isConnected;
            m_Disconnect.interactable = isConnected;
        }
        
        
        void _SwitchToggles(bool isOn, bool playBtnOnly = false)
        {
            if (!EnableCtrl)
                return;
            m_PlayPause.interactable = isOn;
            m_Mute.interactable = isOn && !playBtnOnly;
            m_SwitchMic.interactable = isOn && !playBtnOnly;
            if (!isOn || playBtnOnly) 
	            return;
            MicrophoneControl();
            SwitchVolume();
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
                    Ping ping = new Ping(ipv4);
                    while (!ping.isDone && m_CurrentDuration < m_PingDuration)
                    {
                        m_CurrentDuration += Time.fixedDeltaTime;
                        yield return new WaitForFixedUpdate();
                    }
                    m_CurrentDuration = 0;
                    _UpdatePing(ping.isDone ? ping.time : 1000);
                }
                yield return new WaitForSeconds(m_PingDuration);
            }
        }
    }
}

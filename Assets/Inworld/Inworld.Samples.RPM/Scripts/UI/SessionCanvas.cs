/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Interactions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

namespace Inworld.Sample.RPM
{
    public class SessionCanvas : DemoCanvas
    {
        [Header("UI")]
        [SerializeField] Gradient m_ColorGraph;
        [SerializeField] Image m_Indicator;
        [SerializeField] Toggle m_PlayPause;
        [SerializeField] Toggle m_SwitchMic;
        [SerializeField] Toggle m_Speaker;
        [SerializeField] float m_PingDuration = 1f;
        [SerializeField] InworldAudioInteraction m_Interaction;
        string ipv4;
        float m_CurrentDuration;
        bool m_IsConnecting;
        bool m_HasInit;
        readonly Queue<float> m_LagQueue = new Queue<float>(12);

        /// <summary>
        /// Get if this canvas allows user to control the audio options.
        /// </summary>
        public bool EnableCtrl => m_PlayPause && m_SwitchMic && m_Speaker;
        
        /// <summary>
        /// Pause/Continue the current live session.
        /// </summary>
        public void PlayPause()
        {
            if (m_PlayPause.isOn)
            {
                if (!m_HasInit)
                    InworldController.Instance.Init();
                else
                    InworldController.Instance.Reconnect();
            }
            else
                InworldController.Instance.Disconnect();
        }
        
        /// <summary>
        /// Mute/Unmute the microphone.
        /// </summary>
        public void MicrophoneControl()
        {
            if (!m_SwitchMic)
                return;
            InworldController.Audio.IsBlocked = !m_SwitchMic.isOn;
        }
        
        /// <summary>
        /// Mute/Unmute the speaker.
        /// </summary>
        public void SwitchVolume()
        {
            if (!m_Speaker || !m_Interaction)
                return;
            m_Interaction.IsMute = !m_Speaker.isOn;
        }
        protected override void Awake()
        {
            base.Awake();
            if (string.IsNullOrEmpty(ipv4))
                ipv4 = Dns.GetHostAddresses(InworldController.Client.Server.web)[0].ToString();
            _SwitchToggles(true, true);
        }
        protected override void Start()
        {
            base.Start();
            StartCoroutine(_PingInworld());
        }
        protected override void OnStatusChanged(InworldConnectionStatus incomingStatus)
        {
            base.OnStatusChanged(incomingStatus);
            switch (incomingStatus)
            {
                case InworldConnectionStatus.Idle:
                case InworldConnectionStatus.LostConnect:
                    m_Indicator.color = Color.white;
                    m_IsConnecting = false;
                    _SwitchToggles(true, true);
                    break;
                case InworldConnectionStatus.Initialized:
                    m_HasInit = true;
                    InworldController.Instance.LoadScene();
                    break;
                case InworldConnectionStatus.Connecting:
                    _SwitchToggles(false);
                    break;
                case InworldConnectionStatus.Connected:
                    _SwitchToggles(true);
                    break;
                case InworldConnectionStatus.Initializing:
                    m_Indicator.color = m_ColorGraph.Evaluate(0.5f);
                    m_IsConnecting = true;
                    break;
                case InworldConnectionStatus.Error:
                    m_Indicator.color = m_ColorGraph.Evaluate(1f);
                    m_IsConnecting = false;
                    break;
                case InworldConnectionStatus.InitFailed:
                    m_Indicator.color = m_ColorGraph.Evaluate(1f);
                    m_IsConnecting = false;
                    break;
            }

        }
        void _SwitchToggles(bool isOn, bool playBtnOnly = false)
        {
            if (!EnableCtrl)
                return;
            if(m_Speaker)
                m_Speaker.interactable = isOn && !playBtnOnly;
            if(m_SwitchMic)
                m_SwitchMic.interactable = isOn && !playBtnOnly;
            
            if (isOn && !playBtnOnly)
            {
                MicrophoneControl();
                SwitchVolume();
            }
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
                #if !UNITY_WEBGL
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
                #endif
                yield return new WaitForSeconds(m_PingDuration);
            }
        }
    }
}

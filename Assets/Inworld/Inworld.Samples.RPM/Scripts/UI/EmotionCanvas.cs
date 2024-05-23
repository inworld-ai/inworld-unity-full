/*************************************************************************************************
* Copyright 2022-2024 Theai, Inc. dba Inworld AI
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Assets;
using UnityEngine;
using Inworld.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace Inworld.Sample.RPM
{
    public class EmotionCanvas : DemoCanvas
    {
        [SerializeField] EmotionMap m_EmotionMap;
        [SerializeField] TMP_Dropdown m_StatusDropdown;
        [SerializeField] TMP_Dropdown m_ServerEventDropDown;
        [SerializeField] InworldCharacter m_Character;
        static readonly int s_Emotion = Animator.StringToHash("Emotion");
        static readonly int s_Gesture = Animator.StringToHash("Gesture");
        static readonly int s_Motion = Animator.StringToHash("MainStatus");

        SpaffCode m_CurrentSpaff = SpaffCode.NEUTRAL;
        SpaffCode m_LastSpaff = SpaffCode.NEUTRAL;
        /// <summary>
        /// Get the current spaffcode of emotion.
        /// </summary>
        public SpaffCode Emotion
        {
            get => m_CurrentSpaff;
            private set
            {
                m_LastSpaff = m_CurrentSpaff;
                m_CurrentSpaff = value;
            }
        }
        /// <summary>
        /// Set the current emotion of the animator.
        /// </summary>
        /// <param name="emotion">the enum of the emotion to send.</param>
        public void SendEmotion(int emotion)
        {
            if (!m_Character || !m_Character.Animator)
                return;
            m_Character.Animator.SetInteger(s_Emotion, emotion);
            m_Title.text = $"Set Emotion {(Emotion)emotion}";
        }
        /// <summary>
        /// Set the current gesture of the animator.
        /// </summary>
        /// <param name="gesture">the enum of the gesture to send.</param>
        public void SendGesture(int gesture)
        {
            if (!m_Character || !m_Character.Animator)
                return;
            m_Character.Animator.SetInteger(s_Gesture, gesture);
            m_Title.text = $"Set Gesture {(Gesture)gesture}";
        }
        /// <summary>
        /// Set the main status of the animator.
        /// </summary>
        /// <param name="mainStatus">the enum of the main status to send.</param>
        public void SetMainStatus(int mainStatus)
        {
            if (!m_Character || !m_Character.Animator)
                return;
            m_Character.Animator.SetInteger(s_Motion, mainStatus);
            m_Title.text = $"Set Main {(AnimMainStatus)mainStatus}";
        }
        /// <summary>
        /// Create a mock emotion events (similar data from server) and test its behavior.
        /// </summary>
        /// <param name="nSpaffCode">the spaffcode of the emotion.</param>
        public void MockServerEmoEvents(int nSpaffCode)
        {
            EmotionPacket evt = new EmotionPacket
            {
                routing = new Routing(m_Character.ID),
                emotion = new EmotionEvent
                {
                    behavior = m_EmotionMap.data[nSpaffCode].name
                }
            };
            m_Character.Event.onPacketReceived.Invoke(evt);
        }
        string _ServerState
        {
            get
            {
                string emotion = m_LastSpaff == m_CurrentSpaff ? m_CurrentSpaff.ToString() : $"Last: {m_LastSpaff} Current: {m_CurrentSpaff}";
                return $"Server Status: <color=green>{m_ServerStatus}</color>\nEmotion: <color=green>{emotion}</color>\n";
            }
        }

        string _ClientState
        {
            get
            {
                if (!m_Character || !m_Character.Animator)
                    return "";
                Emotion emotion = (Emotion)m_Character.Animator.GetInteger(s_Emotion);
                Gesture gesture = (Gesture)m_Character.Animator.GetInteger(s_Gesture);
                AnimMainStatus animMainStatus = (AnimMainStatus)m_Character.Animator.GetInteger(s_Motion);
                return $"Client:\nEmotion: <color=green>{emotion}</color>\tGesture: <color=green>{gesture}</color>\nMain Status: <color=green>{animMainStatus}</color>";
            }
        }
        void Awake()
        {
            m_ServerEventDropDown.options = new List<TMP_Dropdown.OptionData>();
            foreach (var emoData in m_EmotionMap.data)
            {
                m_ServerEventDropDown.options.Add(new TMP_Dropdown.OptionData
                {
                    image = null,
                    text = emoData.name.ToString()
                });
            }
        }
        // Start is called before the first frame update
        protected override void OnEnable()
        {
            base.OnEnable();
            m_Character.Event.onPacketReceived.AddListener(OnPacketEvents);
            Debug.Log("EmotionCanvas Start");
        }
        void Update()
        {
            if (!m_Character || !m_Character.Animator)
                return;
            m_Content.text = $"{_ServerState}\n{_ClientState}";
            if (m_StatusDropdown) 
            {
                m_StatusDropdown.value = m_Character.Animator.GetInteger(s_Motion); 
            }
            
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            m_Character.Event.onPacketReceived.RemoveListener(OnPacketEvents);
        }

        void OnPacketEvents(InworldPacket packet)
        {
            if (string.IsNullOrEmpty(m_Character.ID))
                return;
            if (packet.routing.target.name != m_Character.ID && packet.routing.source.name != m_Character.ID)
            {            
                return;
            }
            switch (packet)
            {
                case EmotionPacket emotionEvent:
                    HandleEmotion(emotionEvent.emotion.behavior);
                    break;
            }
        }
        void HandleEmotion(SpaffCode incomingSpaff)
        {
            Emotion = incomingSpaff;
            m_Title.text = $"Get Emotion {incomingSpaff}";
            for (int i = 0; i < m_ServerEventDropDown.options.Count; i++)
            {
                if (!string.Equals(m_ServerEventDropDown.options[i].text, incomingSpaff.ToString(), StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
                m_ServerEventDropDown.value = i;
                break;
            }
        }
    }
}

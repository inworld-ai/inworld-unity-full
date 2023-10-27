/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Assets;
using UnityEngine;
using Inworld.Packet;
using System;
using TMPro;

namespace Inworld.Sample.RPM
{
    public class EmotionCanvas : DemoCanvas
    {
        [SerializeField] EmotionMap m_EmotionMap;
        [SerializeField] TMP_Dropdown m_StatusDropdown;
        [SerializeField] TMP_Dropdown m_ServerEventDropDown;
        static readonly int s_Emotion = Animator.StringToHash("Emotion");
        static readonly int s_Gesture = Animator.StringToHash("Gesture");
        static readonly int s_Motion = Animator.StringToHash("MainStatus");
        Animator m_Animator;

        string m_CurrentSpaff = "";
        string m_LastSpaff = "";
        /// <summary>
        /// Get the current spaffcode of emotion.
        /// </summary>
        public string Emotion
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
            if (!m_CharacterHandler.CurrentCharacter || !m_Animator)
                return;
            m_Animator.SetInteger(s_Emotion, emotion);
            m_Title.text = $"Set Emotion {(Emotion)emotion}";
        }
        /// <summary>
        /// Set the current gesture of the animator.
        /// </summary>
        /// <param name="gesture">the enum of the gesture to send.</param>
        public void SendGesture(int gesture)
        {
            if (!m_CharacterHandler.CurrentCharacter || !m_Animator)
                return;
            m_Animator.SetInteger(s_Gesture, gesture);
            m_Title.text = $"Set Gesture {(Gesture)gesture}";
        }
        /// <summary>
        /// Set the main status of the animator.
        /// </summary>
        /// <param name="mainStatus">the enum of the main status to send.</param>
        public void SetMainStatus(int mainStatus)
        {
            if (!m_CharacterHandler.CurrentCharacter || !m_Animator)
                return;
            m_Animator.SetInteger(s_Motion, mainStatus);
            m_Title.text = $"Set Main {(AnimMainStatus)mainStatus}";
        }
        /// <summary>
        /// Create a mock emotion events (similar data from server) and test its behavior.
        /// </summary>
        /// <param name="nSpaffCode">the spaffcode of the emotion.</param>
        public void MockServerEmoEvents(int nSpaffCode)
        {
            if (!m_CharacterHandler.CurrentCharacter)
            {
                InworldAI.LogError("Please wait until character initialized!");
                return;
            }
            EmotionPacket evt = new EmotionPacket
            {
                routing = new Routing(m_CharacterHandler.CurrentCharacter.ID),
                emotion = new EmotionEvent
                {
                    behavior = m_EmotionMap.data[nSpaffCode].name
                }
            };
            InworldController.Instance.CharacterInteract(evt);
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
                if (!m_Animator)
                    return "";
                Emotion emotion = (Emotion)m_Animator.GetInteger(s_Emotion);
                Gesture gesture = (Gesture)m_Animator.GetInteger(s_Gesture);
                AnimMainStatus animMainStatus = (AnimMainStatus)m_Animator.GetInteger(s_Motion);
                return $"Client:\nEmotion: <color=green>{emotion}</color>\tGesture: <color=green>{gesture}</color>\nMain Status: <color=green>{animMainStatus}</color>";
            }
        }

        // Start is called before the first frame update
        protected override void OnEnable()
        {
            base.OnEnable();
            InworldController.Instance.OnCharacterInteraction += OnPacketEvents;
            Debug.Log("EmotionCanvas Start");
        }
        void Update()
        {
            if (!m_Animator)
                return;
            m_Content.text = $"{_ServerState}\n{_ClientState}";
            if (m_StatusDropdown) 
            {
                m_StatusDropdown.value = m_Animator.GetInteger(s_Motion); 
            }
            
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnCharacterInteraction -= OnPacketEvents;
        }

        protected override void OnCharacterChanged(InworldCharacter oldCharacter, InworldCharacter newCharacter)
        {
            if (!newCharacter && oldCharacter)
                m_Title.text = $"{oldCharacter.transform.name} Disconnected!";
            else
            {
                m_Title.text = $"{newCharacter.transform.name} connected!";
                m_Animator = newCharacter.GetComponent<Animator>();
            }
        }
        void OnPacketEvents(InworldPacket packet)
        {
            string charID = m_CharacterHandler.CurrentCharacter.ID;
            if (packet.routing.target.name != charID && packet.routing.source.name != charID)
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
        void HandleEmotion(string incomingSpaff)
        {
            Emotion = incomingSpaff;
            m_Title.text = $"Get Emotion {incomingSpaff}";
            for (int i = 0; i < m_ServerEventDropDown.options.Count; i++)
            {
                if (!string.Equals(m_ServerEventDropDown.options[i].text, incomingSpaff, StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
                m_ServerEventDropDown.value = i;
                break;
            }
        }
    }
}

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
        public string Emotion
        {
            get => m_CurrentSpaff;
            private set
            {
                m_LastSpaff = m_CurrentSpaff;
                m_CurrentSpaff = value;
            }
        }
        string _ServerState
        {
            get
            {
                string emotion = m_LastSpaff == m_CurrentSpaff ? m_CurrentSpaff.ToString() : $"Last: {m_LastSpaff} Current: {m_CurrentSpaff}";
                return $"Server Status:\nEmotion: <color=green>{emotion}</color>\n";
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
                return $"Client Status:\nEmotion: <color=green>{emotion}</color>\tGesture: <color=green>{gesture}</color>\nMain Status: <color=green>{animMainStatus}</color>";
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            InworldController.Client.OnStatusChanged += OnStatusChanged;
            InworldController.Instance.OnCharacterChanged += OnCharacterChanged;
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
        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Client.OnStatusChanged -= OnStatusChanged;
            InworldController.Instance.OnCharacterChanged -= OnCharacterChanged;
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
            string charID = InworldController.Instance.CurrentCharacter.ID;
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
        public void SendEmotion(int emotion)
        {
            if (!InworldController.Instance.CurrentCharacter || !m_Animator)
                return;
            m_Animator.SetInteger(s_Emotion, emotion);
            m_Title.text = $"Set Emotion {(Emotion)emotion}";
        }
        public void SendGesture(int gesture)
        {
            if (!InworldController.Instance.CurrentCharacter || !m_Animator)
                return;
            m_Animator.SetInteger(s_Gesture, gesture);
            m_Title.text = $"Set Gesture {(Gesture)gesture}";
        }
        public void SetMainStatus(int mainStatus)
        {
            if (!InworldController.Instance.CurrentCharacter || !m_Animator)
                return;
            m_Animator.SetInteger(s_Motion, mainStatus);
            m_Title.text = $"Set Main {(AnimMainStatus)mainStatus}";
        }
        public void MockServerEmoEvents(int nSpaffCode)
        {
            if (!InworldController.Instance.CurrentCharacter)
            {
                InworldAI.LogError("Please wait until character initialized!");
                return;
            }
            EmotionPacket evt = new EmotionPacket
            {
                routing = new Routing(InworldController.Instance.CurrentCharacter.ID),
                emotion = new EmotionEvent
                {
                    behavior = m_EmotionMap.data[nSpaffCode].name
                }
            };
            InworldController.Instance.CharacterInteract(evt);
        }
    }
}

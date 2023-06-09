/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Grpc;
using Inworld.Util;
using UnityEngine;
using InworldPacket = Inworld.Packets.InworldPacket;

namespace Inworld.Sample
{
    public class EmotionCanvas : DemoCanvas
    {
        static readonly int s_Emotion = Animator.StringToHash("Emotion");
        static readonly int s_Gesture = Animator.StringToHash("Gesture");
        static readonly int s_Motion = Animator.StringToHash("MainStatus");
        Animator m_Animator;

        string m_CharName;
        EmotionEvent.Types.SpaffCode m_CurrentSpaff;
        EmotionEvent.Types.SpaffCode m_LastSpaff;
        public EmotionEvent.Types.SpaffCode Emotion
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
            InworldController.Instance.OnStateChanged += OnStatusChanged;
            InworldController.Instance.OnCharacterChanged += OnCharacterChanged;
            InworldController.Instance.OnPacketReceived += OnPacketEvents;
        }
        void Update()
        {
            if (!m_Animator)
                return;
            m_Content.text = $"{_ServerState}\n{_ClientState}";
        }
        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnStateChanged -= OnStatusChanged;
            InworldController.Instance.OnCharacterChanged -= OnCharacterChanged;
            InworldController.Instance.OnPacketReceived -= OnPacketEvents;
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
            if (packet.Routing.Target.Id != charID && packet.Routing.Source.Id != charID)
                return;
            switch (packet)
            {
                case Packets.EmotionEvent emotionEvent:
                    HandleEmotion(emotionEvent.SpaffCode);
                    break;
            }
        }
        void HandleEmotion(EmotionEvent.Types.SpaffCode incomingSpaff)
        {
            Emotion = incomingSpaff;
            m_Title.text = $"Get Emotion {incomingSpaff}";
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
    }
}

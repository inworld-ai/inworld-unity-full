﻿/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Interactions;
using Inworld.Packet;
using UnityEngine;

namespace Inworld.Assets
{
    public class InworldAnimation : MonoBehaviour
    {
        protected InworldCharacter m_Character;
        protected InworldInteraction m_Interaction;
        
        protected virtual void Awake()
        {
            enabled = Init();
        }
        protected virtual void OnEnable()
        {
            InworldController.Instance.OnCharacterInteraction += OnInteractionChanged;
        }
        
        protected virtual void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnCharacterInteraction -= OnInteractionChanged;
        }
        protected virtual bool Init()
        {
            if (!m_Character)
                m_Character = GetComponent<InworldCharacter>();
            if (!m_Interaction)
                m_Interaction = GetComponent<InworldInteraction>();
            return m_Character && m_Interaction;
        }
            
        protected virtual void OnInteractionChanged(InworldPacket packet)
        {
            if (m_Character && !string.IsNullOrEmpty(m_Character.ID) && 
                packet?.routing?.source?.name == m_Character.ID || packet?.routing?.target?.name == m_Character.ID)
                ProcessPacket(packet);
        }
        protected virtual void ProcessPacket(InworldPacket incomingPacket)
        {
            switch (incomingPacket)
            {
                case AudioPacket audioPacket: // Already Played.
                    HandleLipSync(audioPacket);
                    break;
                case EmotionPacket emotionPacket:
                    HandleEmotion(emotionPacket);
                    break;
            }
        }
        protected virtual void HandleLipSync(AudioPacket audioPacket)
        {
            // Handled by children.
        }

        protected virtual void HandleEmotion(EmotionPacket emotionPacket)
        {
            // Handled by children.
        }
    }
}

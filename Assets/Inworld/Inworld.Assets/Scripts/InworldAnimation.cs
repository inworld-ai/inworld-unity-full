/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
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
        [SerializeField] protected EmotionMap m_EmotionMap;
        protected virtual void Awake()
        {
            enabled = Init();
        }
        protected virtual void OnEnable()
        {
            m_Character.Event.onPacketReceived.AddListener(ProcessPacket);
        }
        
        protected virtual void OnDisable()
        {
            m_Character.Event.onPacketReceived.RemoveListener(ProcessPacket);
        }
        protected virtual bool Init()
        {
            if (!m_Character)
                m_Character = GetComponent<InworldCharacter>();
            if (!m_Interaction)
                m_Interaction = GetComponent<InworldInteraction>();
            return m_Character && m_Interaction;
        }
        protected virtual void ProcessPacket(InworldPacket incomingPacket)
        {
            switch (incomingPacket)
            {
                case AudioPacket audioPacket: // Already Played.
                    if (audioPacket?.routing?.source?.name == m_Character.ID)
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

/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Packet;
using Inworld.Sample;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Inworld.Assets
{
    public class ChatPanel3D : ChatPanel
    {
        [SerializeField] EmotionMap m_EmotionMap;
        [SerializeField] InworldFacialEmotion m_Emotion;
        [SerializeField] TMP_Text m_Relation;
        [SerializeField] Image m_EmoIcon;
        [SerializeField] GameObject m_Dots;
        [SerializeField] InworldCharacter m_Character;
        [SerializeField] GameObject m_Canvas;
        void Start()
        {
            if (m_Dots)
                m_Dots.SetActive(false);
        }
        void Update()
        {
            if (!PlayerController.Instance || !m_Canvas)
                return;
            m_Canvas.transform.LookAt(PlayerController.Instance.transform.position);
            m_Canvas.transform.eulerAngles = Vector3.up * (m_Canvas.transform.eulerAngles.y + 180f); 
        }
        protected override void OnInteraction(InworldPacket incomingPacket)
        {
            if (m_Character && incomingPacket.IsRelated(m_Character.ID))
                base.OnInteraction(incomingPacket);
        }
        protected override void OnChatUpdated()
        {
            // YAN: 3D Character does not need to process LLM data.
        }
        protected override bool HandleRelation(CustomPacket packet)
        {
            if (!m_Relation)
                return false;
            string result = "";

            RelationState currentRelation = new RelationState();
            foreach (TriggerParameter param in packet.custom.parameters)
            {
                currentRelation.UpdateByTrigger(param);
            }
            if (currentRelation.attraction != 0)
                result += $"Attraction: {GetRelationIcon(currentRelation.attraction)}\n";
            if (currentRelation.familiar != 0)
                result += $"Familiar: {GetRelationIcon(currentRelation.familiar)}\n";
            if (currentRelation.flirtatious != 0)
                result += $"Flirtatious: {GetRelationIcon(currentRelation.flirtatious)}\n";
            if (currentRelation.respect != 0)
                result += $"Respect: {GetRelationIcon(currentRelation.respect)}\n";
            if (currentRelation.trust != 0)
                result += $"Trust: {GetRelationIcon(currentRelation.trust)}\n";
            m_Relation.text = result;
            return true;
        }

        protected override bool HandleText(TextPacket textPacket)
        {
            if (m_Dots)
                m_Dots.SetActive(true);
            return base.HandleText(textPacket);
        }
        protected override void HandleControl(ControlPacket packet)
        {
            if (m_Dots && packet.Action == ControlType.INTERACTION_END)
                m_Dots.SetActive(false);
        }
        protected override void HandleEmotion(EmotionPacket emotionPacket)
        {
            _ProcessEmotion(emotionPacket.emotion.behavior);
        }
        void _ProcessEmotion(SpaffCode emotion)
        {
            EmotionMapData emoMapData = m_EmotionMap[emotion];
            if (emoMapData == null)
            {
                InworldAI.LogError($"Unhandled emotion {emotion}");
                return;
            }
            FacialAnimation targetEmo = m_Emotion[emoMapData.facialEmotion.ToString()];
            if (targetEmo == null)
            {
                InworldAI.LogError($"Unhandled emotion {emotion}");
                return;
            }
            m_EmoIcon.sprite = targetEmo.icon;
        }

        string GetRelationIcon(int nRelationValue)
        {
            string result = "";
            if (nRelationValue > 0)
            {
                int nIcon = nRelationValue / 10;
                for (int i = 0; i < nIcon; i++)
                {
                    result += "<color=green>+</color>";
                }
            }
            else if (nRelationValue < 0)
            {
                int nIcon = Mathf.Abs(nRelationValue / 10);
                for (int i = 0; i < nIcon; i++)
                {
                    result += "<color=red>-</color>";
                }
            }
            return result;
        }
    }
}


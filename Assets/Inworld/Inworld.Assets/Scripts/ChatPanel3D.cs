/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Packet;
using Inworld.Sample;
using System;
using System.Linq;
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
        
        [SerializeField] InworldCharacter m_Character;

        protected override void OnInteraction(InworldPacket incomingPacket)
        {
            // YAN: Filter unrelated interactions, but not too related. (Only if you're sending/receiving)
            if (m_Character && 
                (incomingPacket.Source == SourceType.PLAYER && incomingPacket.IsBroadCast
                 || incomingPacket.IsSource(m_Character.ID) 
                 || incomingPacket.IsTarget(m_Character.ID)))
            base.OnInteraction(incomingPacket);
        }
        protected override void HandleRelation(CustomPacket packet)
        {
            if (!m_Relation)
                return;
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
        }

        protected override void HandleEmotion(EmotionPacket emotionPacket)
        {
            _ProcessEmotion(emotionPacket.emotion.behavior.ToUpper());
        }
        void _ProcessEmotion(string emotion)
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


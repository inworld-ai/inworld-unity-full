/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Packet;
using Inworld.Sample;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Inworld.Assets
{
    public class ChatPanel3D : ChatPanel
    {
        [SerializeField] InworldFacialEmotion m_Emotion;
        [SerializeField] TMP_Text m_Relation;
        [SerializeField] Image m_EmoIcon;
        
        [SerializeField] InworldCharacter m_Character;

        protected override void OnInteraction(InworldPacket incomingPacket)
        {
            // YAN: Filter unrelated interactions.
            if (!m_Character ||
                string.IsNullOrEmpty(m_Character.ID) ||
                incomingPacket?.routing?.source?.name != m_Character.ID && incomingPacket?.routing?.target?.name != m_Character.ID)
                return;
            base.OnInteraction(incomingPacket);
        }
        protected override void HandleRelation(RelationPacket packet)
        {
            if (!m_Relation)
                return;
            string result = "";
            RelationState currentRelation = packet.debugInfo.relation.relationUpdate;
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
            switch (emotionPacket.emotion.behavior.ToUpper())
            {
                case "AFFECTION":
                case "INTEREST":
                    _ProcessEmotion("Anticipation");
                    break;
                case "HUMOR":
                case "JOY":
                    _ProcessEmotion("Joy");
                    break;
                case "CONTEMPT":
                case "CRITICISM":
                case "DISGUST":
                    _ProcessEmotion("Disgust");
                    break;
                case "BELLIGERENCE":
                case "DOMINEERING":
                case "ANGER":
                    _ProcessEmotion("Anger");
                    break;
                case "TENSION":
                case "STONEWALLING":
                case "TENSEHUMOR":
                case "DEFENSIVENESS":
                    _ProcessEmotion("Fear");
                    break;
                case "WHINING":
                case "SADNESS":
                    _ProcessEmotion("Sadness");
                    break;
                case "SURPRISE":
                    _ProcessEmotion("Surprise");
                    break;
                default:
                    _ProcessEmotion("Neutral");
                    break;
            }
        }
        void _ProcessEmotion(string emotion)
        {
            FacialAnimation targetEmo = m_Emotion.emotions.FirstOrDefault(emo => emo.emotion == emotion);
            
            if (targetEmo != null)
            {
                m_EmoIcon.sprite = targetEmo.icon;
            }
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


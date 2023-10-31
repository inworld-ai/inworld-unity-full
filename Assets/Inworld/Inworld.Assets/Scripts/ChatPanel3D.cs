/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Packet;
using Inworld.Entities;
using Inworld.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Inworld.Assets
{
    public class ChatPanel3D : MonoBehaviour
    {
        [SerializeField] RectTransform m_ContentRT;
        [SerializeField] ChatBubble m_BubbleLeft;
        [SerializeField] ChatBubble m_BubbleRight;
        [SerializeField] InworldFacialEmotion m_Emotion;
        [SerializeField] TMP_Text m_Relation;
        [SerializeField] Image m_EmoIcon;
        readonly protected Dictionary<string, ChatBubble> m_Bubbles = new Dictionary<string, ChatBubble>();
        protected string m_CurrentEmotion;
        [SerializeField] InworldCharacter m_Character;
        void OnEnable()
        {
            InworldController.Instance.OnCharacterInteraction += OnInteraction;
        }

        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnCharacterInteraction -= OnInteraction;
        }
        protected void OnInteraction(InworldPacket incomingPacket)
        {
            if (!m_Character ||
                string.IsNullOrEmpty(m_Character.ID) ||
                (incomingPacket?.routing?.source?.name != m_Character.ID && incomingPacket?.routing?.target?.name != m_Character.ID))
                return;
            
            switch (incomingPacket)
            {
                case ActionPacket actionPacket:
                    HandleAction(actionPacket);
                    break;
                case RelationPacket relationPacket:
                    HandleRelation(relationPacket);
                    break;
                case TextPacket textPacket:
                    HandleText(textPacket);
                    break;
                case EmotionPacket emotionPacket:
                    HandleEmotion(emotionPacket);
                    break;
                default:
                    break;
            }
        }
        void HandleRelation(RelationPacket packet)
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
        void HandleAction(ActionPacket packet)
        {
            if (packet.action == null || packet.action.narratedAction == null || string.IsNullOrWhiteSpace(packet.action.narratedAction.content))
                return;
            m_Bubbles[packet.packetId.utteranceId] = Instantiate(m_BubbleLeft, m_ContentRT);
            InworldCharacterData charData = InworldController.CharacterHandler.GetCharacterDataByID(packet.routing.source.name);
            if (charData != null)
            {
                string charName = charData.givenName ?? "Character";
                string title = $"{charName}: {m_CurrentEmotion}";
                Texture2D thumbnail = charData.thumbnail ? charData.thumbnail : InworldAI.DefaultThumbnail;
                m_Bubbles[packet.packetId.utteranceId].SetBubble(title, thumbnail);
            }
            m_Bubbles[packet.packetId.utteranceId].Text = $"<i>{packet.action.narratedAction.content}</i>";
            _SetContentHeight(m_ContentRT, m_BubbleRight);
        }
        void HandleEmotion(EmotionPacket emotionPacket)
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
        protected virtual void HandleText(TextPacket packet)
        {
            if (packet.text == null || string.IsNullOrEmpty(packet.text.text) || string.IsNullOrWhiteSpace(packet.text.text))
                return;
            if (packet.routing?.source?.name != m_Character.ID && packet.routing?.target?.name != m_Character.ID) // Not Related
                return;
            switch (packet.routing.source.type.ToUpper())
            {
                case "AGENT":
                    if (!m_Bubbles.ContainsKey(packet.packetId.utteranceId))
                    {
                        m_Bubbles[packet.packetId.utteranceId] = Instantiate(m_BubbleLeft, m_ContentRT);
                        InworldCharacterData charData = InworldController.CharacterHandler.GetCharacterDataByID(packet.routing.source.name);
                        if (charData != null)
                        {
                            string charName = charData.givenName ?? "Character";
                            string title = $"{charName}: {m_CurrentEmotion}";
                            Texture2D thumbnail = charData.thumbnail ? charData.thumbnail : InworldAI.DefaultThumbnail;
                            m_Bubbles[packet.packetId.utteranceId].SetBubble(title, thumbnail);
                        }
                    }
                    break;
                case "PLAYER":
                    if (!m_Bubbles.ContainsKey(packet.packetId.utteranceId))
                    {
                        m_Bubbles[packet.packetId.utteranceId] = Instantiate(m_BubbleRight, m_ContentRT);
                        m_Bubbles[packet.packetId.utteranceId].SetBubble(InworldAI.User.Name, InworldAI.DefaultThumbnail);
                    }
                    break;
            }
            m_Bubbles[packet.packetId.utteranceId].Text = packet.text.text;
            _SetContentHeight(m_ContentRT, m_BubbleRight);
        }
        void _SetContentHeight(RectTransform scrollAnchor, InworldUIElement element)
        {
            scrollAnchor.sizeDelta = new Vector2(m_ContentRT.sizeDelta.x, scrollAnchor.childCount * element.Height);
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


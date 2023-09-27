using System.Collections.Generic;
using Inworld;
using Inworld.UI;
using Inworld.Packet;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Inworld
{
    public class PlayerController2D : PlayerController
    {
        [SerializeField] RectTransform m_CharContentAnchor;
        [SerializeField] RectTransform m_BubbleContentAnchor;
        [SerializeField] CharacterButton m_CharSelectorPrefab;
        [SerializeField] ChatBubble m_BubbleLeftPrefab;
        [SerializeField] ChatBubble m_BubbleRightPrefab;
        readonly Dictionary<string, CharacterButton> m_Characters = new Dictionary<string, CharacterButton>();
        readonly protected Dictionary<string, ChatBubble> m_Bubbles = new Dictionary<string, ChatBubble>();

        protected override void Start()
        {
            if (m_PushToTalk)
            {
                m_CharacterHandler.ManualAudioHandling = true;
                m_AudioCapture.AutoPush = false;
            }
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            InworldController.Instance.OnCharacterRegistered += OnCharacterRegistered;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnCharacterRegistered -= OnCharacterRegistered;
        }
        
        protected virtual void OnCharacterRegistered(InworldCharacterData charData)
        {
            if (!m_Characters.ContainsKey(charData.brainName))
                m_Characters[charData.brainName] = Instantiate(m_CharSelectorPrefab, m_CharContentAnchor);
            m_Characters[charData.brainName].SetData(charData);
            _SetContentHeight(m_CharContentAnchor, m_CharSelectorPrefab);
        }
        
        protected override void HandleText(TextPacket packet)
        {
            if (packet.text == null || string.IsNullOrEmpty(packet.text.text) || string.IsNullOrWhiteSpace(packet.text.text))
                return;
            switch (packet.routing.source.type.ToUpper())
            {
                case "AGENT":
                    if (!m_Bubbles.ContainsKey(packet.packetId.utteranceId))
                    {
                        m_Bubbles[packet.packetId.utteranceId] = Instantiate(m_BubbleLeftPrefab, m_BubbleContentAnchor);
                        InworldCharacterData charData = InworldController.Instance.GetCharacter(packet.routing.source.name);
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
                        m_Bubbles[packet.packetId.utteranceId] = Instantiate(m_BubbleRightPrefab, m_BubbleContentAnchor);
                        m_Bubbles[packet.packetId.utteranceId].SetBubble(InworldAI.User.Name, InworldAI.DefaultThumbnail);
                    }
                    break;
            }
            m_Bubbles[packet.packetId.utteranceId].Text = packet.text.text;
            _SetContentHeight(m_BubbleContentAnchor, m_BubbleRightPrefab);
        }
        
        void _SetContentHeight(RectTransform scrollAnchor, InworldUIElement element)
        {
            scrollAnchor.sizeDelta = new Vector2(m_BubbleContentAnchor.sizeDelta.x, scrollAnchor.childCount * element.Height);
        }
    }
}


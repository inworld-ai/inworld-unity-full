/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Entities;
using Inworld.Packet;
using Inworld.UI;
using System;
using UnityEngine;


namespace Inworld.Sample
{
    [Serializable]
    public struct ChatOptions
    {
        public bool audio;
        public bool text;
        public bool emotion;
        public bool narrativeAction;
        public bool relation;
        public bool trigger;
        public bool longBubbleMode;
    }
    public class ChatPanel : BubblePanel
    {
        [SerializeField] protected ChatBubble m_BubbleLeft;
        [SerializeField] protected ChatBubble m_BubbleRight;
        [SerializeField] protected ChatOptions m_ChatOptions;
        public override bool IsUIReady => base.IsUIReady && m_BubbleLeft && m_BubbleRight;
        
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
        
        protected virtual void OnInteraction(InworldPacket incomingPacket)
        {
            switch (incomingPacket)
            {
                case ActionPacket actionPacket:
                    HandleAction(actionPacket);
                    break;
                case TextPacket textPacket:
                    HandleText(textPacket);
                    break;
                case EmotionPacket emotionPacket:
                    HandleEmotion(emotionPacket);
                    break;
                case CustomPacket customPacket:
                    HandleTrigger(customPacket);
                    break;
                case RelationPacket relationPacket:
                    HandleRelation(relationPacket);
                    break;
                case AudioPacket audioPacket: 
                    HandleAudio(audioPacket);
                    break;
                case ControlPacket controlEvent:
                    HandleControl(controlEvent);
                    break;
                default:
                    InworldAI.LogWarning($"Received unknown {incomingPacket.type}");
                    break;
            }
        }
        protected virtual void HandleAudio(AudioPacket audioPacket)
        {
            // Already Played.
        }
        protected virtual void HandleControl(ControlPacket controlEvent)
        {
            // Not process in the global chat panel.
        }
        protected virtual void HandleRelation(RelationPacket relationPacket)
        {
            if (!m_ChatOptions.relation || relationPacket.debugInfo == null || relationPacket.debugInfo.relation == null || string.IsNullOrWhiteSpace(relationPacket.Relation) || !IsUIReady)
                return;
            InworldCharacterData charData = InworldController.CharacterHandler.GetCharacterDataByID(relationPacket.routing.source.name);
            if (charData == null)
                return;
            string key = m_ChatOptions.longBubbleMode ? relationPacket.packetId.interactionId : relationPacket.packetId.utteranceId;
            string charName = charData.givenName ?? "Character";
            Texture2D thumbnail = charData.thumbnail ? charData.thumbnail : InworldAI.DefaultThumbnail;
            string content = relationPacket.Relation;
            InsertBubble(key, m_BubbleLeft, charName, m_ChatOptions.longBubbleMode, content, thumbnail);
        }
        protected virtual void HandleTrigger(CustomPacket customPacket)
        {
            if (!m_ChatOptions.trigger || customPacket.custom == null || !IsUIReady)
                return;
            InworldCharacterData charData = InworldController.CharacterHandler.GetCharacterDataByID(customPacket.routing.source.name);
            if (charData == null)
                return;
            string key = m_ChatOptions.longBubbleMode ? customPacket.packetId.interactionId : customPacket.packetId.utteranceId;
            string charName = charData.givenName ?? "Character";
            Texture2D thumbnail = charData.thumbnail ? charData.thumbnail : InworldAI.DefaultThumbnail;
            string content = $"(Received: {customPacket.Trigger})";
            InsertBubble(key, m_BubbleLeft, charName, m_ChatOptions.longBubbleMode, content, thumbnail);
        }
        protected virtual void HandleEmotion(EmotionPacket emotionPacket)
        {
            // Not process in the global chat panel.
        }
        protected virtual void HandleText(TextPacket textPacket)
        {
            if (!m_ChatOptions.text || textPacket.text == null || string.IsNullOrWhiteSpace(textPacket.text.text) || !IsUIReady)
                return;
            string key = "";
            switch (textPacket.routing.source.type.ToUpper())
            {
                case "AGENT":
                    InworldCharacterData charData = InworldController.CharacterHandler.GetCharacterDataByID(textPacket.routing.source.name);
                    if (charData != null)
                    {
                        key = m_ChatOptions.longBubbleMode ? textPacket.packetId.interactionId : textPacket.packetId.utteranceId;
                        string charName = charData.givenName ?? "Character";
                        Texture2D thumbnail = charData.thumbnail ? charData.thumbnail : InworldAI.DefaultThumbnail;
                        string content = textPacket.text.text;
                        InsertBubble(key, m_BubbleLeft, charName, m_ChatOptions.longBubbleMode, content, thumbnail);
                    }
                    break;
                case "PLAYER":
                    // YAN: Player Input does not apply longBubbleMode.
                    //      And Key is always utteranceID.
                    key = textPacket.packetId.utteranceId;
                    InsertBubble(key, m_BubbleRight, InworldAI.User.Name, false, textPacket.text.text, InworldAI.DefaultThumbnail);
                    break;
            }
        }
        protected virtual void HandleAction(ActionPacket actionPacket)
        {
            if (!m_ChatOptions.narrativeAction || actionPacket.action == null || actionPacket.action.narratedAction == null || string.IsNullOrWhiteSpace(actionPacket.action.narratedAction.content) || !IsUIReady)
                return;
            InworldCharacterData charData = InworldController.CharacterHandler.GetCharacterDataByID(actionPacket.routing.source.name);
            if (charData == null)
                return;
            string key = m_ChatOptions.longBubbleMode ? actionPacket.packetId.interactionId : actionPacket.packetId.utteranceId;
            string charName = charData.givenName ?? "Character";
            Texture2D thumbnail = charData.thumbnail ? charData.thumbnail : InworldAI.DefaultThumbnail;
            string content = $"<i><color=#AAAAAA>{actionPacket.action.narratedAction.content}</color></i>";
            InsertBubble(key, m_BubbleLeft, charName, m_ChatOptions.longBubbleMode, content, thumbnail);
        }
    }
}

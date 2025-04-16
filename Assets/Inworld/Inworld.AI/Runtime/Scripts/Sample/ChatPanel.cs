/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Entities;
using Inworld.LLM;
using Inworld.LLM.Service;
using Inworld.Packet;
using Inworld.UI;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public bool task;
    }
    public class ChatPanel : BubblePanel
    {
        [SerializeField] protected ChatBubble m_BubbleLeft;
        [SerializeField] protected ChatBubble m_BubbleRight;
        [SerializeField] protected ChatOptions m_ChatOptions;
        public override bool IsUIReady => base.IsUIReady && m_BubbleLeft && m_BubbleRight;
        
        void OnEnable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Client.OnPacketSent += OnInteraction;
            InworldController.LLM.onChatHistoryUpdated.AddListener(OnChatUpdated);
            InworldController.CharacterHandler.Event.onCharacterListJoined.AddListener(OnCharacterJoined);
            InworldController.CharacterHandler.Event.onCharacterListLeft.AddListener(OnCharacterLeft);
        }

        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Client.OnPacketSent -= OnInteraction;
            InworldController.LLM.onChatHistoryUpdated.RemoveListener(OnChatUpdated);
            InworldController.CharacterHandler.Event.onCharacterListJoined.RemoveListener(OnCharacterJoined);
            InworldController.CharacterHandler.Event.onCharacterListLeft.RemoveListener(OnCharacterLeft);
        }

        protected virtual void OnCharacterJoined(InworldCharacter character)
        {
            // YAN: Clear existing event listener to avoid adding multiple times.
            character.Event.onPacketReceived.RemoveListener(OnInteraction); 
            character.Event.onPacketReceived.AddListener(OnInteraction);
        }

        protected virtual void OnCharacterLeft(InworldCharacter character)
        {
            character.Event.onPacketReceived.RemoveListener(OnInteraction); 
        }
        protected virtual void OnChatUpdated()
        {
            List<Message> chatHistories = InworldController.LLM.ChatHistory;
            foreach (Message history in chatHistories)
            {
                string hash = history.ToHash;
                ChatBubble bubble = history.role == MessageRole.MESSAGE_ROLE_USER ? m_BubbleRight : m_BubbleLeft;
                Texture2D thumbnail = history.role == MessageRole.MESSAGE_ROLE_USER ? InworldAI.User.Thumbnail : InworldAI.Logo;
                InsertBubble(hash, bubble, history.Role, false, history.ToMessage, thumbnail);
            }
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
                case AudioPacket audioPacket: 
                    HandleAudio(audioPacket);
                    break;
                case ControlPacket controlEvent:
                    HandleControl(controlEvent);
                    break;
                case MutationPacket mutationPacket:
                    RemoveBubbles(mutationPacket);
                    break;
                case ItemOperationPacket itemOperationPacket:
                    // Do nothing
                    break;
                default:
                    InworldAI.LogWarning($"Received unknown {incomingPacket}");
                    break;
            }
        }
        protected virtual bool RemoveBubbles(MutationPacket mutationPacket)
        {
            CancelResponse response = new();
            if (mutationPacket?.mutation is RegenerateResponseEvent regenEvt)
                response.interactionId = regenEvt.regenerateResponse.interactionId;
            else if (mutationPacket?.mutation is CancelResponseEvent cancelEvt)
                response = cancelEvt.cancelResponses;
            RemoveBubbleByResponse(response);
            return true;
        }

        protected virtual void RemoveBubbleByResponse(CancelResponse responseToRemove)
        {
            if (string.IsNullOrEmpty(responseToRemove?.interactionId))
                return;
            if (!m_ChatOptions.longBubbleMode)
            {
                responseToRemove.utteranceId?.ForEach(RemoveBubble);
                return;
            }
            m_Bubbles[responseToRemove.interactionId].RemoveUtterances(responseToRemove.utteranceId);
        }
        protected virtual void HandleAudio(AudioPacket audioPacket)
        {
            // Already Played.
        }
        protected virtual void HandleControl(ControlPacket controlEvent)
        {
            // Not process in the global chat panel.
        }
        protected virtual bool HandleRelation(CustomPacket relationPacket)
        {
            if (!m_ChatOptions.relation || !IsUIReady)
                return false;
            InworldCharacterData charData = InworldController.Client.GetCharacterDataByID(relationPacket.routing.source.name);
            if (charData == null)
                return false;
            string key = m_ChatOptions.longBubbleMode ? relationPacket.packetId.interactionId : relationPacket.packetId.utteranceId;
            string charName = charData.givenName ?? "Character";
            Texture2D thumbnail = charData.thumbnail ? charData.thumbnail : InworldAI.DefaultThumbnail;
            string content = relationPacket.custom.parameters.Aggregate(" ", (current, param) => current + $"{param.name}: {param.value} ");
            InsertBubbleWithPacketInfo(relationPacket.packetId, m_BubbleLeft, charName, m_ChatOptions.longBubbleMode, content, thumbnail);
            return true;
        }
        protected virtual bool HandleTask(CustomPacket taskPacket)
        {
            if (!m_ChatOptions.task || !IsUIReady)
                return false;
            string key = m_ChatOptions.longBubbleMode ? taskPacket.packetId.interactionId : taskPacket.packetId.utteranceId;
            List<TriggerParameter> parameters = new List<TriggerParameter>(taskPacket.custom.parameters);
            TriggerParameter taskIDParameter = parameters.Find(parameter => parameter.name == "task_id");
            if(taskIDParameter != null)
                parameters.Remove(taskIDParameter);
            string content;
            if (taskPacket.Source == SourceType.PLAYER)
                content = $"{taskPacket.custom.name}\n" + parameters.Aggregate("", (current, param) => current + $"{param.name}: {param.value}\n");
            else
                content = $"Received Task: {taskPacket.custom.name}\n" + parameters.Aggregate("", (current, param) => current + $"{param.name}: {param.value}\n");
            
            InsertBubbleWithPacketInfo(taskPacket.packetId, m_BubbleLeft, "Task", m_ChatOptions.longBubbleMode, $"<i><color=#AAAAAA>{content}</color></i>", InworldAI.DefaultThumbnail);
            return true;
        }
        protected virtual bool HandleTrigger(CustomPacket customPacket)
        {
            if (customPacket.Message == InworldMessage.RelationUpdate)
                HandleRelation(customPacket);
            else if (customPacket.Message == InworldMessage.Task)
                HandleTask(customPacket);
            if (!m_ChatOptions.trigger || customPacket.custom == null || !IsUIReady)
                return false;
            InworldCharacterData charData = InworldController.Client.GetCharacterDataByID(customPacket.routing.source.name);
            if (charData == null)
                return false;
            string key = m_ChatOptions.longBubbleMode ? customPacket.packetId.interactionId : customPacket.packetId.utteranceId;
            string charName = charData.givenName ?? "Character";
            Texture2D thumbnail = charData.thumbnail ? charData.thumbnail : InworldAI.DefaultThumbnail;
            if (string.IsNullOrEmpty(customPacket.TriggerName))
                return false;
            string content = $"(Received: {customPacket.Trigger})";
            InsertBubbleWithPacketInfo(customPacket.packetId, m_BubbleLeft, charName, m_ChatOptions.longBubbleMode, content, thumbnail);
            return true;
        }
        protected virtual void HandleEmotion(EmotionPacket emotionPacket)
        {
            // Not process in the global chat panel.
        }
        protected virtual bool HandleText(TextPacket textPacket)
        {
            if (!m_ChatOptions.text || textPacket.text == null || string.IsNullOrWhiteSpace(textPacket.text.text) || !IsUIReady)
                return false;
            string key = "";
            switch (textPacket.Source)
            {
                case SourceType.AGENT:
                {
                    InworldCharacterData charData = InworldController.Client.GetCharacterDataByID(textPacket.routing.source.name);
                    if (charData != null)
                    {
                        key = m_ChatOptions.longBubbleMode ? textPacket.packetId.interactionId : textPacket.packetId.utteranceId;
                        string charName = charData.givenName ?? "Character";
                        Texture2D thumbnail = charData.thumbnail ? charData.thumbnail : InworldAI.DefaultThumbnail;
                        string content = textPacket.text.text;
                        InsertBubbleWithPacketInfo(textPacket.packetId, m_BubbleLeft, charName, m_ChatOptions.longBubbleMode, content, thumbnail);
                    }
                    break;
                }
                case SourceType.PLAYER:
                    // YAN: Player Input does not apply longBubbleMode.
                    //      And Key is always utteranceID.
                    key = textPacket.packetId.utteranceId;
                    InsertBubbleWithPacketInfo(textPacket.packetId, m_BubbleRight, InworldAI.User.Name, false, textPacket.text.text, InworldAI.User.Thumbnail);
                    break;
            }
            return true;
        }
        protected virtual bool HandleAction(ActionPacket actionPacket)
        {
            if (!m_ChatOptions.narrativeAction || actionPacket.action == null || actionPacket.action.narratedAction == null || string.IsNullOrWhiteSpace(actionPacket.action.narratedAction.content) || !IsUIReady)
                return false;

            switch (actionPacket.routing.source.type)
            {
                case SourceType.AGENT:
                    InworldCharacterData charData = InworldController.Client.GetCharacterDataByID(actionPacket.routing.source.name);
                    if (charData == null)
                        return false;
                    string key = m_ChatOptions.longBubbleMode ? actionPacket.packetId.interactionId : actionPacket.packetId.utteranceId;
                    string charName = charData.givenName ?? "Character";
                    Texture2D thumbnail = charData.thumbnail ? charData.thumbnail : InworldAI.DefaultThumbnail;
                    string content = $"<i><color=#AAAAAA>{actionPacket.action.narratedAction.content}</color></i>";
                    InsertBubbleWithPacketInfo(actionPacket.packetId, m_BubbleLeft, charName, m_ChatOptions.longBubbleMode, content, thumbnail);
                    break;
                case SourceType.PLAYER:
                    // YAN: Player Input does not apply longBubbleMode.
                    //      And Key is always utteranceID.
                    key = actionPacket.packetId.utteranceId;
                    content = $"<i><color=#AAAAAA>{actionPacket.action.narratedAction.content}</color></i>";
                    InsertBubbleWithPacketInfo(actionPacket.packetId, m_BubbleRight, InworldAI.User.Name, false, content, InworldAI.DefaultThumbnail);
                    break;
            }
            return true;
        }
    }
}

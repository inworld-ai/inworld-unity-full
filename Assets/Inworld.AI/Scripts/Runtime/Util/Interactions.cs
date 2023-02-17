/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Collections;
using Inworld.Packets;
using Inworld.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
namespace Inworld
{
    public class InteractionEvent : UnityEvent<InteractionStatus, List<HistoryItem>> {}

    /**
     * Controls per-agent interaction history.
     * It will only show last historySize utterances that where marked as played.
     */
    public class Interactions : MonoBehaviour
    {
        protected InworldCharacter Character { get; set; }

        #region Variables & Properties
        [SerializeField] int m_HistorySize = 16;
        readonly LinkedList<HistoryItem> m_ChatHistory = new LinkedList<HistoryItem>();
        LimitedSizeDictionary<string, HistoryItem> m_ChatHistoryByUtteranceID;
        LimitedSizeDictionary<string, bool> m_PlayedUtterances;
        LimitedSizeDictionary<string, bool> m_CanceledInteractions;
        PacketId m_CurrentUtteranceID;
        List<HistoryItem> History => m_ChatHistory.Where(x => !x.IsAgent || m_PlayedUtterances.ContainsKey(x.UtteranceId)).Take(m_HistorySize).ToList();
        internal bool isSpeaking;
        #endregion


        #region MonoBehavior
        void Awake()
        {
            Init();
        }
        void OnEnable()
        {
            InworldController.Instance.OnPacketReceived += OnPacketEvents;
        }
        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnPacketReceived -= OnPacketEvents;
        }
        #endregion

        #region Private Functions
        protected virtual void Init()
        {
            Character ??= GetComponent<InworldCharacter>();
            int collectionsSize = m_HistorySize + 64;
            m_ChatHistoryByUtteranceID ??= new LimitedSizeDictionary<string, HistoryItem>(collectionsSize);
            m_PlayedUtterances ??= new LimitedSizeDictionary<string, bool>(collectionsSize);
            m_CanceledInteractions ??= new LimitedSizeDictionary<string, bool>(collectionsSize);
            Character.OnCharacterSpeaks ??= new UnityEvent<string, string>();
            Character.OnFinishedSpeaking ??= new UnityEvent();
            Character.OnBeginSpeaking ??= new UnityEvent();
        }
        protected virtual void OnPacketEvents(InworldPacket packet)
        {
            if (!Character)
                return;
            if (packet.Routing.Target.Id != Character.ID && packet.Routing.Source.Id != Character.ID)
                return;
            switch (packet)
            {
                case TextEvent textEvent:
                    _HandleTextEvent(textEvent);
                    break;
                case ControlEvent controlEvent:
                    _AddInteractionEnd(controlEvent.PacketId.InteractionId);
                    break;
            }
        }
        /**
         * Signals that there wont be more interaction utterances.
         */
        void _AddInteractionEnd(string interactionId)
        {
            if (m_ChatHistory == null || m_ChatHistory.Count <= 0)
                return;
            HistoryItem lastHistoryItem = m_ChatHistory.FirstOrDefault(x => x.InteractionId == interactionId && x.IsAgent);
            if (lastHistoryItem == null || lastHistoryItem.UtteranceId == null)
                return;
            lastHistoryItem.Final = true;
            if (!m_PlayedUtterances.ContainsKey(lastHistoryItem.UtteranceId))
                return;
            // Already played utterance.
            CompleteInteraction(interactionId);
        }
        public virtual void AddText(TextEvent textEvent)
        {
            CancelResponsesEvent cancel = _AddText(textEvent);
            StartUtterance(textEvent.PacketId);
            if (cancel == null)
                return;
            if (Character)
                Character.SendEventToAgent(cancel);
        }
        protected CancelResponsesEvent _AddText(TextEvent text)
        {
            if (IsInteractionCanceled(text.PacketId.InteractionId))
            {
                Debug.Log($"Cancel followup: {text.PacketId.InteractionId}");
                return new CancelResponsesEvent
                (
                    text.PacketId.InteractionId,
                    new List<string>
                    {
                        text.PacketId.UtteranceId
                    }
                );
            }
            CancelResponsesEvent cancel = null;
            if (m_CurrentUtteranceID != null &&
                text.Routing.Source.IsPlayer() &&
                text.PacketId.InteractionId != m_CurrentUtteranceID.InteractionId)
            {
                // Canceling current interaction
                List<string> canceledUtterances = new List<string>();


                foreach (HistoryItem historyItem in m_ChatHistory.Reverse())
                {
                    if (historyItem.UtteranceId != m_CurrentUtteranceID.UtteranceId ||
                        !historyItem.IsAgent ||
                        historyItem.InteractionId != m_CurrentUtteranceID.InteractionId)
                        continue;
                    m_ChatHistory.Remove(historyItem);
                    canceledUtterances.Add(historyItem.UtteranceId);
                }
                if (canceledUtterances.Count > 0)
                    cancel = new CancelResponsesEvent(m_CurrentUtteranceID.InteractionId, canceledUtterances);
                m_CanceledInteractions.Add(m_CurrentUtteranceID.InteractionId, true);
                m_CurrentUtteranceID = null;
            }
            HistoryItem existedUtterance = FindChatHistoryItemByUtteranceId(text.PacketId?.UtteranceId);
            if (existedUtterance == null)
            {
                InsertChatHistoryItem(new HistoryItem(text));
            }
            else
            {
                existedUtterance.Event = text;
                OnChatHistoryListChanged();
            }

            return cancel;
        }
        protected bool IsInteractionCanceled(string interactionId)
        {
            return m_CanceledInteractions.ContainsKey(interactionId);
        }

        protected void StartUtterance(PacketId packetId)
        {
            m_CurrentUtteranceID = packetId;
            m_PlayedUtterances.Add(packetId.UtteranceId, true);
            OnChatHistoryListChanged();
        }
        protected void CompleteUtterance(PacketId packetId)
        {
            HistoryItem utteranceItem = FindChatHistoryItemByUtteranceId(packetId.UtteranceId);
            if (utteranceItem != null && utteranceItem.Final)
            {
                CompleteInteraction(packetId.InteractionId);
            }

            m_CurrentUtteranceID = null;
        }
        void InsertChatHistoryItem(HistoryItem item)
        {
            m_ChatHistoryByUtteranceID.Add(item.UtteranceId, item);
            m_ChatHistory.AddFirst(item);
            // Cleaning history
            int limit = m_HistorySize + 16;
            if (m_ChatHistory.Count > limit)
            {
                m_ChatHistory.RemoveLast();
            }
            OnChatHistoryListChanged();
        }
        public virtual void Clear()
        {
            m_ChatHistoryByUtteranceID.Clear();
            m_PlayedUtterances.Clear();
            m_ChatHistory.Clear();
            OnChatHistoryListChanged();
        }
        protected void OnChatHistoryListChanged()
        {
            if (Character)
                Character.InteractionEvent.Invoke(InteractionStatus.HistoryChanged, History);
        }
        void _HandleTextEvent(TextEvent textEvent)
        {
            if (textEvent?.PacketId?.InteractionId == null || textEvent.PacketId?.UtteranceId == null)
                return;
            if (Character)
            {
                if (textEvent.Routing.Source.IsAgent())
                {
                    Character.IsSpeaking = true;
                    Character.OnCharacterSpeaks?.Invoke(Character.CharacterName, textEvent.Text);
                }
                if (textEvent.Routing.Target.IsAgent())
                    Character.OnCharacterSpeaks?.Invoke("Player", textEvent.Text);
            }
            AddText(textEvent);
        }
        HistoryItem FindChatHistoryItemByUtteranceId(string utteranceId)
        {
            return m_ChatHistoryByUtteranceID.ContainsKey(utteranceId) ? m_ChatHistoryByUtteranceID[utteranceId] : null;
        }
        protected void CompleteInteraction(string interactionId)
        {
            Character.IsSpeaking = false;
            List<HistoryItem> itemsByInteraction = History
                                                   .Where(x => x.InteractionId == interactionId).ToList();
            if (Character)
                Character.InteractionEvent.Invoke(InteractionStatus.InteractionCompleted, itemsByInteraction);
        }
        #endregion
    }
}

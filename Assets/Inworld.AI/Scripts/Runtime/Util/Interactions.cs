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
    class Interactions
    {
        #region Variables & Properties
        readonly int m_HistorySize;
        readonly LinkedList<HistoryItem> m_ChatHistory = new LinkedList<HistoryItem>();
        readonly LimitedSizeDictionary<string, HistoryItem> m_ChatHistoryByUtteranceID;
        readonly LimitedSizeDictionary<string, bool> m_PlayedUtterances;
        readonly LimitedSizeDictionary<string, bool> m_CanceledInteractions;
        PacketId m_CurrentUtteranceID;
        public InteractionEvent Event { get; }
        List<HistoryItem> History => m_ChatHistory.Where(x => !x.IsAgent || m_PlayedUtterances.ContainsKey(x.UtteranceId)).Take(m_HistorySize).ToList();
        #endregion

        #region Private Functions
        internal Interactions(int historySize)
        {
            m_HistorySize = historySize;
            int collectionsSize = m_HistorySize + 64;
            m_ChatHistoryByUtteranceID = new LimitedSizeDictionary<string, HistoryItem>(collectionsSize);
            m_PlayedUtterances = new LimitedSizeDictionary<string, bool>(collectionsSize);
            m_CanceledInteractions = new LimitedSizeDictionary<string, bool>(collectionsSize);
            Event = new InteractionEvent();
        }
        /**
         * Signals that there wont be more interaction utterances.
         */
        internal void AddInteractionEnd(string interactionId)
        {
            if (m_ChatHistory == null || m_ChatHistory.Count <= 0)
                return;
            HistoryItem lastHistoryItem = m_ChatHistory.FirstOrDefault(x => x.InteractionId == interactionId && x.IsAgent);
            if (lastHistoryItem == null || lastHistoryItem.UtteranceId == null)
                return;
            lastHistoryItem.Final = true;
            if (m_PlayedUtterances.ContainsKey(lastHistoryItem.UtteranceId) && (m_CurrentUtteranceID == null || m_CurrentUtteranceID.UtteranceId != lastHistoryItem.UtteranceId))
            {
                // Already played utterance.
                CompleteInteraction(interactionId);
            }
        }
        internal CancelResponsesEvent AddText(TextEvent text)
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
        internal bool IsInteractionCanceled(string interactionId)
        {
            return m_CanceledInteractions.ContainsKey(interactionId);
        }
        internal void StartUtterance(PacketId packetId)
        {
            m_CurrentUtteranceID = packetId;
            m_PlayedUtterances.Add(packetId.UtteranceId, true);
            OnChatHistoryListChanged();
        }
        internal void CompleteUtterance(PacketId packetId)
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
        internal void Clear()
        {
            m_ChatHistoryByUtteranceID.Clear();
            m_PlayedUtterances.Clear();
            m_ChatHistory.Clear();
            OnChatHistoryListChanged();
        }
        void OnChatHistoryListChanged()
        {
            Event.Invoke(InteractionStatus.HistoryChanged, History);
        }
        HistoryItem FindChatHistoryItemByUtteranceId(string utteranceId)
        {
            return m_ChatHistoryByUtteranceID.ContainsKey(utteranceId) ? m_ChatHistoryByUtteranceID[utteranceId] : null;
        }
        internal void CompleteInteraction(string interactionId)
        {
            InworldAI.Log("" + interactionId + " Finish!");
            List<HistoryItem> itemsByInteraction = History
                                                   .Where(x => x.InteractionId == interactionId).ToList();
            Event.Invoke(InteractionStatus.InteractionCompleted, itemsByInteraction);
        }
        #endregion
    }
}

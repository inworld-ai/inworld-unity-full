/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Packet;
using System;


namespace Inworld.Interactions
{
    public class Interaction : IContainable
    {
        public string ID { get; set; }
        public DateTime RecentTime { get; set; }
        internal Utterance CurrentUtterance { get; set; }
        readonly IndexQueue<Utterance> m_Prepared;
        readonly IndexQueue<Utterance> m_Processed;
        readonly IndexQueue<Utterance> m_Cancelled;

        public Interaction(InworldPacket packet)
        {
            ID = packet?.packetId?.interactionId;
            m_Prepared = new IndexQueue<Utterance>();
            m_Processed = new IndexQueue<Utterance>();
            m_Cancelled = new IndexQueue<Utterance>();
            RecentTime = InworldDateTime.ToDateTime(packet?.timestamp);
            m_Prepared.Add(packet);
        }
        /// <summary>
        /// Add the packet to the store data.
        /// If it's over due and it's not a trigger, discard.
        /// If it's in time, dispatch immediately.
        /// Otherwise stored in cache.
        /// </summary>
        /// <param name="packet">packet to add.</param>
        /// <returns>If it needs to dispatch immediately, will return, otherwise return null.</returns>
        public void Add(InworldPacket packet)
        {
            if (m_Processed.IsOverDue(packet) || m_Processed.Contains(packet))
            {
                m_Processed.Add(packet);
                return; //YAN: As will return null if it's not trigger.
            }
            if (CurrentUtterance != null && CurrentUtterance.ID == packet?.packetId?.utteranceId)
            {
                CurrentUtterance.Add(packet);
                return;
            }
            m_Prepared.Add(packet);
        }
        public Utterance Dequeue()
        {
            Utterance nextUtterance = m_Prepared.Dequeue(true);
            if (nextUtterance == null)
                return null;
            m_Processed.Enqueue(nextUtterance);
            return nextUtterance;
        }
        public bool IsEmpty => (m_Prepared == null || m_Prepared.IsEmpty) && (CurrentUtterance == null || CurrentUtterance.IsEmpty);
        public bool Contains(InworldPacket packet) => packet?.packetId?.interactionId == ID;
        
        public void Cancel(bool isHardCancelling = true)
        {
            if (isHardCancelling && CurrentUtterance != null)
            {
                CurrentUtterance.Cancel();
                m_Cancelled.Enqueue(CurrentUtterance);
            }
            m_Prepared.PourTo(m_Cancelled);
        }
        public void OnDequeue()
        {
            // YAN: You can add your callback here when it's dequeuing.
        }
    }
}

/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Packet;
using System;
using UnityEngine;


namespace Inworld.Interactions
{
    public class Interaction : IContainable
    {
        public string ID { get; set; }
        public DateTime RecentTime { get; set; }
        public bool Interruptible { get; set; } = true;
        public bool ReceivedInteractionEnd { get; set; }
        internal Utterance CurrentUtterance { get; set; }
        public bool IsEmpty => (m_Prepared == null || m_Prepared.IsEmpty) && (CurrentUtterance == null || CurrentUtterance.IsEmpty);
        
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
            if (packet is ControlPacket controlPacket && controlPacket.Action == ControlType.INTERACTION_END)
                ReceivedInteractionEnd = true;
            if (packet is CustomPacket customPacket && customPacket.Message == InworldMessage.Uninterruptible)
                Interruptible = false;
            if (m_Processed.IsOverDue(packet) || m_Processed.Contains(packet))
            {
                if (packet is TextPacket || packet is CustomPacket)
                    m_Prepared.Add(packet);
                else
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
            return nextUtterance;
        }
        public void Processed()
        {
            if (CurrentUtterance == null)
                return;
            m_Processed.Enqueue(CurrentUtterance);
            CurrentUtterance = null;
        }

        public bool Contains(InworldPacket packet) => packet?.packetId?.interactionId == ID;
        
        public void Cancel(bool isHardCancelling = true)
        {
            if (isHardCancelling && CurrentUtterance != null)
            {
                m_Cancelled.Enqueue(CurrentUtterance);
                CurrentUtterance = null;
            }
            m_Prepared.PourTo(m_Cancelled);
        }
        public bool OnDequeue()
        {
            // YAN: You can add your callback here when it's dequeuing.
            return true;
        }
    }
}

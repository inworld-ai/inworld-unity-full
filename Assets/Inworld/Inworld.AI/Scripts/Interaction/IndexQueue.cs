/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Packet;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Inworld.Interactions
{
    public interface IContainable
    {
        public string ID { get; set; }
        public DateTime RecentTime { get; set; }
        public bool IsEmpty { get; }
        public bool Contains(InworldPacket packet);
        public void Add(InworldPacket packet);
        public void Cancel(bool isHardCancelling = true);
        public void OnDequeue();
    }
    public class IndexQueue<T> where T : IContainable
    {
        readonly List<T> m_Elements = new List<T>();
        public DateTime RecentTime { get; private set; } = DateTime.MinValue;
        public int Count => m_Elements.Count;
        public bool Contains(InworldPacket packet) => m_Elements.Any(i => i.Contains(packet));
        public bool IsOverDue(InworldPacket packet) => RecentTime > InworldDateTime.ToDateTime(packet?.timestamp);
        public bool IsEmpty => m_Elements.Count == 0;
        public T this[int index] => index < m_Elements.Count ? m_Elements[index] : default;

        public void Add(InworldPacket packet)
        {
            if (packet == null || packet.packetId == null)
                return;
            DateTime packetTime = InworldDateTime.ToDateTime(packet.timestamp);
            RecentTime = packetTime > RecentTime ? packetTime : RecentTime;
            T i = m_Elements.FirstOrDefault(i => i.Contains(packet));
            if (i != null)
                i.Add(packet);
            else
            {
                T t = (T)Activator.CreateInstance(typeof(T), packet);
                m_Elements.Add(t);
            }
        }
        public void Clear() => m_Elements.Clear();
        
        public void Enqueue(T interaction)
        {
            RecentTime = interaction.RecentTime > RecentTime ? interaction.RecentTime : RecentTime;
            m_Elements.Add(interaction);
        }
        public T Dequeue(bool needCallback = false)
        {
            // YAN: No need to update RecentTime while dequeuing.
            if (m_Elements.Count == 0)
                return default;
            T result = m_Elements[0];
            if (needCallback)
                result.OnDequeue();
            m_Elements.Remove(result);
            return result;
        }

        public void PourTo(IndexQueue<T> rhs)
        {
            while (m_Elements.Count > 0)
            {
                rhs.Enqueue(Dequeue(true));
            }
        }
    }
}

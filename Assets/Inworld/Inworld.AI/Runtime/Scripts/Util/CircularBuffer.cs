/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/


using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inworld
{
    public class CircularBuffer<T>
    {
        public int lastPos = 0;
        public int currPos = 0;

        readonly List<T> m_Buffer;
        readonly int m_Size;
        
        public CircularBuffer(int size)
        {
            m_Buffer = new List<T>();
            for (int i = 0; i < size; i++)
                m_Buffer.Add(default);
            m_Size = size;
        }

        public void Clear()
        {
            for (int i = 0; i < m_Size; i++)
            {
                m_Buffer[i] = default;
            }
        }
        public void Enqueue(List<T> objs)
        {
            lastPos = currPos;
            int nIndex = lastPos;
            for (int i = 0; i < objs.Count; i++)
            {
                nIndex = (lastPos + i) % m_Size;
                m_Buffer[nIndex] = objs[i];
            }
            currPos = (nIndex + 1) % m_Size; 
        }
        public List<T> ToList() => m_Buffer;

        public List<T> GetRange(int start, int end)
        {
            List<T> objs = new List<T>();
            if (start < 0 || start >= m_Size || end < 0 || end >= m_Size)
                return objs;
            if (end < start)
            {
                objs.AddRange(m_Buffer.GetRange(start, m_Buffer.Count - start));
                objs.AddRange(m_Buffer.GetRange(0, end));
            }
            else if (end > start)
            {
                objs.AddRange(m_Buffer.GetRange(start, currPos - start));
            }
            return objs;
        }
        public List<T> Dequeue()
        {
            List<T> objs = new List<T>();
            if (currPos < lastPos)
            {
                objs.AddRange(m_Buffer.GetRange(lastPos, m_Buffer.Count - lastPos));
                objs.AddRange(m_Buffer.GetRange(0, currPos));
            }
            else if (currPos > lastPos)
            {
                objs.AddRange(m_Buffer.GetRange(lastPos, currPos - lastPos));
            }
            return objs;
        }
        public void Print()
        {
            foreach (T data in m_Buffer)
            {
                Debug.Log(data.ToString());
            }
        }
    }
}
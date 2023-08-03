using Inworld.Interactions;
using System.Collections.Generic;

namespace Inworld.NDK
{
    public class SharedAudioData
    {
        private readonly List<(float[], float)> m_Data = new List<(float[], float)>();
        private readonly object m_LockObj = new object();

        public void Add(float[] audioData, float time)
        {
            lock (m_LockObj)
            {
                m_Data.Add((audioData, time));

                // Clean up old data
                while (m_Data.Count > 0 && time - m_Data[0].Item2 > 1.0f)
                {
                    m_Data.RemoveAt(0);
                }
            }
        }

        public void Clear()
        {
            lock (m_LockObj)
            {
                m_Data.Clear();
            }
        }

        public List<(float[], float)> GetData()
        {
            lock (m_LockObj)
            {
                // Return a copy of the data
                // to avoid potential issues with external code modifying it.
                return new List<(float[], float)>(m_Data);
            }
        }
    }

}

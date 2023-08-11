using Inworld.Interactions;
using System.Collections.Generic;

namespace Inworld.NDK
{
    public class SharedAudioData
    {
        private readonly List<(string, float)> m_Data = new List<(string, float)>();

        public bool IsEmpty()
        {
            return m_Data.Count == 0;
        }
        
        public void Add(string audioData, float time)
        {
            lock (m_Data)
            {
                m_Data.Add((audioData, time));
            }

            lock (m_Data)
            {
                // Clean up old data
                while (m_Data.Count > 0 && time - m_Data[0].Item2 > 1.0f)
                {
                    m_Data.RemoveAt(0);
                }
            }
        }

        public void Clear()
        {
            lock (m_Data)
            {
                m_Data.Clear();
            }
        }

        public List<(string, float)> GetData()
        {
            // Return a copy of the data
            // to avoid potential issues with external code modifying it.
            return new List<(string, float)>(m_Data);
        }
        
        public List<short> GetDataAsShorts()
        {
            List<short> shortData = new List<short>();

            lock (m_Data)
            {
                for (int index = 0; index < m_Data.Count; index++)
                {
                    (string, float) tuple = m_Data[index];
                    string audioData = tuple.Item1;

                    for (int i = 0; i < audioData.Length; i++)
                    {
                        float sample = audioData[i];
                        short shortSample = (short)(sample * 32767);
                        shortData.Add(shortSample);
                    }
                }
            }

            return shortData;
        }
    }

}

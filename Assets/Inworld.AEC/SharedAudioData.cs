using Inworld.Interactions;
using System.Collections.Generic;

namespace Inworld
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
        
        public short[] GetDataAsShortArray()
        {
            int totalSize = 0;

            // Calculate the total size required for the array
            lock (m_Data)
            {
                foreach (var tuple in m_Data)
                {
                    totalSize += tuple.Item1.Length;
                }
            }

            short[] shortData = new short[totalSize];
            int position = 0;

            lock (m_Data)
            {
                foreach (var tuple in m_Data)
                {
                    float[] audioData = tuple.Item1;

                    for (int i = 0; i < audioData.Length; i++)
                    {
                        float sample = audioData[i];
                        short shortSample = (short)(sample * 32767);
                        shortData[position] = shortSample;
                        position++;
                    }
                }
            }

            return shortData;
        }

        
        public List<short> GetDataAsShorts()
        {
            List<short> shortData = new List<short>();

            lock (m_Data)
            {
                for (int index = 0; index < m_Data.Count; index++)
                {
                    (float[], float) tuple = m_Data[index];
                    float[] audioData = tuple.Item1;

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

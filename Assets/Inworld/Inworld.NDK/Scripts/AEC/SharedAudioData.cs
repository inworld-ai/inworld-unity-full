using Inworld.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inworld
{
    public class SharedAudioData
    {
        private readonly Dictionary<int, float> m_MixedData = new Dictionary<int, float>();
        private readonly object m_LockObj = new object();
        private int m_sampleRate;
        private float m_LastTimestamp = -1;

        public SharedAudioData(int sampleRate = 16000)
        {
            m_sampleRate = sampleRate;
        }
        
        public void Add(float[] audioData, float time)
        {
            lock (m_LockObj)
            {
                // Find the starting sample position for this audio data based on the timestamp.
                int startSample = (int)(time * m_sampleRate);

                for (int i = 0; i < audioData.Length; i++)
                {
                    int samplePos = startSample + i;
                    if (m_MixedData.ContainsKey(samplePos))
                    {
                        m_MixedData[samplePos] += audioData[i];
                    }
                    else
                    {
                        m_MixedData[samplePos] = audioData[i];
                    }
                }

                m_LastTimestamp = time;

                // Clean up old data
                int oldestSampleAllowed = (int)((time - 1.0f) * m_sampleRate);
                List<int> keysToRemove = m_MixedData.Keys.Where(k => k < oldestSampleAllowed).ToList();
                foreach (int key in keysToRemove)
                {
                    m_MixedData.Remove(key);
                }
            }
        }
        
        public short[] GetDataAsMixedShortArray()
        {
            if (!m_MixedData.Any())
                return Array.Empty<short>();

            // Convert the mixed data dictionary into a sorted list
            var sortedData = m_MixedData.OrderBy(pair => pair.Key).ToList();

            // Create a continuous array of shorts for the audio data
            int totalSamples = (int)((m_LastTimestamp + 1) * m_sampleRate); // +1 to account for potential rounding errors
            short[] shortData = new short[totalSamples];

            foreach (var pair in sortedData)
            {
                float sampleValue = Mathf.Clamp(pair.Value, -1.0f, 1.0f);  // prevent clipping
                shortData[pair.Key] = (short)(sampleValue * 32767);
            }
            
            return shortData;
        }
    }
}

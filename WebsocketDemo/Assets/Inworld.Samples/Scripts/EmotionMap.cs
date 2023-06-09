using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inworld.Sample
{
    [Serializable]
    public class EmotionMapData
    {
        public string name;
        public Emotion bodyEmotion;
        public Gesture bodyGesture;
        public FacialEmotion emoteAnimation;
        public FacialEmotion facialEmotion;
    }
    [Serializable]
    public class PhonemeToViseme
    {
        public string phoneme;
        public int visemeIndex;
    }
    [CreateAssetMenu(fileName = "Emotion Map", menuName = "Inworld/Emotion Map", order = 0)]
    public class EmotionMap : ScriptableObject
    {
        public List<EmotionMapData> data;
        public List<PhonemeToViseme> p2vMap;
        public EmotionMapData this[string emoName] => data.FirstOrDefault(e => e.name == emoName);
    }
}

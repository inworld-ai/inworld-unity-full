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
    [CreateAssetMenu(fileName = "Emotion Map", menuName = "Inworld/Emotion Map", order = 0)]
    public class EmotionMap : ScriptableObject
    {
        public List<EmotionMapData> data;
        public EmotionMapData this[string emoName] => data.FirstOrDefault(e => e.name == emoName);
    }
}

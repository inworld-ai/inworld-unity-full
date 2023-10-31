using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Inworld.Util
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
    public class EmotionMap : ScriptableObject
    {
        public List<EmotionMapData> data;
        public EmotionMapData this[string emoName] => data.FirstOrDefault(e => e.name == emoName);
    }
}

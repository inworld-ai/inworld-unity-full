/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inworld.Assets
{
    [Serializable]
    public class EmotionMapData
    {
        public SpaffCode name;
        public Emotion bodyEmotion;
        public Gesture bodyGesture;
        public FacialEmotion emoteAnimation;
        public FacialEmotion facialEmotion;
    }
    public class EmotionMap : ScriptableObject
    {
        public List<EmotionMapData> data;
        public EmotionMapData this[SpaffCode emoName] => data.FirstOrDefault(e => e.name == emoName);
    }
}

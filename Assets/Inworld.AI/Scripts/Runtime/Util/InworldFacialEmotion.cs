using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inworld.Util
{
    [Serializable]
    public class MorphState
    {
        public string morphName;
        public float morphWeight;
    }
    [Serializable]
    public class FacialAnimation
    {
        public string emotion;
        public Sprite icon;
        public List<MorphState> morphStates;
    }

    public class InworldFacialEmotion : ScriptableObject
    {
        public List<FacialAnimation> emotions;
    }
}

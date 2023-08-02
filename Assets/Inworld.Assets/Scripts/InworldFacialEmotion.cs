using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inworld.Assets
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
    [CreateAssetMenu(fileName = "FILENAME", menuName = "MENUNAME", order = 0)]
    public class InworldFacialEmotion : ScriptableObject
    {
        public List<FacialAnimation> emotions;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inworld.Util
{
    [Serializable]
    public class FaceTransform
    {
        public string name;
        public int animIndex;
        public float imgHeight;
        public Texture eyeBlow;
        public Texture eye;
        public Texture eyeClosed;
        public Texture nose;
        public Texture mouthDefault;
        public List<Texture> mouth;
    }
    public class InworldInnequinFacialEmotion : ScriptableObject
    {
        public List<FaceTransform> data;

        public FaceTransform this[string facialName] => data.FirstOrDefault(f => f.name == facialName);
    }
}

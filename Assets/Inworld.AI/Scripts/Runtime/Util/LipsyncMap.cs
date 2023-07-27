using System;
using System.Collections.Generic;
using UnityEngine;
namespace Inworld.Util
{

    [Serializable]
    public class PhonemeToViseme
    {
        public string phoneme;
        public int visemeIndex;
    }

    public class LipsyncMap : ScriptableObject
    {
        public List<PhonemeToViseme> p2vMap;
    }
}

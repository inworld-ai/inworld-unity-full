using System;
using UnityEngine;

namespace Inworld
{
    [CreateAssetMenu(fileName = "README", menuName = "Inworld/Read Me", order = 0)]
    public class Readme : ScriptableObject
    {
        public Font titleFont;
        public Font contentFont;
        public Texture2D icon;
        public string title;
        public Section[] sections;
        public bool loadedLayout;
	
        [Serializable]
        public class Section 
        {
            public string heading, text, linkText, url;
        }
    }
}


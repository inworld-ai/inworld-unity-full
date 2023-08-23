using System;
using UnityEngine;

namespace Inworld
{
    public class Readme : ScriptableObject
    {
        public Font titleFont;
        public Font contentFont;
        public Texture2D icon;
        public string title;
        public Section[] sections;
	
        [Serializable]
        public class Section 
        {
            public string heading, text, linkText, url;
        }
    }
}


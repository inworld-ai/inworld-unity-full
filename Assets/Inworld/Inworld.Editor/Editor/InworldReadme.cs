/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using UnityEngine;

namespace Inworld.UI
{
    public class InworldReadme : ScriptableObject
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


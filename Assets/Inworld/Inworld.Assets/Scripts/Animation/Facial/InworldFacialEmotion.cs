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
        public FacialAnimation this[string emoName] => emotions.FirstOrDefault(e => e.emotion == emoName);
    }
}

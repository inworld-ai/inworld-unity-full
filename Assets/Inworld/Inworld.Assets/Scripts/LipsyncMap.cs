/*************************************************************************************************
* Copyright 2022-2024 Theai, Inc. dba Inworld AI
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inworld.Assets
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

/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;
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
[Serializable]
public class PhonemeToViseme
{
    public string phoneme;
    public int visemeIndex;
}
public class FacialAnimationData : ScriptableObject
{
    public List<FacialAnimation> emotions;
    public List<PhonemeToViseme> p2vMap;
}

/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;
using UnityEngine;
namespace Inworld.Packet
{
    [Serializable]
    public class EmotionEvent
    {
        [HideInInspector] public float joy;
        [HideInInspector] public float fear;
        [HideInInspector] public float trust;
        [HideInInspector] public float surprise;
        public string behavior;
        public string strength;
        public override string ToString() => $"{strength} {behavior}";
    }
    [Serializable]
    public class EmotionPacket : InworldPacket
    {
        public EmotionEvent emotion;
        
        public EmotionPacket()
        {
            type = "EMOTION";
            emotion = new EmotionEvent();
        }
        public EmotionPacket(InworldPacket rhs, EmotionEvent evt) : base(rhs)
        {
            type = "EMOTION";
            emotion = evt;
        }
    }
}

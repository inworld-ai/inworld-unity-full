/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
        [JsonConverter(typeof(StringEnumConverter))]
        public SpaffCode behavior;
        [JsonConverter(typeof(StringEnumConverter))]
        public Strength strength;
        public override string ToString() => $"{strength} {behavior}";
    }
    [Serializable]
    public class EmotionPacket : InworldPacket
    {
        public EmotionEvent emotion;
        
        public EmotionPacket()
        {
            emotion = new EmotionEvent();
        }
        public EmotionPacket(InworldPacket rhs, EmotionEvent evt) : base(rhs)
        {
            emotion = evt;
        }
    }
}

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

namespace Inworld.Sample.Innequin
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
    public class FaceTransformData : ScriptableObject
    {
        public List<FaceTransform> data;
        public FaceTransform this[string facialName] => data.FirstOrDefault(f => f.name == facialName);
    }
}

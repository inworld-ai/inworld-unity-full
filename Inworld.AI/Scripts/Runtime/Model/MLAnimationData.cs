/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using System;
using System.Collections.Generic;
namespace Inworld.Model
{
    /// <summary>
    ///     These classes are used to receive ML animation from inworld server.
    ///     Which would be implemented in future
    /// </summary>
    [Serializable]
    public class AnimationFromAudioResponse
    {
        public string animation_format;
        public float fps;
        public Bone[] bones;
        public string timestamp;
    }

    [Serializable]
    public class Frame
    {
        public IKVector4 position;
        public IKVector4 rotation;
    }

    [Serializable]
    public class Bone
    {
        public string name;
        public List<Frame> frames;
    }
    [Serializable]
    public class IKVector4
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }
}

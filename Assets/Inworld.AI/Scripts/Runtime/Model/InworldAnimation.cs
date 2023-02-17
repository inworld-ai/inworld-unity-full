/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Grpc;
using Inworld.Util;
using UnityEngine;
namespace Inworld.Model
{
    /// <summary>
    ///     Interface for handling Emotion and Gesture Events from Inworld Server.
    /// </summary>
    public interface InworldAnimation
    {
        public Animator Animator { get; set; }
        public InworldCharacter Character { get; set; }
        public bool Init();
        public void HandleMainStatus(AnimMainStatus status);
        public void HandleEmotion(EmotionEvent.Types.SpaffCode spaffCode);
        public void HandleGesture(GestureEvent.Types.Type gesture);
    }
}

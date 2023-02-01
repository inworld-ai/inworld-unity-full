/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using UnityEngine;
namespace Inworld.Model
{
    /// <summary>
    ///     The interface of Eye Head Animation Loader.
    ///     Currently it's null, and only supports basic feature of looking at player.
    ///     Inworld has default support for `Realistic Eye Movements`
    ///     https://assetstore.unity.com/packages/tools/animation/realistic-eye-movements-29168
    ///     To implement this, you could do the followings:
    ///     1. purchase and download the package.
    ///     2. add `LookTargetController` and `EyeAndHeadAnimator` to Inworld Character Prefab
    ///     3. create a prefab with script inherit this interface,
    ///     GetComponent
    ///     <EyeAndHeadAnimator>
    ///         (),
    ///         let it call ImportFromJson(), with data stored at `Resources/Animations/`
    ///         4. put the prefab inside prefab GLTFAvatarLoader's head anim loader.
    ///         If you'd like to implement your own head animation,
    ///         you could also create a prefab with script inherit this interface, then put that prefab
    ///         inside GLTFAvatarLoader's head anim loader.
    /// </summary>
    public interface IEyeHeadAnimLoader
    {
        public void SetupHeadMovement(GameObject avatar);
    }
}

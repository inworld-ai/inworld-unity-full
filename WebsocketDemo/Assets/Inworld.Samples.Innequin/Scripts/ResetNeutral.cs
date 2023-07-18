/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using UnityEngine;
namespace Inworld.Animation
{
    /// <summary>
    ///     This script attached to Neutral gesture.
    ///     Neutral is the main status of animation. Whenever some states enters it,
    ///     it'll reset all the flags.
    /// </summary>
    public class ResetNeutral : StateMachineBehaviour
    {
        static readonly int s_Motion = Animator.StringToHash("MainStatus");

        /// <summary>
        ///     This function is called in animator, bound to State Idle.
        /// </summary>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetInteger(s_Motion, 0);
        }
    }
}

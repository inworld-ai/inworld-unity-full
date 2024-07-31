
using UnityEngine;
using UnityEngine.Animations;



public class ResetInteractionStatus : StateMachineBehaviour
{
    const int k_InteractionLayerIndex = 1;
    const int k_EmotionLayerIndex = 2;

    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash, AnimatorControllerPlayable controller)
    {
        Debug.Log("Hey Enter");
        animator.SetLayerWeight(k_InteractionLayerIndex, 1);
        animator.SetLayerWeight(k_EmotionLayerIndex, 0);
        base.OnStateMachineEnter(animator, stateMachinePathHash, controller);
    }
    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash, AnimatorControllerPlayable controller)
    {
        Debug.Log("Hey Exit");
        animator.SetLayerWeight(k_InteractionLayerIndex, 0);
        animator.SetLayerWeight(k_EmotionLayerIndex, 1);
        base.OnStateMachineExit(animator, stateMachinePathHash, controller);
    }
     
}

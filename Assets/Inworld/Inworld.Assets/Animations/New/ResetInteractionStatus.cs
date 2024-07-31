
using UnityEngine;
using UnityEngine.Animations;



public class ResetInteractionStatus : StateMachineBehaviour
{
    const int k_InteractionLayerIndex = 1;
    const int k_EmotionLayerIndex = 2;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetLayerWeight(k_InteractionLayerIndex, 1);
        animator.SetLayerWeight(k_EmotionLayerIndex, 0);
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetLayerWeight(k_InteractionLayerIndex, 0);
        animator.SetLayerWeight(k_EmotionLayerIndex, 1);
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }
}

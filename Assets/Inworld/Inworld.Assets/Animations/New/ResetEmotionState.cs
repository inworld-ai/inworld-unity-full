using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetEmotionState : StateMachineBehaviour
{
    public string m_EmotionName;
    int m_HashIndex;
    static readonly int s_EmotionChanged = Animator.StringToHash("EmotionChanged");
    void Awake()
    {
        m_HashIndex = Animator.StringToHash(m_EmotionName);
    }
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetFloat(m_HashIndex, 0);
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime >= 1.0f)
        {
            animator.SetTrigger(s_EmotionChanged);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetState : StateMachineBehaviour
{
    public string m_EmotionName;
    int m_HashIndex; 
    void Awake()
    {
        m_HashIndex = Animator.StringToHash(m_EmotionName);
    }
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetInteger(m_HashIndex, 0);
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }
}

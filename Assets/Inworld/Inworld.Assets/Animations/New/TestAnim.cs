using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAnim : MonoBehaviour
{
    [SerializeField] Animator m_Animator;
    int m_AckIndex;
    int m_MoveStatus;
    static readonly int s_AckIndex = Animator.StringToHash("AckIndex");
    static readonly int s_Acknowledge = Animator.StringToHash("Acknowledge");
    static readonly int s_MoveStatus = Animator.StringToHash("MoveStatus");
    static readonly int s_MoveStatusChanged = Animator.StringToHash("MoveStatusChanged");
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void _ApplyAnimation(int hashName, int hashTrigger, int currentVal, int minVal, int maxVal)
    {
        currentVal = Mathf.Clamp(currentVal, minVal, maxVal);
        m_Animator.SetInteger(hashName, currentVal);
        m_Animator.SetTrigger(hashTrigger);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            m_AckIndex++;
            _ApplyAnimation(s_AckIndex, s_Acknowledge, m_AckIndex, 0, 10);
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            m_AckIndex--;
            _ApplyAnimation(s_AckIndex, s_Acknowledge, m_AckIndex, 0, 10);
        }
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            Debug.Log("Hit MoveStatus++");
            m_MoveStatus++;
            _ApplyAnimation(s_MoveStatus, s_MoveStatusChanged, m_MoveStatus, 0, 2);
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            m_MoveStatus--;
            _ApplyAnimation(s_MoveStatus, s_MoveStatusChanged, m_MoveStatus, 0, 2);
        }
    }
}

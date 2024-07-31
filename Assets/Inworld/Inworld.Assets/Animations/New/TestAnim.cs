using UnityEngine;

public class TestAnim : MonoBehaviour
{
    [SerializeField] Animator m_Animator;
    int m_AckIndex = 1;
    int m_MoveStatus = 1;
    int m_InteractionStatus = 1;
    bool m_IsAngry;
    static readonly int s_AckIndex = Animator.StringToHash("AckIndex");
    static readonly int s_MoveStatus = Animator.StringToHash("MoveStatus");
    static readonly int s_InteractionStatus = Animator.StringToHash("InteractionStatus");
    static readonly int s_IsAngry = Animator.StringToHash("IsAngry");

    void _ApplyAnimation(int hashName, int currentVal, int minVal, int maxVal)
    {
        currentVal = Mathf.Clamp(currentVal, minVal, maxVal);
        m_Animator.SetInteger(hashName, currentVal);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            m_AckIndex++;
            _ApplyAnimation(s_AckIndex,  m_AckIndex, 1, 10);
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            m_AckIndex--;
            _ApplyAnimation(s_AckIndex,  m_AckIndex, 1, 10);
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            m_MoveStatus++;
            _ApplyAnimation(s_MoveStatus,  m_MoveStatus, 1, 2);
        }
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            m_MoveStatus--;
            _ApplyAnimation(s_MoveStatus,  m_MoveStatus, 1, 2);
        }
        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            m_InteractionStatus++;
            _ApplyAnimation(s_InteractionStatus,  m_InteractionStatus, 1, 2);
        }
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            m_InteractionStatus--;
            _ApplyAnimation(s_InteractionStatus,  m_InteractionStatus, 1, 2);
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            m_IsAngry = !m_IsAngry;
            m_Animator.SetBool(s_IsAngry, m_IsAngry);
        }
    }
}

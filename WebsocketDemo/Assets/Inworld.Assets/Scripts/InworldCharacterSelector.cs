using UnityEngine;

namespace Inworld.Sample
{
    public class InworldCharacterSelector : MonoBehaviour
    {
        [SerializeField] Transform m_Player;
        [Range(1, 180)]
        [SerializeField] float m_SightAngle = 90f;
        [Range(1, 30)]
        [SerializeField] float m_SightDistance = 10f;
        [SerializeField] float m_SightRefreshRate = 0.25f;
        float m_CurrentTime;
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            m_CurrentTime += Time.fixedDeltaTime;
            if (m_CurrentTime > m_SightRefreshRate)
            {
                m_CurrentTime = 0;
                _CheckPriority();
            }
        }
        void _CheckPriority()
        {
            throw new System.NotImplementedException();
        }
    }
}


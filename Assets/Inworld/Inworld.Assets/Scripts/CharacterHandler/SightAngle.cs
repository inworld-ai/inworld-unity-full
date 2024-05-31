/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using UnityEngine;


namespace Inworld.Sample
{
    public class SightAngle : MonoBehaviour
    {
        [SerializeField] Transform m_HeadTransform;
        [SerializeField] Transform m_PlayerTransform;

        [Range(1, 180)]
        [SerializeField] float m_SightAngle = 90f;
        [Range(1, 30)]
        [SerializeField] float m_SightDistance = 10f;
        [Range(0, 10)]
        [Tooltip("How much of an impact the Player's forward direction will have in the Priority calculation.")]
        [SerializeField] float m_PlayerAngleWeight = 1;
        [Range(0, 10)]
        [Tooltip("How much of an impact distance will have in the Priority calculation.")]
        [SerializeField] float m_DistanceWeight = 0.5f;
        [Range(0, 10)]
        [Tooltip("How much of an impact the character's forward direction will have in the Priority calculation.")]
        [SerializeField] float m_CharacterAngleWeight = 0.15f;
        [Range(0.1f, 1f)]
        [SerializeField] float m_RefreshRate = 0.25f;
        
        float m_CurrentTime = 0f;
        /// <summary>
        /// Get its character.
        /// </summary>
        public virtual InworldCharacter Character { get; private set; }

        protected virtual bool IsValid => InworldController.Instance && HeadTransform && PlayerTransform
                               && InworldController.CharacterHandler.SelectingMethod == CharSelectingMethod.SightAngle;

        public Transform HeadTransform
        {
            get
            {
                if (m_HeadTransform)
                    return m_HeadTransform;
                Animator animator = GetComponent<Animator>();
                if (animator)
                    m_HeadTransform = animator.GetBoneTransform(HumanBodyBones.Head);
                return m_HeadTransform;
            }
        }
        public Transform PlayerTransform
        {
            get
            {
                if (m_PlayerTransform)
                    return m_PlayerTransform;
                if (PlayerController.Instance)
                    m_PlayerTransform = PlayerController.Instance.transform;
                return m_PlayerTransform;
            }

        }

        void Awake()
        {
            Character = GetComponent<InworldCharacter>();
            if (!Character)
                enabled = false;
        }

        void Update()
        {
            if (!IsValid)
                return;
            CheckPriority();
        }

        void CheckPriority()
        {
            if (!HeadTransform || !PlayerTransform)
                return;
            m_CurrentTime += Time.deltaTime;
            if (m_CurrentTime < m_RefreshRate)
                return;
            m_CurrentTime = 0;
            
            float distance = Vector3.Distance(HeadTransform.position, PlayerTransform.position);
            if (distance > m_SightDistance)
                Character.Priority = -1f;
            else
            {
                Vector3 vecDirection = (PlayerTransform.position - HeadTransform.position).normalized;
                float fAngle = Vector3.Angle(vecDirection, transform.forward);
                if (fAngle > m_SightAngle)
                {
                    Character.Priority = -1f;
                }
                else
                {
                    Character.Priority = (Vector3.Angle(-vecDirection, PlayerTransform.forward) / 180f) * m_PlayerAngleWeight;
                    Character.Priority += (distance / m_SightDistance) * m_DistanceWeight; 
                    Character.Priority += (Vector3.Angle(HeadTransform.forward, vecDirection) / m_SightAngle) * m_CharacterAngleWeight;
                }
            }
        }
        void OnDrawGizmosSelected()
        {
            if (!HeadTransform)
                return;
            
            Gizmos.color = Color.cyan;
            Vector3 trPosition = m_HeadTransform.position;
            for (float angle = -m_SightAngle; angle < m_SightAngle; angle += m_SightAngle * 0.05f)
            {
                Gizmos.DrawLine(trPosition, trPosition + Quaternion.AngleAxis(angle, transform.up) * transform.forward * m_SightDistance);
            }
            Gizmos.color = Color.red;

            if (!m_PlayerTransform)
                return;
            Vector3 vecDirection = (m_PlayerTransform.position - trPosition).normalized;
            Gizmos.DrawLine(trPosition, trPosition + transform.forward * m_SightDistance);
            Gizmos.DrawLine(trPosition, trPosition + vecDirection * m_SightDistance);
        }
    }
}

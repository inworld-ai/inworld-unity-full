/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using UnityEngine;
using UnityEngine.Serialization;

namespace Inworld.Sample
{
    public class SightAngle : MonoBehaviour
    {
        [SerializeField] Animator m_Animator;
        [SerializeField] InworldCharacter m_Character;
        [Range(1, 180)]
        [SerializeField] float m_SightAngle = 90f;
        [Range(1, 30)]
        [SerializeField] float m_SightDistance = 10f;
        [Range(0, 100)]
        [Tooltip("How much of an impact distance will make in the Priority calculation.")]
        [SerializeField] float m_DistancePriorityFactor = 10;
        [Range(0.1f, 1f)]
        [SerializeField] float m_RefreshRate = 0.25f;
        
        Transform m_HeadTransform;
        Transform m_CameraTransform;
        float m_CurrentTime = 0f;
        /// <summary>
        /// Get its character.
        /// </summary>
        public virtual InworldCharacter Character => m_Character;
        
        /// <summary>
        ///     Returns the priority of the character.
        ///     the higher the Priority is, the character is more likely responding to player.
        /// </summary>
        public float Priority { get; private set; }

        protected virtual bool IsValid => InworldController.Instance 
                               && PlayerController.Instance 
                               && InworldController.CharacterHandler.SelectingMethod == CharSelectingMethod.SightAngle;

        void Awake()
        {
            m_HeadTransform = m_Animator.GetBoneTransform(HumanBodyBones.Head);
        }
        
        void OnEnable()
        {
            if (!m_CameraTransform)
                m_CameraTransform = PlayerController.Instance.transform;
        }

        void Update()
        {
            if (!IsValid)
                return;
            CheckPriority();
        }

        void CheckPriority()
        {
            m_CurrentTime += Time.deltaTime;
            if (m_CurrentTime < m_RefreshRate)
                return;
            m_CurrentTime = 0;
            
            float distance = Vector3.Distance(m_HeadTransform.position, m_CameraTransform.position);
            if (distance > m_SightDistance)
                Priority = -1f;
            else
            {
                Vector3 vecDirection = (m_CameraTransform.position - m_HeadTransform.position).normalized;
                float fAngle = Vector3.Angle(vecDirection, transform.forward);
                if (fAngle > m_SightAngle * 0.5f)
                {
                    Priority = -1f;
                }
                else
                {
                    Vector3 vecPlayerDirection = -vecDirection;
                    Priority = Vector3.Angle(vecPlayerDirection, m_CameraTransform.forward) + distance * m_DistancePriorityFactor;
                }
            }
        }
        void OnDrawGizmosSelected()
        {
            if (!m_HeadTransform)
                return;
            
            Gizmos.color = Color.cyan;
            Vector3 trPosition = m_HeadTransform.position;
            for (float angle = m_SightAngle * -0.5f; angle < m_SightAngle * 0.5f; angle += m_SightAngle * 0.05f)
            {
                Gizmos.DrawLine(trPosition, trPosition + Quaternion.AngleAxis(angle, transform.up) * transform.forward * m_SightDistance);
            }
            Gizmos.color = Color.red;

            if (!InworldController.Instance || !PlayerController.Instance)
                return;
            Vector3 vecDirection = (PlayerController.Instance.transform.position - trPosition).normalized;
            Gizmos.DrawLine(trPosition, trPosition + transform.forward * m_SightDistance);
            Gizmos.DrawLine(trPosition, trPosition + vecDirection * m_SightDistance);
        }
    }
}

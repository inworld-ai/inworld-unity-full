/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Packet;
using UnityEngine;

namespace Inworld.Assets
{
    public class InworldBodyAnimation : InworldAnimation
    {
        [SerializeField] Transform m_PlayerCamera;
        [SerializeField] Animator m_BodyAnimator;
        [SerializeField] EmotionMap m_EmotionMap;

        Transform m_Transform;
        Transform m_MainCameraTransform;
        Transform m_trLookAt;
        Vector3 m_vecInitPosition;
        Vector3 m_vecInitEuler;
        float m_LookAtWeight;
        
        static readonly int s_Emotion = Animator.StringToHash("Emotion");
        static readonly int s_Gesture = Animator.StringToHash("Gesture");
        static readonly int s_Motion = Animator.StringToHash("MainStatus");
        static readonly int s_RemainSec = Animator.StringToHash("RemainSec");
        static readonly int s_Random = Animator.StringToHash("Random");

        protected override void OnEnable()
        {
            m_Transform = transform;
            if (Camera.main)
                m_MainCameraTransform = Camera.main.transform;
            m_vecInitEuler = m_Transform.localEulerAngles;
            m_vecInitPosition = m_Transform.localPosition;
            m_Interaction.OnStartStopInteraction += OnStartStopInteraction;
            InworldController.CharacterHandler.OnCharacterChanged += OnCharChanged;
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            m_Interaction.OnStartStopInteraction -= OnStartStopInteraction;
            if (!InworldController.Instance)
                return;
            InworldController.CharacterHandler.OnCharacterChanged -= OnCharChanged;
            base.OnDisable();
        }
        void OnAnimatorIK(int layerIndex)
        {
            if (!m_BodyAnimator)
                return;
            if (m_trLookAt == null)
            {
                _StopLookAt();
                return;
            }
            _StartLookAt(m_trLookAt.position);
        }

        protected virtual void OnStartStopInteraction(bool isStarting)
        {
            HandleMainStatus(isStarting ? AnimMainStatus.Talking : AnimMainStatus.Neutral);
        }
        protected virtual void OnCharChanged(InworldCharacter oldChar, InworldCharacter newChar)
        {
            if (oldChar != null && oldChar.BrainName == m_Character.Data.brainName)
            {
                m_trLookAt = null;
                HandleMainStatus(AnimMainStatus.Goodbye);
                m_BodyAnimator.enabled = false;
            }
            if (newChar != null && newChar.BrainName == m_Character.Data.brainName)
            {
                m_trLookAt = m_PlayerCamera ? m_PlayerCamera :m_MainCameraTransform;
                m_BodyAnimator.enabled = true;
                HandleMainStatus(AnimMainStatus.Hello);
            }
        }
        protected virtual void HandleMainStatus(AnimMainStatus status) => m_BodyAnimator.SetInteger(s_Motion, (int)status);
        
        protected override void HandleEmotion(EmotionPacket packet)
        {
            m_BodyAnimator.SetFloat(s_Random, Random.Range(0, 1) > 0.5f ? 1 : 0);
            m_BodyAnimator.SetFloat(s_RemainSec, m_Interaction.AudioLength);
            _ProcessEmotion(packet.emotion.behavior.ToUpper());
        }

        void _ProcessEmotion(string emotionBehavior)
        {
            EmotionMapData emoMapData = m_EmotionMap[emotionBehavior];
            if (emoMapData == null)
            {
                InworldAI.LogError($"Unhandled emotion {emotionBehavior}");
                return;
            }
            m_BodyAnimator.SetInteger(s_Emotion, (int)emoMapData.bodyEmotion);
            m_BodyAnimator.SetInteger(s_Gesture, (int)emoMapData.bodyGesture);
        }
        void _StartLookAt(Vector3 lookPos)
        {
            m_LookAtWeight = Mathf.Clamp(1 - Vector3.Angle(transform.forward, lookPos) * 0.01f, 0, 1);
            m_BodyAnimator.SetLookAtWeight(m_LookAtWeight);
            m_BodyAnimator.SetLookAtPosition(lookPos);
        }
        void _StopLookAt()
        {
            m_Transform.localPosition = m_vecInitPosition;
            m_Transform.localEulerAngles = m_vecInitEuler;
            m_LookAtWeight = Mathf.Clamp(m_LookAtWeight - 0.01f, 0, 1);
            m_BodyAnimator.SetLookAtWeight(m_LookAtWeight);
        }
    }
}


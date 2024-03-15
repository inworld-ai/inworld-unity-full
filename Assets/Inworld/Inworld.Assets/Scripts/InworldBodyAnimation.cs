/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Packet;
using Inworld.Sample;
using UnityEngine;
using UnityEngine.Events;

namespace Inworld.Assets
{
    public class InworldBodyAnimation : InworldAnimation
    {
        [SerializeField] protected Animator m_BodyAnimator;
        
        Transform m_HeadTransform;
        Vector3 m_vecInitPosition;
        Vector3 m_vecInitEuler;
        float m_LookAtWeight;
        
        protected static readonly int s_Emotion = Animator.StringToHash("Emotion");
        protected static readonly int s_Gesture = Animator.StringToHash("Gesture");
        protected static readonly int s_Motion = Animator.StringToHash("MainStatus");
        protected static readonly int s_RemainSec = Animator.StringToHash("RemainSec");
        protected static readonly int s_Random = Animator.StringToHash("Random");

        protected override void Awake()
        {
            base.Awake();
            m_HeadTransform = m_BodyAnimator.GetBoneTransform(HumanBodyBones.Head);
        }
        
        protected override void OnEnable()
        {
            m_vecInitEuler = m_HeadTransform.localEulerAngles;
            m_vecInitPosition = m_HeadTransform.localPosition;
            m_Character.Event.onBeginSpeaking.AddListener(OnCharacterStartSpeaking);
            m_Character.Event.onEndSpeaking.AddListener(OnCharacterEndSpeaking);
            m_Character.Event.onCharacterSelected.AddListener(OnCharacterSelected);
            m_Character.Event.onCharacterDeselected.AddListener(OnCharacterDeselected);
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            m_Character.Event.onBeginSpeaking.RemoveListener(OnCharacterStartSpeaking);
            m_Character.Event.onEndSpeaking.RemoveListener(OnCharacterEndSpeaking);
            m_Character.Event.onCharacterSelected.RemoveListener(OnCharacterSelected);
            m_Character.Event.onCharacterDeselected.RemoveListener(OnCharacterDeselected);
            base.OnDisable();
        }
        void OnAnimatorIK(int layerIndex)
        {
            if (!m_BodyAnimator)
                return;
            if (!PlayerController.Instance)
            {
                _StopLookAt();
                return;
            }
            _StartLookAt(PlayerController.Instance.transform.position);
        }
        protected virtual void OnCharacterStartSpeaking(string brainName) => HandleMainStatus(AnimMainStatus.Talking);

        protected virtual void OnCharacterEndSpeaking(string brainName) => HandleMainStatus(AnimMainStatus.Neutral);
        public virtual void OnStartStopInteraction(bool isStarting)
        {
            HandleMainStatus(isStarting ? AnimMainStatus.Talking : AnimMainStatus.Neutral);
        }

        protected virtual void OnCharacterSelected(string brainName) => HandleMainStatus(AnimMainStatus.Hello);
        
        protected virtual void OnCharacterDeselected(string brainName) => HandleMainStatus(AnimMainStatus.Goodbye);


        protected virtual void HandleMainStatus(AnimMainStatus status) => m_BodyAnimator.SetInteger(s_Motion, (int)status);
        
        protected override void HandleEmotion(EmotionPacket packet)
        {
            m_BodyAnimator.SetFloat(s_Random, Random.Range(0, 1) > 0.5f ? 1 : 0);
            m_BodyAnimator.SetFloat(s_RemainSec, m_Interaction.AnimFactor);
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
            m_LookAtWeight = Mathf.Clamp(1 - Vector3.Angle(transform.forward, (lookPos - m_HeadTransform.position).normalized) * 0.01f, 0, 1);
            m_BodyAnimator.SetLookAtWeight(m_LookAtWeight);
            m_BodyAnimator.SetLookAtPosition(lookPos);
        }
        void _StopLookAt()
        {
            m_HeadTransform.localPosition = m_vecInitPosition;
            m_HeadTransform.localEulerAngles = m_vecInitEuler;
            m_LookAtWeight = Mathf.Clamp(m_LookAtWeight - 0.01f, 0, 1);
            m_BodyAnimator.SetLookAtWeight(m_LookAtWeight);
        }
    }
}


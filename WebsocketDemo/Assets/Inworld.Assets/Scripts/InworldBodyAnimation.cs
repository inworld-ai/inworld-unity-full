using Inworld.Interactions;
using Inworld.Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inworld.Sample
{
    public class InworldBodyAnimation : MonoBehaviour
    {
        [SerializeField] Transform m_PlayerCamera;
        [SerializeField] Animator m_BodyAnimator;
        [SerializeField] EmotionMap m_EmotionMap;
        InworldCharacter m_Character;
        InworldInteraction m_Interaction;
        Transform m_Transform;
        Transform m_trLookAt;
        Vector3 m_vecInitPosition;
        Vector3 m_vecInitEuler;
        float m_LookAtWeight;
        static readonly int s_Emotion = Animator.StringToHash("Emotion");
        static readonly int s_Gesture = Animator.StringToHash("Gesture");
        static readonly int s_Motion = Animator.StringToHash("MainStatus");
        static readonly int s_RemainSec = Animator.StringToHash("RemainSec");
        static readonly int s_Random = Animator.StringToHash("Random");
        
        
        // Start is called before the first frame update
        void Awake()
        {
            enabled = _Init();
        }
        void Start()
        {
        
        }
        protected virtual void OnEnable()
        {
            m_Transform = transform;
            m_vecInitEuler = m_Transform.localEulerAngles;
            m_vecInitPosition = m_Transform.localPosition;

            InworldController.Instance.OnCharacterChanged += OnCharChanged;
            m_Interaction.OnStartStopInteraction += OnStartStopInteraction;
            m_Interaction.OnInteractionChanged += OnInteractionChanged;
        }

        protected virtual void OnDisable()
        {
            m_Interaction.OnStartStopInteraction -= OnStartStopInteraction;
            m_Interaction.OnInteractionChanged -= OnInteractionChanged;
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnCharacterChanged -= OnCharChanged;
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
        // Update is called once per frame
        void Update()
        {
        
        }
        protected virtual void OnInteractionChanged(List<InworldPacket> packets)
        {
            foreach (InworldPacket packet in packets)
            {
                ProcessPacket(packet);
            }
        }
        protected virtual void ProcessPacket(InworldPacket incomingPacket)
        {
            switch (incomingPacket)
            {
                case EmotionPacket emotionPacket:
                    HandleEmotion(emotionPacket);
                    break;
            }
        }
        protected void OnStartStopInteraction(bool isStarting)
        {
            HandleMainStatus(isStarting ? AnimMainStatus.Talking : AnimMainStatus.Neutral);
        }
        protected void OnCharChanged(InworldCharacter oldChar, InworldCharacter newChar)
        {
            if (oldChar != null && oldChar.BrainName == m_Character.Data.brainName)
            {
                m_trLookAt = null;
                HandleMainStatus(AnimMainStatus.Goodbye);
                m_BodyAnimator.enabled = false;
            }
            if (newChar != null && newChar.BrainName == m_Character.Data.brainName)
            {
                m_trLookAt = m_PlayerCamera ? m_PlayerCamera : Camera.main.transform;
                m_BodyAnimator.enabled = true;
                HandleMainStatus(AnimMainStatus.Hello);
            }
        }
        public void HandleMainStatus(AnimMainStatus status) => m_BodyAnimator.SetInteger(s_Motion, (int)status);
        
        protected void HandleEmotion(EmotionPacket packet)
        {
            m_BodyAnimator.SetFloat(s_Random, Random.Range(0, 1) > 0.5f ? 1 : 0);
            m_BodyAnimator.SetFloat(s_RemainSec, m_Interaction.AudioLength);
            _ProcessEmotion(packet.emotion.behavior.ToUpper());
        }
        
        bool _Init()
        {
            m_Character ??= GetComponent<InworldCharacter>();
            m_Interaction ??= GetComponent<InworldInteraction>();
            return m_Character && m_Interaction;
        }
        void _ProcessEmotion(string emotionBehavior)
        {
            EmotionMapData emoMapData = m_EmotionMap[emotionBehavior];
            if (emoMapData == null)
            {
                Debug.LogError($"Unhandled emotion {emotionBehavior}");
                return;
            }
            m_BodyAnimator.SetInteger(s_Emotion, (int)emoMapData.bodyEmotion);
            m_BodyAnimator.SetInteger(s_Gesture, (int)emoMapData.bodyGesture);
        }
        void _StartLookAt(Vector3 lookPos)
        {
            m_LookAtWeight = Mathf.Clamp(m_LookAtWeight + 0.01f, 0, 1);
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

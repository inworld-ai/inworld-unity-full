/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Packets;
using Inworld.Sample.UI;
using Inworld.Util;
using System.Collections;
using System.Linq;
using UnityEngine;
using EmotionEvent = Inworld.Grpc.EmotionEvent;
using GestureEvent = Inworld.Grpc.GestureEvent;
namespace Inworld.Model
{
    /// <summary>
    ///     This class is the basic class to display head animations,
    ///     that only supports looking at players.
    ///     If you want to use detailed head-eye movement, please do the followings:
    ///     1. purchase and download page `Realistic Eye Movements`
    ///     https://assetstore.unity.com/packages/tools/animation/realistic-eye-movements-29168
    ///     2. Add `LookTargetController` and `EyeAndHeadAnimator` components to InworldCharacters.
    ///     3. Implement `SetupHeadMovement`:
    ///     a. Call Resources.Load
    ///     <TextAsset>
    ///         (m_HeadEyeAsset);
    ///         b. Call `EyeAndHeadAnimator::ImportFromJson()`, with the data of the TextAsset you loaded.
    /// </summary>
    public class HeadAnimation : MonoBehaviour, InworldAnimation, IEyeHeadAnimLoader
    {
        #region Callbacks
        void OnCharacterChanged(InworldCharacter oldChar, InworldCharacter newChar)
        {
            if (Character == oldChar)
                m_trLookAt = null;
            else if (Character == newChar)
                m_trLookAt = InworldController.Player.transform;
        }
        #endregion

        #region Inspector Variables
        [SerializeField] string m_HeadEyeAsset = "Animations/REMRPM";
        [SerializeField] FacialAnimationData m_FaceData;
        [SerializeField] float m_MorphTime = 0.5f;
        #endregion

        #region Private Variables
        Transform m_trLookAt;
        Transform m_Transform;
        Vector3 m_vecInitPosition;
        Vector3 m_vecInitEuler;
        SkinnedMeshRenderer m_Skin;
        FacialAnimation m_LastFacial;
        ChatPanel3D m_CharacterChatPanel;
        float m_LookAtWeight;
        #endregion

        #region Properties
        /// <summary>
        ///     Get/Set the attached Animator.
        /// </summary>
        public Animator Animator { get; set; }
        /// <summary>
        ///     Get/Set the attached Inworld Character.
        /// </summary>
        public InworldCharacter Character { get; set; }
        #endregion

        #region Monobehavior Functions
        void Awake()
        {
            enabled = Init();
        }
        void OnEnable()
        {
            m_Transform = transform;
            m_vecInitEuler = m_Transform.localEulerAngles;
            m_vecInitPosition = m_Transform.localPosition;
            InworldController.Instance.OnCharacterChanged += OnCharacterChanged;
            InworldController.Instance.OnPacketReceived += OnPacketEvents;
        }
        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnCharacterChanged -= OnCharacterChanged;
            InworldController.Instance.OnPacketReceived -= OnPacketEvents;
        }
        void OnAnimatorIK(int layerIndex)
        {
            if (!Animator)
                return;
            if (m_trLookAt == null)
            {
                _StopLookAt();
                return;
            }
            _StartLookAt(m_trLookAt.position);
        }
        #endregion

        #region Private Functions
        void _StartLookAt(Vector3 lookPos)
        {
            m_LookAtWeight = Mathf.Clamp(m_LookAtWeight + 0.01f, 0, 1);
            Animator.SetLookAtWeight(m_LookAtWeight);
            Animator.SetLookAtPosition(lookPos);
        }
        void _StopLookAt()
        {
            m_Transform.localPosition = m_vecInitPosition;
            m_Transform.localEulerAngles = m_vecInitEuler;
            m_LookAtWeight = Mathf.Clamp(m_LookAtWeight - 0.01f, 0, 1);
            Animator.SetLookAtWeight(m_LookAtWeight);
        }
        #endregion

        #region Interface Implementation
        public void HandleMainStatus(AnimMainStatus status)
        {
            //Implement your own logic here.
        }
        void OnPacketEvents(InworldPacket packet)
        {
            if (packet.Routing.Target.Id != Character.ID && packet.Routing.Source.Id != Character.ID)
                return;
            switch (packet)
            {
                case Packets.EmotionEvent emotionEvent:
                    HandleEmotion(emotionEvent.SpaffCode);
                    break;
                case Packets.GestureEvent gestureEvent:
                    HandleGesture(gestureEvent.Simple);
                    break;
            }
        }
        public void HandleEmotion(EmotionEvent.Types.SpaffCode spaffCode)
        {
            switch (spaffCode)
            {
                case EmotionEvent.Types.SpaffCode.Affection:
                case EmotionEvent.Types.SpaffCode.Interest:
                    _ProcessEmotion("Anticipation");
                    break;
                case EmotionEvent.Types.SpaffCode.Humor:
                case EmotionEvent.Types.SpaffCode.Joy:
                    _ProcessEmotion("Joy");
                    break;
                case EmotionEvent.Types.SpaffCode.Contempt:
                case EmotionEvent.Types.SpaffCode.Criticism:
                case EmotionEvent.Types.SpaffCode.Disgust:
                    _ProcessEmotion("Disgust");
                    break;
                case EmotionEvent.Types.SpaffCode.Belligerence:
                case EmotionEvent.Types.SpaffCode.Domineering:
                case EmotionEvent.Types.SpaffCode.Anger:
                    _ProcessEmotion("Anger");
                    break;
                case EmotionEvent.Types.SpaffCode.Tension:
                case EmotionEvent.Types.SpaffCode.Stonewalling:
                case EmotionEvent.Types.SpaffCode.TenseHumor:
                case EmotionEvent.Types.SpaffCode.Defensiveness:
                    _ProcessEmotion("Fear");
                    break;
                case EmotionEvent.Types.SpaffCode.Whining:
                case EmotionEvent.Types.SpaffCode.Sadness:
                    _ProcessEmotion("Sadness");
                    break;
                case EmotionEvent.Types.SpaffCode.Surprise:
                    _ProcessEmotion("Surprise");
                    break;
                default:
                    _ProcessEmotion("Neutral");
                    break;
            }
        }
        public void HandleGesture(GestureEvent.Types.Type gesture)
        {
            //Implement your own logic here.
        }
        public void SetupHeadMovement(GameObject avatar)
        {
            InworldAI.Log($"If you want to integrate detailed head/eye movent,\nplease Load {m_HeadEyeAsset} as Text,\nthen use`Realistic Eye Movements` to load it from json");
            //Implement your own logic here.
        }
        public bool Init()
        {
            Animator ??= GetComponent<Animator>();
            Character ??= GetComponent<InworldCharacter>();
            m_Skin ??= Character.GetComponentInChildren<SkinnedMeshRenderer>();
            m_CharacterChatPanel ??= Character.GetComponentInChildren<ChatPanel3D>();
            return Animator && Character;
        }
        void _ProcessEmotion(string emotion)
        {
            FacialAnimation targetEmo = m_FaceData.emotions.FirstOrDefault(emo => emo.emotion == emotion);
            if (targetEmo != null && m_LastFacial != targetEmo)
            {
                StartCoroutine(_MorphTo(targetEmo));
            }
        }
        IEnumerator _MorphTo(FacialAnimation emo)
        {
            if (m_CharacterChatPanel)
                m_CharacterChatPanel.ProcessEmotion(emo);
            if (!m_Skin)
                yield break;
            float morphTime = 0;
            while (morphTime < m_MorphTime)
            {
                for (int i = 0; i < m_Skin.sharedMesh.blendShapeCount; i++)
                {
                    string currIterName = m_Skin.sharedMesh.GetBlendShapeName(i);
                    float fCurrShapeWeight = m_Skin.GetBlendShapeWeight(i);
                    MorphState lastState = m_LastFacial?.morphStates.FirstOrDefault(morph => morph.morphName == currIterName);
                    MorphState currState = emo.morphStates.FirstOrDefault(morph => morph.morphName == currIterName);
                    // 1. Reset Old
                    if (lastState != null && currState == null)
                        m_Skin.SetBlendShapeWeight(i, Mathf.Lerp(fCurrShapeWeight, 0, 0.15f));
                    // 2. Apply New
                    if (currState != null)
                        m_Skin.SetBlendShapeWeight(i, Mathf.Lerp(fCurrShapeWeight, currState.morphWeight, 0.15f));
                }
                morphTime += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            m_LastFacial = emo;
        }
        #endregion
    }
}

/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Grpc;
using Inworld.Util;
using UnityEngine;
using InworldPacket = Inworld.Packets.InworldPacket;
namespace Inworld.Model
{
    /// <summary>
    ///     This component is used to receive gesture/emotion events from server,
    ///     and play animations on that character.
    /// </summary>
    public class BodyAnimation : MonoBehaviour, InworldAnimation
    {
        /// <summary>
        ///     Handle Character's main status:
        ///     Idle, Talking, walking, etc.
        /// </summary>
        /// <param name="status">incomingStatus</param>
        public void HandleMainStatus(AnimMainStatus status)
        {
            Animator.SetInteger(s_Motion, (int)status);
        }
        /// <summary>
        ///     Play Animation according to target emotion.
        ///     Please adjust this function to select/play your customized animations.
        /// </summary>
        /// <param name="spaffCode">An enum of emotion</param>
        public void HandleEmotion(EmotionEvent.Types.SpaffCode spaffCode)
        {
            Character.Emotion = spaffCode.ToString();
            Animator.SetFloat(s_Random, Random.Range(0, 1) > 0.5f ? 1 : 0);
            Animator.SetFloat(s_RemainSec, Character.CurrentAudioRemainingTime);
            switch (spaffCode)
            {
                case EmotionEvent.Types.SpaffCode.Neutral:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Neutral);
                    Animator.SetInteger(s_Gesture, (int)Gesture.Neutral);
                    break;
                case EmotionEvent.Types.SpaffCode.Affection:
                case EmotionEvent.Types.SpaffCode.Interest:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Happy);
                    break;
                case EmotionEvent.Types.SpaffCode.Humor:
                case EmotionEvent.Types.SpaffCode.Joy:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Happy);
                    break;
                case EmotionEvent.Types.SpaffCode.Contempt:
                case EmotionEvent.Types.SpaffCode.Belligerence:
                case EmotionEvent.Types.SpaffCode.Domineering:
                case EmotionEvent.Types.SpaffCode.Criticism:
                case EmotionEvent.Types.SpaffCode.Anger:
                case EmotionEvent.Types.SpaffCode.Defensiveness:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Angry);
                    break;
                case EmotionEvent.Types.SpaffCode.Tension:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Fear);
                    break;
                case EmotionEvent.Types.SpaffCode.Stonewalling:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Fear);
                    break;
                case EmotionEvent.Types.SpaffCode.TenseHumor:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Fear);
                    break;
                case EmotionEvent.Types.SpaffCode.Whining:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Sad);
                    break;
                case EmotionEvent.Types.SpaffCode.Sadness:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Sad);
                    break;
                case EmotionEvent.Types.SpaffCode.Validation:
                    Animator.SetInteger(s_Gesture, (int)Gesture.Confuse);
                    break;
                case EmotionEvent.Types.SpaffCode.Disgust:
                    Animator.SetInteger(s_Gesture, (int)Gesture.Disgusted);
                    break;
                case EmotionEvent.Types.SpaffCode.Surprise:
                    Animator.SetInteger(s_Gesture, (int)Gesture.Surprise);
                    break;
            }
        }
        /// <summary>
        ///     Play target gesture's animations.
        ///     Please adjust this function to select/play your customized animations.
        /// </summary>
        /// <param name="gesture">An enum of target gesture</param>
        public void HandleGesture(GestureEvent.Types.Type gesture)
        {
            Character.Gesture = gesture.ToString();
            Animator.SetFloat(s_Random, Random.Range(0, 1) > 0.5f ? 1 : 0);
            Animator.SetFloat(s_RemainSec, Character.CurrentAudioRemainingTime);
            switch (gesture)
            {
                case GestureEvent.Types.Type.Agreement:
                    Animator.SetInteger(s_Gesture, (int)Gesture.Acknowledge);
                    break;
                case GestureEvent.Types.Type.Greeting:
                    Animator.SetInteger(s_Gesture, (int)Gesture.Greetings);
                    break;
                case GestureEvent.Types.Type.Farewell:
                    Animator.SetInteger(s_Motion, (int)AnimMainStatus.Goodbye);
                    break;
                case GestureEvent.Types.Type.Disagreement:
                    Animator.SetInteger(s_Gesture, (int)Gesture.Disagree);
                    break;
                case GestureEvent.Types.Type.Gratitude:
                    Animator.SetInteger(s_Gesture, (int)Gesture.Thank);
                    break;
                case GestureEvent.Types.Type.Celebration:
                    Animator.SetInteger(s_Gesture, (int)Gesture.Celebrate);
                    break;
                case GestureEvent.Types.Type.Boredom:
                    Animator.SetInteger(s_Gesture, (int)Gesture.Bore);
                    break;
                case GestureEvent.Types.Type.Uncertainty:
                    Animator.SetInteger(s_Gesture, (int)Gesture.Confuse);
                    break;
            }
        }
        public bool Init()
        {
            Animator ??= GetComponent<Animator>();
            Character ??= GetComponent<InworldCharacter>();
            return Animator && Character;
        }

        #region Private Variables
        static readonly int s_Emotion = Animator.StringToHash("Emotion");
        static readonly int s_Gesture = Animator.StringToHash("Gesture");
        static readonly int s_RemainSec = Animator.StringToHash("RemainSec");
        static readonly int s_Random = Animator.StringToHash("Random");
        static readonly int s_Motion = Animator.StringToHash("MainStatus");
        #endregion

        #region Properties
        /// <summary>
        ///     Get/Set the Animator this component attached.
        /// </summary>
        public Animator Animator { get; set; }
        /// <summary>
        ///     Get/Set the Inworld Character this component used.
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
            InworldController.Instance.OnCharacterChanged += OnCharacterChanged;
            InworldController.Instance.OnStateChanged += OnStatusChanged;
            InworldController.Instance.OnPacketReceived += OnPacketEvents;
            if (!Character)
                return;
            Character.OnBeginSpeaking.AddListener(OnAudioStarted);
            Character.OnFinishedSpeaking.AddListener(OnAudioFinished);
        }
        void OnDisable()
        {
            if (InworldController.Instance)
            {
                InworldController.Instance.OnCharacterChanged -= OnCharacterChanged;
                InworldController.Instance.OnStateChanged -= OnStatusChanged;
                InworldController.Instance.OnPacketReceived -= OnPacketEvents;
            }
            if (!Character)
                return;
            Character.OnBeginSpeaking.RemoveListener(OnAudioStarted);
            Character.OnFinishedSpeaking.RemoveListener(OnAudioFinished);
        }
        #endregion

        #region Callbacks
        void OnCharacterChanged(InworldCharacter oldCharacter, InworldCharacter newCharacter)
        {
            if (oldCharacter == Character)
            {
                HandleMainStatus(AnimMainStatus.Goodbye);
            }
            else if (newCharacter == Character)
            {
                HandleMainStatus(AnimMainStatus.Hello);
            }
        }
        void OnPacketEvents(InworldPacket packet)
        {
            if (!Animator)
                return;
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
        void OnStatusChanged(ControllerStates newStatus)
        {
            if (newStatus == ControllerStates.Connected)
                HandleMainStatus(AnimMainStatus.Neutral);
        }
        void OnAudioStarted()
        {
            HandleMainStatus(AnimMainStatus.Talking);
        }

        void OnAudioFinished()
        {
            HandleMainStatus(AnimMainStatus.Neutral);
        }
        #endregion
    }
}

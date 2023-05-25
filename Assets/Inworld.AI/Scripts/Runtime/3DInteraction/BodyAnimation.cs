/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Grpc;
using Inworld.Util;
using System;
using UnityEngine;
using InworldPacket = Inworld.Packets.InworldPacket;
using Random = UnityEngine.Random;
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
                    Animator.SetInteger(s_Emotion, (int)Emotion.Happy);
                    Animator.SetInteger(s_Gesture, (int)Gesture.Celebrate);
                    break;
                case EmotionEvent.Types.SpaffCode.Interest:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Neutral);
                    Animator.SetInteger(s_Gesture, (int)Gesture.Interested);
                    break;
                case EmotionEvent.Types.SpaffCode.Humor:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Happy);
                    Animator.SetInteger(s_Gesture, (int)Gesture.Celebrate);
                    break;
                case EmotionEvent.Types.SpaffCode.Joy:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Happy);
                    Animator.SetInteger(s_Gesture, (int)Gesture.Happy);
                    break;
                case EmotionEvent.Types.SpaffCode.Contempt:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Angry);
                    Animator.SetInteger(s_Gesture, (int)Gesture.Dismissing);
                    break;
                case EmotionEvent.Types.SpaffCode.Belligerence:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Angry);
                    Animator.SetInteger(s_Gesture, (int)Gesture.FollowMe);
                    break;
                case EmotionEvent.Types.SpaffCode.Domineering:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Angry);
                    Animator.SetInteger(s_Gesture, (int)Gesture.Happy);
                    break;
                case EmotionEvent.Types.SpaffCode.Criticism:
                    Animator.SetInteger(s_Gesture, (int)Gesture.Think);
                    break;
                case EmotionEvent.Types.SpaffCode.Anger:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Angry);
                    Animator.SetInteger(s_Gesture, (int)Gesture.Angry);
                    break;
                case EmotionEvent.Types.SpaffCode.Defensiveness:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Neutral);
                    Animator.SetInteger(s_Gesture, (int)Gesture.TellToListen);
                    break;
                case EmotionEvent.Types.SpaffCode.Tension:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Fear);
                    Animator.SetInteger(s_Gesture, (int)Gesture.Fear);
                    break;
                case EmotionEvent.Types.SpaffCode.Stonewalling:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Sad);
                    Animator.SetInteger(s_Gesture, (int)Gesture.TellToListen);
                    break;
                case EmotionEvent.Types.SpaffCode.TenseHumor:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Fear);
                    Animator.SetInteger(s_Gesture, (int)Gesture.Exclamation);
                    break;
                case EmotionEvent.Types.SpaffCode.Whining:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Sad);
                    Animator.SetInteger(s_Gesture, (int)Gesture.Disagree);
                    break;
                case EmotionEvent.Types.SpaffCode.Sadness:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Sad);
                    Animator.SetInteger(s_Emotion, (int)Gesture.Sad);
                    break;
                case EmotionEvent.Types.SpaffCode.Validation:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Neutral);
                    Animator.SetInteger(s_Gesture, (int)Gesture.Agree);
                    break;
                case EmotionEvent.Types.SpaffCode.Disgust:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Neutral);
                    Animator.SetInteger(s_Gesture, (int)Gesture.Disgusted);
                    break;
                case EmotionEvent.Types.SpaffCode.Surprise:
                    Animator.SetInteger(s_Emotion, (int)Emotion.Neutral);
                    Animator.SetInteger(s_Gesture, (int)Gesture.Surprise);
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
            Animator.enabled = false;
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
            Animator.enabled = false;
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
                Animator.enabled = false;
            }
            else if (newCharacter == Character)
            {
                Animator.enabled = true;
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

/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Interactions;
using Inworld.Packet;
using Inworld.Entities;
using Inworld.Sample;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inworld
{
    public class InworldCharacter : MonoBehaviour
    {
        [SerializeField] protected InworldCharacterData m_Data;
        [SerializeField] protected CharacterEvents m_CharacterEvents;
        [SerializeField] protected bool m_VerboseLog;

        protected Animator m_Animator;
        protected InworldInteraction m_Interaction;
        protected bool m_IsSpeaking;
        
        RelationState m_CurrentRelation = new RelationState();

        /// <summary>
        /// Gets if this character is in debug mode and enables verbose log.
        /// </summary>
        public bool EnableVerboseLog
        {
            get => m_VerboseLog;
            set => m_VerboseLog = value;
        }
        /// <summary>
        /// Get/Set if the character is trying to disable.
        /// </summary>
        public bool IsOnDisable { get; set; }
        /// <summary>
        /// Gets the Unity Events of the character.
        /// </summary>
        public CharacterEvents Event => m_CharacterEvents;
        /// <summary>
        /// Gets the character's animator.
        /// </summary>
        public Animator Animator
        {
            get
            {
                if (!m_Animator)
                    m_Animator = GetComponent<Animator>();
                return m_Animator;
            }
        }
        /// <summary>
        /// Gets/Sets if this character is speaking.
        /// </summary>
        public bool IsSpeaking
        {
            get => m_IsSpeaking;
            internal set
            {
                if (m_IsSpeaking == value)
                    return;
                m_IsSpeaking = value;
                if (m_IsSpeaking)
                {
                    if (m_VerboseLog)
                        InworldAI.Log($"{Name} Starts Speaking");
                    m_CharacterEvents.onBeginSpeaking.Invoke(BrainName);
                }
                else
                {
                    if (m_VerboseLog)
                        InworldAI.Log($"{Name} Ends Speaking");
                    m_CharacterEvents.onEndSpeaking.Invoke(BrainName);
                }
            }
        }
        /// <summary>
        /// Gets/Sets the character's current relationship towards players. Will invoke onRelationUpdated when set.
        /// </summary>
        public RelationState CurrRelation
        {
            get => m_CurrentRelation;
            protected set
            {
                if (m_CurrentRelation.IsEqualTo(value))
                    return;
                if (m_VerboseLog)
                    InworldAI.Log($"{Name}: {m_CurrentRelation.GetUpdate(value)}");
                m_CurrentRelation = value;
                m_CharacterEvents.onRelationUpdated.Invoke(BrainName);
            }
        }
        /// <summary>
        /// Gets/Sets the character's data.
        /// If set, it'll also allocate the live session ID to the character's `InworldInteraction` component.
        /// </summary>
        public InworldCharacterData Data 
        {
            get => m_Data;
            set => m_Data = value;
        }
        /// <summary>
        /// Get the display name for the character. Note that name may not be unique.
        /// </summary>
        public string Name => Data?.givenName ?? "";
        /// <summary>
        /// The `BrainName` for the character.
        /// Note that `BrainName` is actually the character's full name, formatted like `workspace/xxx/characters/xxx`.
        /// It is unique.
        /// </summary>
        public string BrainName
        {
            get
            {
                if (Data == null)
                    return "";
                string brainName = Data.brainName;
                if (brainName.Contains("/characters/"))
                    return brainName;
                if (!InworldController.Instance || !InworldController.Instance.GameData)
                    return "";
                string wsName = InworldController.Instance.GameData.workspaceName;
                return InworldAI.GetCharacterFullName(wsName, brainName);
            }
        }

        /// <summary>
        /// Gets the live session ID of the character. If not registered, will try to fetch one from InworldController's CharacterHandler.
        /// </summary>
        public string ID => string.IsNullOrEmpty(Data?.agentId) ? GetLiveSessionID() : Data?.agentId;
        /// <summary>
        ///     Returns the priority of the character. It's used by CharacterHandler3D.     
        ///     The closer to zero the Priority is, the character is more likely responding to player.
        /// </summary>
        public float Priority { get; set; } = float.MaxValue;
        /// <summary>
        /// Register the character in the character list.
        /// Get the live session ID for an Inworld character.
        /// </summary>

        public virtual string GetLiveSessionID()
        {
            if (string.IsNullOrEmpty(BrainName))
                return "";
            if (!InworldController.Client.LiveSessionData.TryGetValue(BrainName, out InworldCharacterData value))
                return "";
            Data = value;
            return Data.agentId;
        }
        /// <summary>
        /// Send the message to this character.
        /// </summary>
        /// <param name="text">the message to send</param>
        public virtual bool SendText(string text)
        {
            // 1. Interrupt current speaking.
            CancelResponse();
            // 2. Send Text.
            return InworldController.Client.SendTextTo(text, BrainName);
        }
        /// <summary>
        /// Send a narrative action to this character.
        /// </summary>
        /// <param name="narrative">the narrative text to send</param>
        public virtual bool SendNarrative(string narrative)
        {
            return InworldController.Client.SendNarrativeActionTo(narrative, BrainName);
        }
        /// <summary>
        /// Send the trigger to this character.
        /// Trigger is defined in the goals section of the character in Inworld Studio.
        /// </summary>
        /// <param name="trigger">the name of the trigger.</param>
        /// <param name="needCancelResponse">If checked, this sending process will interrupt the character's current speaking.</param>
        /// <param name="parameters">The parameters and values of the trigger.</param>
        public virtual bool SendTrigger(string trigger, bool needCancelResponse = true, Dictionary<string, string> parameters = null)
        {
            return InworldController.Client.SendTriggerTo(trigger.ToLower(), parameters, BrainName, needCancelResponse);
        }
        /// <summary>
        /// Enable target goal of this character.
        /// By default, all the goals are already enabled.
        /// </summary>
        /// <param name="goalName">the name of the goal to enable.</param>
        public virtual bool EnableGoal(string goalName) => InworldMessenger.EnableGoal(goalName, BrainName);
        /// <summary>
        /// Disable target goal of this character.
        /// </summary>
        /// <param name="goalName">the name of the goal to disable.</param>
        public virtual bool DisableGoal(string goalName) => InworldMessenger.DisableGoal(goalName, BrainName);
        /// <summary>
        /// Succeed a task performed by this character.
        /// </summary>
        /// <param name="taskID">the ID of the task which succeeded.</param>
        public virtual bool SucceedTask(string taskID) => InworldMessenger.SendTaskSucceeded(taskID, BrainName);
        /// <summary>
        /// Fail a task performed by this character.
        /// </summary>
        /// <param name="taskID">the ID of the task which failed.</param>
        /// <param name="reason">the reason explaining why this task failed (must be less than 100 characters).</param>
        public virtual bool FailTask(string taskID, string reason) => InworldMessenger.SendTaskFailed(taskID, reason, BrainName);
        /// <summary>
        /// Interrupt the current character's speaking.
        /// Ignore all the current incoming messages from the character.
        /// <param name="isHardCancelling">mark if the current playing utterance would be cancelled.</param>
        /// </summary>
        public virtual bool CancelResponse(bool isHardCancelling = true) => m_Interaction && m_Interaction.CancelResponse(isHardCancelling);

        /// <summary>
        /// Gradually lower the volume and call cancelresponse.
        /// </summary>
        /// <returns></returns>
        public virtual void CancelResponseAsync()
        {
            if (m_Interaction)
                StartCoroutine(m_Interaction.CancelResponseAsync());
        }
        protected virtual void Awake()
        {
            if (m_Interaction == null)
                m_Interaction = GetComponent<InworldInteraction>();
        }

        protected virtual void OnEnable()
        {
            InworldController.Client.OnStatusChanged += OnStatusChanged;
        }
        protected virtual void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.CharacterHandler.Unregister(this);
            InworldController.Client.OnStatusChanged -= OnStatusChanged;
        }
        protected virtual void OnDestroy()
        {
            if (!InworldController.Instance)
                return;
            InworldController.CharacterHandler.Unregister(this);
            m_CharacterEvents?.onCharacterDestroyed?.Invoke(BrainName);
        }

        protected virtual void OnStatusChanged(InworldConnectionStatus newStatus)
        {
            if (newStatus == InworldConnectionStatus.Idle)
            {
                Data.agentId = "";
            }
        }
        internal virtual void OnInteractionChanged(List<InworldPacket> packets)
        {
            foreach (InworldPacket packet in packets)
            {
                ProcessPacket(packet);
            }
        }

        internal virtual bool ProcessPacket(InworldPacket incomingPacket)
        {
            if (!incomingPacket.IsRelated(ID))
                return false;
                
            m_CharacterEvents.onPacketReceived.Invoke(incomingPacket);
            
            switch (incomingPacket)
            {
                case ActionPacket actionPacket:
                    HandleAction(actionPacket);
                    break;
                case AudioPacket audioPacket: // Already Played.
                    HandleLipSync(audioPacket);
                    break;
                case ControlPacket controlPacket: // Interaction_End
                    HandleControl(controlPacket);
                    break;
                case TextPacket textPacket:
                    HandleText(textPacket);
                    break;
                case EmotionPacket emotionPacket:
                    HandleEmotion(emotionPacket);
                    break;
                case CustomPacket customPacket:
                    HandleTrigger(customPacket);
                    break;
                default:
                    Debug.LogError($"Received Unknown {incomingPacket}");
                    break;
            }
            return true;
        }
        protected virtual void HandleControl(ControlPacket controlPacket)
        {
            if (controlPacket.Action != ControlType.INTERACTION_END)
                return;
            if (m_VerboseLog)
                InworldAI.Log($"{Name} Received Interaction End");
            Event.onInteractionEnd?.Invoke(BrainName);
        }
        protected virtual void HandleRelation(CustomPacket relationPacket)
        {
            RelationState tmp = new RelationState();
            foreach (TriggerParameter param in relationPacket.custom.parameters)
            {
                tmp.UpdateByTrigger(param);
            }
            CurrRelation = tmp;
        }

        protected virtual bool HandleText(TextPacket packet)
        {
            if (packet.text == null || string.IsNullOrWhiteSpace(packet.text.text))
                return false;
            
            if (packet.Source == SourceType.PLAYER)
            {
                CancelResponse();
                if (m_VerboseLog)
                    InworldAI.Log($"{InworldAI.User.Name}: {packet.text.text}");
                if (PlayerController.Instance)
                    PlayerController.Instance.onPlayerSpeaks.Invoke(packet.text.text);
            }
            if (packet.Source != SourceType.AGENT || !packet.IsSource(ID)) 
                return false;
            IsSpeaking = true;
            if (m_VerboseLog)
                InworldAI.Log($"{Name}: {packet.text.text}");
            Event.onCharacterSpeaks.Invoke(BrainName, packet.text.text);
            return true;
        }

        protected virtual bool HandleNextTurn()
        {
            if (!InworldController.Client)
                return false;
            return InworldController.Client.NextTurn();
        }
        protected virtual void HandleTask(CustomPacket taskPacket)
        {
            if (!InworldMessenger.GetTask(taskPacket, out string taskName)) return;

            if (m_VerboseLog)
            {
                string output = $"{Name} received Task: {taskName}";
                output = taskPacket.custom.parameters.Aggregate(output, (current, param) => current + $"\n{param.name}: {param.value}");
                InworldAI.Log(output);
            }
            m_CharacterEvents.onTaskReceived.Invoke(BrainName, taskName, taskPacket.custom.parameters);
        }
        
        protected virtual bool HandleEmotion(EmotionPacket packet)
        {
            if (!packet.IsSource(ID) && !packet.IsTarget(ID))
                return false;
            if (m_VerboseLog)
                InworldAI.Log($"{Name}: {packet.emotion.behavior} {packet.emotion.strength}");
            m_CharacterEvents.onEmotionChanged.Invoke(BrainName, packet.emotion.ToString());
            return true;
        }
        protected virtual bool HandleTrigger(CustomPacket customPacket)
        {
            if (!customPacket.IsSource(ID) && !customPacket.IsTarget(ID))
                return false;
            switch (customPacket.Message)
            {
                case InworldMessage.RelationUpdate:
                    HandleRelation(customPacket);
                    return true;
                case InworldMessage.Task:
                    HandleTask(customPacket);
                    return true;
            }
            if (m_VerboseLog)
            {
                string output = $"{Name}: Received Trigger {customPacket.custom.name}";
                output = customPacket.custom.parameters.Aggregate(output, (current, param) => current + $" With {param.name}: {param.value}");
                InworldAI.Log(output);
            }
            m_CharacterEvents.onGoalCompleted.Invoke(BrainName, customPacket.TriggerName);
            return true;
        }
        protected virtual void HandleAction(ActionPacket actionPacket)
        {
            if (m_VerboseLog && (actionPacket.IsSource(ID) || actionPacket.IsTarget(ID)))
                InworldAI.Log($"{Name} {actionPacket.action.narratedAction.content}");
        }
        protected virtual void HandleLipSync(AudioPacket audioPacket)
        {
            // Won't process lip sync in pure text 2D conversation
        }
    }
}

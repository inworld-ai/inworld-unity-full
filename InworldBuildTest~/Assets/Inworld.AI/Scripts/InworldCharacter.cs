/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Interactions;
using Inworld.Packet;
using Inworld.Entities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Inworld
{
    [RequireComponent(typeof(InworldInteraction))]
    public class InworldCharacter : MonoBehaviour
    {
        [SerializeField] protected InworldCharacterData m_Data;
        [SerializeField] bool m_VerboseLog;
       
        public UnityEvent onBeginSpeaking;
        public UnityEvent onEndSpeaking;
        public UnityEvent<InworldPacket> onPacketReceived;
        public UnityEvent<string ,string> onCharacterSpeaks;
        public UnityEvent<string, string> onEmotionChanged;
        public UnityEvent<string> onGoalCompleted;
        public UnityEvent onRelationUpdated;
        
        RelationState m_CurrentRelation = new RelationState();
        protected InworldInteraction m_Interaction;
        /// <summary>
        /// Gets/Sets if this character is speaking.
        /// </summary>
        public bool IsSpeaking
        {
            get => m_Interaction && m_Interaction.IsSpeaking;
            protected set
            {
                if (!m_Interaction)
                    return;
                m_Interaction.IsSpeaking = value;
            }
        }
        /// <summary>
        /// Gets/Sets the charater's current relationship towards players. Will invoke onRelationUpdated when set.
        /// </summary>
        public RelationState CurrRelation
        {
            get => m_CurrentRelation;
            set
            {
                if (m_VerboseLog)
                    InworldAI.Log($"{Name}: {m_CurrentRelation.GetUpdate(value)}");
                m_CurrentRelation = value;
                onRelationUpdated.Invoke();
            }
        }
        /// <summary>
        /// Gets/Sets the character's data.
        /// If set, it'll also allocate the live session ID to the character's `InworldInteraction` component.
        /// </summary>
        public InworldCharacterData Data
        {
            get => m_Data;
            set
            {
                m_Data = value;
                if (!string.IsNullOrEmpty(m_Data.agentId))
                    m_Interaction.LiveSessionID = m_Data.agentId;
            }
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
        public string BrainName => Data?.brainName ?? "";
        /// <summary>
        /// Gets the live session ID of the character. If not registered, will try to fetch one from InworldController's CharacterHandler.
        /// </summary>
        public string ID => string.IsNullOrEmpty(Data?.agentId) ? InworldController.CharacterHandler.GetLiveSessionID(this) : Data?.agentId;
        
        /// <summary>
        /// Register live session. Get the live session ID for this character, and also assign it to this character's components.
        /// </summary>
        public virtual void RegisterLiveSession()
        {
            m_Interaction.LiveSessionID = Data.agentId = InworldController.CharacterHandler.GetLiveSessionID(this);
            if (!InworldController.CurrentCharacter && !string.IsNullOrEmpty(m_Interaction.LiveSessionID))
                InworldController.CharacterHandler.CurrentCharacter = this;
        }
        /// <summary>
        /// Send the message to this character.
        /// </summary>
        /// <param name="text">the message to send</param>
        public virtual void SendText(string text)
        {
            // 1. Interrupt current speaking.
            CancelResponse();
            // 2. Send Text.
            InworldController.Instance.SendText(ID, text);
        }
        /// <summary>
        /// Default Version of sending triggers that can be used in UnityEvent registration.
        /// Parameters and values not supported.
        /// Always Cancelling current response.
        /// </summary>
        /// <param name="trigger">the name of the trigger.</param>
        public virtual void SendTrigger(string trigger) => SendTrigger(trigger, true);

        /// <summary>
        /// Send the trigger to this character.
        /// Trigger is defined in the goals section of the character in Inworld Studio.
        /// </summary>
        /// <param name="trigger">the name of the trigger.</param>
        /// <param name="needCancelResponse">If checked, this sending process will interrupt the character's current speaking.</param>
        /// <param name="parameters">The parameters and values of the trigger.</param>
        public virtual void SendTrigger(string trigger, bool needCancelResponse, Dictionary<string, string> parameters = null)
        {
            // 1. Interrupt current speaking.
            if (needCancelResponse)
                CancelResponse();
            // 2. Send Text. YAN: Now all trigger has to be lower cases.
            InworldController.Instance.SendTrigger(trigger.ToLower(), ID, parameters);
        }
        /// <summary>
        /// Enable target goal of this character.
        /// By default, all the goals are already enabled.
        /// </summary>
        /// <param name="goalName">the name of the goal to enable.</param>
        public virtual void EnableGoal(string goalName) => InworldController.Instance.SendTrigger($"inworld.goal.enable.{goalName}", ID);
        /// <summary>
        /// Disable target goal of this character.
        /// </summary>
        /// <param name="goalName">the name of the goal to disable.</param>
        public virtual void DisableGoal(string goalName) => InworldController.Instance.SendTrigger($"inworld.goal.disable.{goalName}", ID);
        /// <summary>
        /// Interrupt the current character's speaking.
        /// Ignore all the current incoming messages from the character.
        /// </summary>
        public virtual void CancelResponse() => m_Interaction.CancelResponse();
        protected virtual void Awake()
        {
            if (m_Interaction == null)
                m_Interaction = GetComponent<InworldInteraction>();
        }

        protected virtual void OnEnable()
        {
            InworldController.CharacterHandler.OnCharacterRegistered += OnCharRegistered;
            InworldController.Client.OnStatusChanged += OnStatusChanged;
            m_Interaction.OnStartStopInteraction += OnStartStopInteraction;
            // YAN: This event is for handling global packets. Please only use it in InworldCharacter.
            //      For customized integration, please use InworldController.Instance.OnCharacterInteraction
            m_Interaction.OnInteractionChanged += OnInteractionChanged;
        }
        protected virtual void OnDisable()
        {
            m_Interaction.OnStartStopInteraction -= OnStartStopInteraction;
            m_Interaction.OnInteractionChanged -= OnInteractionChanged;
            if (!InworldController.Instance)
                return;
            InworldController.CharacterHandler.OnCharacterRegistered -= OnCharRegistered;
            InworldController.Client.OnStatusChanged -= OnStatusChanged;
        }
        protected virtual void OnStartStopInteraction(bool isStarting)
        {
            if (isStarting)
            {
                if (m_VerboseLog)
                    InworldAI.Log($"{Name} Starts Speaking");
                onBeginSpeaking.Invoke();
            }
            else
            {
                if (m_VerboseLog)
                    InworldAI.Log($"{Name} Ends Speaking");
                onEndSpeaking.Invoke();
            }
        }
        protected virtual void OnCharRegistered(InworldCharacterData charData)
        {
            
        }
        protected virtual void OnStatusChanged(InworldConnectionStatus newStatus)
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
            onPacketReceived.Invoke(incomingPacket);
            InworldController.Instance.CharacterInteract(incomingPacket);
            
            switch (incomingPacket)
            {
                case ActionPacket actionPacket:
                    HandleAction(actionPacket);
                    break;
                case AudioPacket audioPacket: // Already Played.
                    HandleLipSync(audioPacket);
                    break;
                case ControlPacket controlPacket: // Interaction_End
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
                case RelationPacket relationPacket:
                    HandleRelation(relationPacket);
                    break;
                default:
                    Debug.LogError($"Received Unknown {incomingPacket}");
                    break;
            }
        }
        protected virtual void HandleRelation(RelationPacket relationPacket) => CurrRelation = relationPacket.debugInfo.relation.relationUpdate;

        protected virtual void HandleText(TextPacket packet)
        {
            if (packet.text == null || string.IsNullOrEmpty(packet.text.text) || string.IsNullOrWhiteSpace(packet.text.text))
                return;
            switch (packet.routing.source.type.ToUpper())
            {
                case "AGENT":
                    IsSpeaking = true;
                    if (m_VerboseLog)
                        InworldAI.Log($"{Name}: {packet.text.text}");
                    onCharacterSpeaks.Invoke(packet.routing.source.name, packet.text.text);
                    break;
                case "PLAYER":
                    if (m_VerboseLog)
                        InworldAI.Log($"{InworldAI.User.Name}: {packet.text.text}");
                    onCharacterSpeaks.Invoke(InworldAI.User.Name, packet.text.text);
                    CancelResponse();
                    break;
            }
        }
        protected virtual void HandleEmotion(EmotionPacket packet)
        {
            if (m_VerboseLog)
                InworldAI.Log($"{Name}: {packet.emotion.behavior} {packet.emotion.strength}");
            onEmotionChanged.Invoke(packet.emotion.strength, packet.emotion.behavior);
        }
        protected virtual void HandleTrigger(CustomPacket customPacket)
        {
            if (m_VerboseLog)
            {
                InworldAI.Log($"{Name}: Received Trigger {customPacket.custom.name}");
                foreach (TriggerParameter param in customPacket.custom.parameters)
                {
                    InworldAI.Log($"With {param.name}: {param.value}");
                }
            }
            onGoalCompleted.Invoke(customPacket.TriggerName);
        }
        protected virtual void HandleAction(ActionPacket actionPacket)
        {
            if (m_VerboseLog)
                InworldAI.Log($"{Name} {actionPacket.action.narratedAction.content}");
        }
        protected virtual void HandleLipSync(AudioPacket audioPacket)
        {
            // Won't process lip sync in pure text 2D conversation
        }
    }
}

using Inworld.Interactions;
using Inworld.Packet;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Inworld
{
    [RequireComponent(typeof(InworldInteraction))]
    public class InworldCharacter : MonoBehaviour
    {
        [SerializeField] InworldCharacterData m_Data;
        [SerializeField] bool m_VerboseLog;
        
        // ReSharper disable all InconsistentNaming
        public UnityEvent onBeginSpeaking;
        public UnityEvent onEndSpeaking;
        public UnityEvent<InworldPacket> onPacketReceived;
        public UnityEvent<string ,string> onCharacterSpeaks;
        public UnityEvent<string, string> onEmotionChanged;
        public UnityEvent<string> onGoalCompleted;
        
        protected InworldInteraction m_Interaction;
        public bool IsSpeaking
        {
            get => m_Interaction && m_Interaction.IsSpeaking;
            internal set
            {
                if (!m_Interaction)
                    return;
                m_Interaction.IsSpeaking = value;
            }
        }
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
        public string Name => Data?.givenName ?? "";
        public string BrainName => Data?.brainName ?? "";
        public string ID => string.IsNullOrEmpty(Data?.agentId) ? InworldController.Instance.GetLiveSessionID(this) : Data?.agentId;
        public void RegisterLiveSession()
        {
            m_Interaction.LiveSessionID = Data.agentId = InworldController.Instance.GetLiveSessionID(this);
            if (InworldController.Status == InworldConnectionStatus.Connected && !InworldController.Instance.CurrentCharacter)
                InworldController.Instance.CurrentCharacter = this;
        }

        void Awake()
        {
            m_Interaction ??= GetComponent<InworldInteraction>();
        }

        protected virtual void OnEnable()
        {
            InworldController.Instance.OnCharacterRegistered += OnCharRegistered;
            InworldController.Instance.OnCharacterChanged += OnCharChanged;
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
            InworldController.Instance.OnCharacterRegistered -= OnCharRegistered;
            InworldController.Instance.OnCharacterChanged -= OnCharChanged;
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
        protected virtual void OnCharChanged(InworldCharacter oldChar, InworldCharacter newChar) {}
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
                case AudioPacket audioPacket: // Already Played.
                    HandleLipSync(audioPacket);
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
        }
        
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
                foreach (TriggerParamer param in customPacket.custom.parameters)
                {
                    InworldAI.Log($"With {param.name}: {param.value}");
                }
            }
            onGoalCompleted.Invoke(customPacket.TriggerName);
        }
        protected virtual void HandleLipSync(AudioPacket audioPacket)
        {
            // Won't process lip sync in pure text 2D conversation
        }
        public void CancelResponse() => m_Interaction.CancelResponse();
    }
}

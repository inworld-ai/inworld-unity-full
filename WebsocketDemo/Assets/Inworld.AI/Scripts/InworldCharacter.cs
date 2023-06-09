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
        [SerializeField] bool m_DebugMode;
        [SerializeField] InworldCharacterData m_Data;
        
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

        public void RegisterLiveSession() => m_Interaction.LiveSessionID = InworldController.Instance.GetLiveSessionID(Data.brainName);

        void Awake()
        {
            m_Interaction ??= GetComponent<InworldInteraction>();
        }

        protected virtual void OnEnable()
        {
            InworldController.Instance.OnCharacterRegistered += OnCharRegistered;
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
            InworldController.Instance.OnCharacterRegistered -= OnCharRegistered;
            InworldController.Instance.OnCharacterChanged -= OnCharChanged;
        }
        protected virtual void OnStartStopInteraction(bool isStarting)
        {
            if (isStarting)
                onBeginSpeaking.Invoke();
            else
                onEndSpeaking.Invoke();
        }
        protected virtual void OnCharRegistered(InworldCharacterData charData)
        {
            if (charData.brainName == Data.brainName)
                m_Interaction.LiveSessionID = charData.agentId;
        }
        protected virtual void OnCharChanged(InworldCharacterData oldChar, InworldCharacterData newChar) {}

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
                    Debug.Log($"Received {incomingPacket}");
                    break;
            }
        }
        
        protected virtual void HandleText(TextPacket packet)
        {
            if (packet.text == null || string.IsNullOrEmpty(packet.text.text))
                return;
            switch (packet.routing.source.type)
            {
                case "AGENT":
                    IsSpeaking = true;
                    if (m_DebugMode)
                        Debug.Log($"{packet.routing.source.name}: {packet.text.text}");
                    onCharacterSpeaks.Invoke(packet.routing.source.name, packet.text.text);
                    break;
                case "PLAYER":
                    if (m_DebugMode)
                        Debug.Log($"{InworldController.Player}: {packet.text.text}");
                    onCharacterSpeaks.Invoke(InworldController.Player, packet.text.text);
                    break;
            }
        }
        protected virtual void HandleEmotion(EmotionPacket packet)
        {
            if (m_DebugMode)
                Debug.Log($"{packet.routing.source.name}: {packet}");
            onEmotionChanged.Invoke(packet.emotion.strength, packet.emotion.behavior);
        }
        
        protected virtual void HandleTrigger(CustomPacket customPacket)
        {
            if (m_DebugMode)
            {
                Debug.Log($"Received Trigger {customPacket.custom.name}");
                foreach (TriggerParamer param in customPacket.custom.parameters)
                {
                    Debug.Log($"With Param {param.name}: {param.value}");
                }
            }
            onGoalCompleted.Invoke(customPacket.custom.name);
        }
        protected virtual void HandleLipSync(AudioPacket audioPacket)
        {
            if (m_DebugMode)
                Debug.Log($"Won't process lip sync in pure text 2D conversation");
        }
    }
}

using Inworld.Interactions;
using Inworld.Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Inworld
{
    [RequireComponent(typeof(InworldInteraction))]
    public class InworldCharacter : MonoBehaviour
    {
        [SerializeField] InworldCharacterData m_Data;
        [SerializeField] bool m_VerboseLog;
        
        [Header("Sight")]
        [Range(1, 180)]
        [SerializeField] float m_SightAngle = 90f;
        [Range(1, 30)]
        [SerializeField] float m_SightDistance = 10f;
        [SerializeField] float m_SightRefreshRate = 0.25f;
        /// <summary>
        ///     Returns the priority of the character.
        ///     the higher the Priority is, the character is more likely responding to player.
        /// </summary>
        public float Priority;

        
        public UnityEvent onBeginSpeaking;
        public UnityEvent onEndSpeaking;
        public UnityEvent<InworldPacket> onPacketReceived;
        public UnityEvent<string ,string> onCharacterSpeaks;
        public UnityEvent<string, string> onEmotionChanged;
        public UnityEvent<string> onGoalCompleted;
        
        [FormerlySerializedAs("m_Interaction")] public InworldInteraction Interaction;
        public bool IsSpeaking
        {
            get => Interaction && Interaction.IsSpeaking;
            internal set
            {
                if (!Interaction)
                    return;
                Interaction.IsSpeaking = value;
            }
        }
        public InworldCharacterData Data
        {
            get => m_Data;
            set
            {
                m_Data = value;
                if (!string.IsNullOrEmpty(m_Data.agentId))
                    Interaction.LiveSessionID = m_Data.agentId;
            }
        }
        public string Name => Data?.givenName ?? "";
        public string BrainName => Data?.brainName ?? "";
        public string ID => Data?.agentId ?? InworldController.Instance.GetLiveSessionID(this);
        public void RegisterLiveSession()
        {
            Interaction.LiveSessionID = Data.agentId = InworldController.Instance.GetLiveSessionID(this);
        }

        void Awake()
        {
            Interaction ??= GetComponent<InworldInteraction>();
        }

        protected virtual void OnEnable()
        {
            InworldController.Instance.OnCharacterRegistered += OnCharRegistered;
            InworldController.Instance.OnCharacterChanged += OnCharChanged;
            InworldController.Client.OnStatusChanged += OnStatusChanged;
            Interaction.OnStartStopInteraction += OnStartStopInteraction;
            Interaction.OnInteractionChanged += OnInteractionChanged;
            StartCoroutine(CheckPriority());
        }

        protected virtual void OnDisable()
        {
            Interaction.OnStartStopInteraction -= OnStartStopInteraction;
            Interaction.OnInteractionChanged -= OnInteractionChanged;
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnCharacterRegistered -= OnCharRegistered;
            InworldController.Instance.OnCharacterChanged -= OnCharChanged;
            InworldController.Client.OnStatusChanged -= OnStatusChanged;
            StopCoroutine(CheckPriority());
        }
        
        IEnumerator CheckPriority()
        {
            // YAN: Update refreshed too fast. Use Coroutine for better performance.
            while (true)
            {
                if (InworldController.Instance)
                {
                    Transform trCharacter = transform;
                    Transform trPlayer = Camera.main.transform;
                    Priority = Vector3.Distance(trCharacter.position, trPlayer.position);
                    if (Priority > m_SightDistance)
                        Priority = -1f;
                    else
                    {
                        Vector3 vecDirection = (trPlayer.position - trCharacter.position).normalized;
                        float fAngle = Vector3.Angle(vecDirection, trCharacter.forward);
                        if (fAngle > m_SightAngle * 0.5f)
                        {
                            Priority = -1f;
                        }
                        else
                        {
                            Vector3 vecPlayerDirection = -vecDirection;
                            Priority = Vector3.Angle(vecPlayerDirection, trPlayer.forward);
                        }
                    }
                }
                yield return new WaitForSeconds(m_SightRefreshRate);
            }
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
                RegisterLiveSession();
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
            if (packet.text == null || string.IsNullOrEmpty(packet.text.text))
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
            onGoalCompleted.Invoke(customPacket.custom.name);
        }
        protected virtual void HandleLipSync(AudioPacket audioPacket)
        {
            // Won't process lip sync in pure text 2D conversation
        }
        public void CancelResponse() => Interaction.CancelResponse();
    }
}

using System.Collections.Generic;
using Inworld;
using Inworld.UI;
using Inworld.Packet;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Inworld
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Audio Capture")]
        [SerializeField] protected bool m_PushToTalk;
        [SerializeField] protected KeyCode m_PushToTalkKey = KeyCode.C;
        [Header("References")]
        [SerializeField] protected TMP_Text m_StatusText;
        [SerializeField] protected TMP_Text m_ConnectButtonText;
        [SerializeField] protected TMP_InputField m_InputField;
        [SerializeField] protected Button m_SendButton;
        [SerializeField] protected Button m_RecordButton;
        [SerializeField] protected Button m_ConnectButton;
        protected string m_CurrentEmotion;
        protected bool m_PTTKeyPressed;
        protected bool m_BlockAudioHandling;
        
        // YAN: Direct 2D Sample.
        public void ConnectInworld()
        {
            if (InworldController.Status == InworldConnectionStatus.Idle)
                InworldController.Instance.Reconnect();
            else if (InworldController.Status == InworldConnectionStatus.Connected)
                InworldController.Instance.Disconnect();
        }
        
        public void SendText()
        {
            if (!m_InputField || string.IsNullOrEmpty(m_InputField.text) || !CharacterHandler.Instance.CurrentCharacter)
                return;
            try
            {
                CharacterHandler.Instance.SendText(m_InputField.text);
                m_InputField.text = "";
            } catch(InworldException e) {}
        }

        protected virtual void Awake()
        {
            if (m_SendButton)
                m_SendButton.interactable = false;
            if (m_RecordButton)
                m_RecordButton.interactable = false;
        }
        protected virtual void Start()
        {
            CharacterHandler.Instance.ManualAudioHandling = m_PushToTalk;
            AudioCapture.Instance.AutoPush = !m_PushToTalk;
        }
        protected virtual void OnEnable()
        {
            InworldController.Client.OnStatusChanged += OnStatusChanged;
            CharacterHandler.Instance.OnCharacterChanged += OnCharacterChanged;
            InworldController.Instance.OnCharacterInteraction += OnInteraction;
        }
        protected virtual void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Client.OnStatusChanged -= OnStatusChanged;
            CharacterHandler.Instance.OnCharacterChanged -= OnCharacterChanged;
            InworldController.Instance.OnCharacterInteraction -= OnInteraction;
        }
        
        protected virtual void OnStatusChanged(InworldConnectionStatus newStatus)
        {
            if(m_ConnectButton)
                m_ConnectButton.interactable = newStatus == InworldConnectionStatus.Idle || newStatus == InworldConnectionStatus.Connected;
            if(m_ConnectButtonText)
                m_ConnectButtonText.text = newStatus == InworldConnectionStatus.Connected ? "DISCONNECT" : "CONNECT";
            if (newStatus == InworldConnectionStatus.Connected && CharacterHandler.Instance.CurrentCharacter)
            {
                if (m_SendButton)
                    m_SendButton.interactable = true;
                if (m_RecordButton)
                    m_RecordButton.interactable = true;
                if (m_StatusText)
                    m_StatusText.text = $"Current: {CharacterHandler.Instance.CurrentCharacter.Name}";
                if (m_PushToTalk && m_PTTKeyPressed && !m_BlockAudioHandling)
                    CharacterHandler.Instance.StartAudio();
            }
            else
            {
                if (m_SendButton)
                    m_SendButton.interactable = false;
                if (m_RecordButton)
                    m_RecordButton.interactable = false;
                if (m_StatusText)
                    m_StatusText.text = newStatus.ToString();
                if (m_PushToTalk && !m_PTTKeyPressed && !m_BlockAudioHandling)
                    CharacterHandler.Instance.StopAudio();
            }
            
            if (newStatus == InworldConnectionStatus.Error)
            {
                if(m_StatusText)
                    m_StatusText.text = InworldController.Client.Error;
            }
        }

        protected virtual void OnCharacterChanged(InworldCharacter oldChar, InworldCharacter newChar)
        {
            if(m_RecordButton)
                m_RecordButton.interactable = InworldController.Status == InworldConnectionStatus.Connected && CharacterHandler.Instance.CurrentCharacter;
            if(m_SendButton)
                m_SendButton.interactable = InworldController.Status == InworldConnectionStatus.Connected && CharacterHandler.Instance.CurrentCharacter;
            if (newChar != null)
            {
                InworldAI.Log($"Now Talking to: {newChar.Name}");
                if (m_StatusText)
                    m_StatusText.text = $"Current: {newChar.Name}";
                if (m_PushToTalk && m_PTTKeyPressed && !m_BlockAudioHandling)
                    CharacterHandler.Instance.StartAudio();
            }
        }

        protected void OnInteraction(InworldPacket incomingPacket)
        {
            switch (incomingPacket)
            {
                case AudioPacket audioPacket: // Already Played.
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
                    InworldAI.Log($"Received {incomingPacket}");
                    break;
            }
        }
        protected virtual void HandleTrigger(CustomPacket customPacket)
        {
            InworldAI.Log($"Received Trigger {customPacket.Trigger}");
        }
        protected virtual void HandleEmotion(EmotionPacket packet) => m_CurrentEmotion = packet.emotion.ToString();

        protected virtual void HandleText(TextPacket packet)
        {

        }
        
        protected virtual void Update()
        {
            if(m_PushToTalk && !m_BlockAudioHandling)
                HandlePTT();
            HandleInput();
        }
        
        protected virtual void HandlePTT()
        {
            if (Input.GetKeyDown(m_PushToTalkKey))
            {
                m_PTTKeyPressed = true;
                CharacterHandler.Instance.StartAudio();
            }
            else if (Input.GetKeyUp(m_PushToTalkKey))
            {
                m_PTTKeyPressed = false;
                CharacterHandler.Instance.StopAudio();
            }
        }

        protected virtual void HandleInput()
        {
            if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
                SendText();
        }
    }
}


/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System.Collections.Generic;
using Inworld.Packet;
using Inworld.Entities;
using Inworld.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inworld.Sample
{
    /// <summary>
    /// This is the demo use case for how to interact with Inworld.
    /// For developers please feel free to create your own.
    /// </summary>
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
        [SerializeField] protected RectTransform m_BubbleContentAnchor;
        [SerializeField] protected ChatBubble m_BubbleLeftPrefab;
        [SerializeField] protected ChatBubble m_BubbleRightPrefab;
        [Space(10)][SerializeField] protected bool m_DisplaySplash;
        
        protected string m_CurrentEmotion;
        protected bool m_PTTKeyPressed;
        protected bool m_BlockAudioHandling;
        readonly protected Dictionary<string, ChatBubble> m_Bubbles = new Dictionary<string, ChatBubble>();

        /// <summary>
        /// Control the InworldController to connect inworld server.
        /// </summary>
        public void ConnectInworld()
        {
            if (InworldController.Status == InworldConnectionStatus.Idle)
                InworldController.Instance.Reconnect();
            else if (InworldController.Status == InworldConnectionStatus.Connected)
                InworldController.Instance.Disconnect();
        }
        /// <summary>
        /// Send target message in the input field.
        /// </summary>
        public void SendText()
        {
            if (!m_InputField || string.IsNullOrEmpty(m_InputField.text) || !InworldController.CurrentCharacter)
                return;
            try
            {
                if (InworldController.CurrentCharacter)
                    InworldController.CurrentCharacter.SendText(m_InputField.text);
                m_InputField.text = "";
            }
            catch (InworldException e)
            {
                InworldAI.LogWarning($"Failed to send text: {e}");
            }
        }

        protected virtual void Awake()
        {
            if (m_SendButton)
                m_SendButton.interactable = false;
            if (m_RecordButton)
                m_RecordButton.interactable = false;
            if (m_DisplaySplash && InworldController.IsAutoStart && !SplashScreen.Instance && InworldAI.SplashScreen)
                Instantiate(InworldAI.SplashScreen);
        }
        protected virtual void Start()
        {
            InworldController.CharacterHandler.ManualAudioHandling = m_PushToTalk;
            InworldController.Audio.AutoPush = !m_PushToTalk;
        }
        protected virtual void OnEnable()
        {
            InworldController.Client.OnStatusChanged += OnStatusChanged;
            InworldController.CharacterHandler.OnCharacterChanged += OnCharacterChanged;
            InworldController.Instance.OnCharacterInteraction += OnInteraction;
        }
        protected virtual void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Client.OnStatusChanged -= OnStatusChanged;
            InworldController.CharacterHandler.OnCharacterChanged -= OnCharacterChanged;
            InworldController.Instance.OnCharacterInteraction -= OnInteraction;
        }
        
        protected virtual void OnStatusChanged(InworldConnectionStatus newStatus)
        {
            if(m_ConnectButton)
                m_ConnectButton.interactable = newStatus == InworldConnectionStatus.Idle || newStatus == InworldConnectionStatus.Connected;
            if(m_ConnectButtonText)
                m_ConnectButtonText.text = newStatus == InworldConnectionStatus.Connected ? "DISCONNECT" : "CONNECT";
            if (newStatus == InworldConnectionStatus.Connected && InworldController.CurrentCharacter)
            {
                if (m_SendButton)
                    m_SendButton.interactable = true;
                if (m_RecordButton)
                    m_RecordButton.interactable = true;
                if (m_StatusText)
                    m_StatusText.text = $"Current: {InworldController.CurrentCharacter.Name}";
                if (m_PushToTalk && m_PTTKeyPressed && !m_BlockAudioHandling)
                    InworldController.Instance.StartAudio();
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
                    InworldController.Instance.StopAudio();
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
                m_RecordButton.interactable = InworldController.Status == InworldConnectionStatus.Connected && InworldController.CurrentCharacter;
            if(m_SendButton)
                m_SendButton.interactable = InworldController.Status == InworldConnectionStatus.Connected && InworldController.CurrentCharacter;
            if (newChar == null)
                return;
            InworldAI.Log($"Now Talking to: {newChar.Name}");
            if (m_StatusText)
                m_StatusText.text = $"Current: {newChar.Name}";
            if (m_PushToTalk && m_PTTKeyPressed && !m_BlockAudioHandling)
                InworldController.Instance.StartAudio();
        }

        protected void OnInteraction(InworldPacket incomingPacket)
        {
            switch (incomingPacket)
            {
                case AudioPacket audioPacket: // Already Played.
                    break;
                case ActionPacket actionPacket:
                    HandleAction(actionPacket);
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
                case ControlPacket controlEvent:
                    break;
                case RelationPacket relationPacket:
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

        protected virtual void HandleAction(ActionPacket packet)
        {
            if (packet.action == null || packet.action.narratedAction == null || string.IsNullOrWhiteSpace(packet.action.narratedAction.content) || m_Bubbles == null || !m_BubbleContentAnchor || !m_BubbleLeftPrefab || !m_BubbleRightPrefab)
                return;
            m_Bubbles[packet.packetId.utteranceId] = Instantiate(m_BubbleLeftPrefab, m_BubbleContentAnchor);
            InworldCharacterData charData = InworldController.CharacterHandler.GetCharacterDataByID(packet.routing.source.name);
            if (charData != null)
            {
                string charName = charData.givenName ?? "Character";
                string title = $"{charName}: {m_CurrentEmotion}";
                Texture2D thumbnail = charData.thumbnail ? charData.thumbnail : InworldAI.DefaultThumbnail;
                m_Bubbles[packet.packetId.utteranceId].SetBubble(title, thumbnail);
            }
            m_Bubbles[packet.packetId.utteranceId].Text = $"<i>{packet.action.narratedAction.content}</i>";
            SetContentHeight(m_BubbleContentAnchor, m_BubbleRightPrefab);
        }
        protected virtual void HandleText(TextPacket packet)
        {
            if (packet.text == null || string.IsNullOrEmpty(packet.text.text) || string.IsNullOrWhiteSpace(packet.text.text) || !m_BubbleContentAnchor || !m_BubbleLeftPrefab || !m_BubbleRightPrefab)
                return;
            switch (packet.routing.source.type.ToUpper())
            {
                case "AGENT":
                    if (!m_Bubbles.ContainsKey(packet.packetId.utteranceId))
                    {
                        m_Bubbles[packet.packetId.utteranceId] = Instantiate(m_BubbleLeftPrefab, m_BubbleContentAnchor);
                        InworldCharacterData charData = InworldController.CharacterHandler.GetCharacterDataByID(packet.routing.source.name);
                        if (charData != null)
                        {
                            string charName = charData.givenName ?? "Character";
                            string title = $"{charName}: {m_CurrentEmotion}";
                            Texture2D thumbnail = charData.thumbnail ? charData.thumbnail : InworldAI.DefaultThumbnail;
                            m_Bubbles[packet.packetId.utteranceId].SetBubble(title, thumbnail);
                        }
                    }
                    break;
                case "PLAYER":
                    if (!m_Bubbles.ContainsKey(packet.packetId.utteranceId))
                    {
                        m_Bubbles[packet.packetId.utteranceId] = Instantiate(m_BubbleRightPrefab, m_BubbleContentAnchor);
                        m_Bubbles[packet.packetId.utteranceId].SetBubble(InworldAI.User.Name, InworldAI.DefaultThumbnail);
                    }
                    break;
            }
            m_Bubbles[packet.packetId.utteranceId].Text = packet.text.text;
            SetContentHeight(m_BubbleContentAnchor, m_BubbleRightPrefab);
        }
        
        protected virtual void SetContentHeight(RectTransform scrollAnchor, InworldUIElement element)
        {
            if (!m_BubbleContentAnchor)
                return;
            scrollAnchor.sizeDelta = new Vector2(m_BubbleContentAnchor.sizeDelta.x, scrollAnchor.childCount * element.Height);
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
                InworldController.Instance.StartAudio();
            }
            else if (Input.GetKeyUp(m_PushToTalkKey))
            {
                m_PTTKeyPressed = false;
                InworldController.Instance.PushAudio();
            }
        }

        protected virtual void HandleInput()
        {
            if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
                SendText();
        }
    }
}


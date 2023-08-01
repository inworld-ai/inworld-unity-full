using System.Collections.Generic;
using Inworld;
using Inworld.UI;
using Inworld.Packet;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] RectTransform m_CharButtonAnchor;
    [SerializeField] RectTransform m_ContentRT;
    [SerializeField] CharacterButton m_CharSelector;
    [SerializeField] ChatBubble m_BubbleLeft;
    [SerializeField] ChatBubble m_BubbleRight;
    [SerializeField] protected TMP_Text m_StatusText;
    [SerializeField] protected TMP_Text m_ConnectButtonText;
    [SerializeField] protected TMP_InputField m_InputField;
    [SerializeField] protected Button m_SendButton;
    [SerializeField] protected Button m_ConnectButton;
    readonly Dictionary<string, CharacterButton> m_Characters = new Dictionary<string, CharacterButton>();
    readonly protected Dictionary<string, ChatBubble> m_Bubbles = new Dictionary<string, ChatBubble>();
    protected string m_CurrentEmotion;
    void Awake()
    {
        if (m_SendButton)
            m_SendButton.interactable = false;
    }
    void OnEnable()
    {
        InworldController.Client.OnStatusChanged += OnStatusChanged;
        InworldController.Instance.OnCharacterRegistered += OnCharacterRegistered;
        InworldController.Instance.OnCharacterChanged += OnCharacterChanged;
        InworldController.Instance.OnCharacterInteraction += OnInteraction;
    }
    void OnDisable()
    {
        if (!InworldController.Instance)
            return;
        InworldController.Client.OnStatusChanged -= OnStatusChanged;
        InworldController.Instance.OnCharacterRegistered -= OnCharacterRegistered;
        InworldController.Instance.OnCharacterChanged -= OnCharacterChanged;
        InworldController.Instance.OnCharacterInteraction -= OnInteraction;
    }
    protected virtual void OnCharacterRegistered(InworldCharacterData charData)
    {
        if (!m_Characters.ContainsKey(charData.brainName))
            m_Characters[charData.brainName] = Instantiate(m_CharSelector, m_CharButtonAnchor);
        m_Characters[charData.brainName].SetData(charData);
        _SetContentHeight(m_CharButtonAnchor, m_CharSelector);
    }

    protected virtual void OnStatusChanged(InworldConnectionStatus newStatus)
    {
        m_ConnectButton.interactable = newStatus == InworldConnectionStatus.Idle || newStatus == InworldConnectionStatus.Connected;
        m_ConnectButtonText.text = newStatus == InworldConnectionStatus.Connected ? "DISCONNECT" : "CONNECT";

        if (newStatus == InworldConnectionStatus.Connected && InworldController.Instance.CurrentCharacter)
        {
            m_SendButton.interactable = true;
            if (!InworldController.IsRecording)
                InworldController.Instance.StartAudio(InworldController.Instance.CurrentCharacter.ID);
        }
        else
        {
            m_SendButton.interactable = false;
            if (InworldController.IsRecording)
                InworldController.Instance.StopAudio(InworldController.Instance.CurrentCharacter.ID);
        }
        
        if (m_StatusText)
            m_StatusText.text = newStatus.ToString();
        if (newStatus == InworldConnectionStatus.Error)
            m_StatusText.text = InworldController.Client.Error;
    }

    // YAN: Direct 2D Sample.
    public void ConnectInworld()
    {
        if (InworldController.Status == InworldConnectionStatus.Idle)
            InworldController.Instance.Reconnect();
        else if (InworldController.Status == InworldConnectionStatus.Connected)
            InworldController.Instance.Disconnect();
    }

    protected virtual void OnCharacterChanged(InworldCharacter oldChar, InworldCharacter newChar)
    {
        m_SendButton.interactable = newChar != null;
        if (newChar != null && m_StatusText)
            m_StatusText.text = $"Current: {newChar.Name}";
        StartCoroutine(_SwapAudioCapture(oldChar, newChar));
    }
    IEnumerator _SwapAudioCapture(InworldCharacter oldChar, InworldCharacter newchar)
    {
        if (oldChar != null && !string.IsNullOrEmpty(oldChar.ID))
            InworldController.Instance.StopAudio(oldChar.ID);
        yield return new WaitForFixedUpdate();
        if (newchar != null && !string.IsNullOrEmpty(newchar.ID))
        {
            InworldController.Instance.StartAudio(newchar.ID);
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
        if (packet.text == null || string.IsNullOrEmpty(packet.text.text) || string.IsNullOrWhiteSpace(packet.text.text))
                return;
        switch (packet.routing.source.type.ToUpper())
        {
            case "AGENT":
                if (!m_Bubbles.ContainsKey(packet.packetId.utteranceId))
                {
                    m_Bubbles[packet.packetId.utteranceId] = Instantiate(m_BubbleLeft, m_ContentRT);
                    InworldCharacterData charData = InworldController.Instance.GetCharacter(packet.routing.source.name);
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
                    m_Bubbles[packet.packetId.utteranceId] = Instantiate(m_BubbleRight, m_ContentRT);
                    m_Bubbles[packet.packetId.utteranceId].SetBubble(InworldAI.User.Name, InworldAI.DefaultThumbnail);
                }
                break;
        }
        m_Bubbles[packet.packetId.utteranceId].Text = packet.text.text;
        _SetContentHeight(m_ContentRT, m_BubbleRight);
    }

    void _SetContentHeight(RectTransform scrollAnchor, InworldUIElement element)
    {
        scrollAnchor.sizeDelta = new Vector2(m_ContentRT.sizeDelta.x, scrollAnchor.childCount * element.Height);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
            SendText();
    }
    public void SendText(bool isTrigger = false)
    {
        if (string.IsNullOrEmpty(m_InputField.text) || !m_SendButton.interactable)
            return;
        InworldController.Instance.SendText(m_InputField.text);
        m_InputField.text = "";
    }
}

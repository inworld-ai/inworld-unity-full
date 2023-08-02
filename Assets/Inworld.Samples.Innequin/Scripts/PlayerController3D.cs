using Inworld.Packet;
using TMPro;
using UnityEngine;

namespace Inworld.Sample.Innequin
{
    public class PlayerController3D : PlayerController
    {
        [SerializeField] GameObject m_ChatCanvas;
        [SerializeField] TMP_Text m_Subtitle;

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.BackQuote))
                m_ChatCanvas.SetActive(!m_ChatCanvas.activeSelf);
            if (!m_ChatCanvas.activeSelf)
                return;
            if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
                SendText();
        }
        protected override void OnCharacterRegistered(InworldCharacterData charData){}

        protected override void OnCharacterChanged(InworldCharacter oldChar, InworldCharacter newChar)
        {
            m_SendButton.interactable = InworldController.Status == InworldConnectionStatus.Connected && InworldController.Instance.CurrentCharacter;
            if (newChar != null && m_StatusText)
                m_StatusText.text = $"Current: {newChar.Name}";
        }
        protected override void HandleText(TextPacket packet)
        {
            if (packet.text == null || string.IsNullOrEmpty(packet.text.text))
                return;
            switch (packet.routing.source.type.ToUpper())
            {
                case "AGENT":
                    InworldCharacterData character = InworldController.Instance.GetCharacter(packet.routing.source.name);
                    string charName = character?.givenName ?? "Character";
                    string title = $"{charName}({m_CurrentEmotion}):";
                    m_Subtitle.text = $"{title} {packet.text.text}";
                    break;
                case "PLAYER":
                    m_Subtitle.text = $"{InworldAI.User.Name}: {packet.text.text}";
                    break;
            }
        }
        protected override void HandleTrigger(CustomPacket customPacket)
        {
            m_Subtitle.text = $"(Received {customPacket.Trigger})";
        }
        protected override void HandleEmotion(EmotionPacket packet) => m_CurrentEmotion = packet.emotion.ToString();
    }
}

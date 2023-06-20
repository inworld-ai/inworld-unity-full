using Inworld.Packet;
using TMPro;
using UnityEngine;

namespace Inworld.Sample
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

        protected override void HandleTrigger(CustomPacket customPacket)
        {
            m_Subtitle.text = $"(Received {customPacket.Trigger})";
        }
        protected override void HandleEmotion(EmotionPacket packet) => m_CurrentEmotion = packet.emotion.ToString();
    }
}

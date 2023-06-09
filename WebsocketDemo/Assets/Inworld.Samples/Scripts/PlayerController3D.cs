using Inworld.Packet;
using System.Linq;
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
        protected override void OnCharacterRegistered(InworldCharacterData charData) {}

        protected override void OnCharacterChanged(InworldCharacterData oldChar, InworldCharacterData newChar)
        {
            m_SendButton.interactable = newChar != null;
            if (newChar != null && m_StatusText)
                m_StatusText.text = $"Current: {newChar.givenName}";
        }

        protected override void HandleTrigger(CustomPacket customPacket)
        {
            string triggerContent = $"Received {customPacket.custom.name}";

            if (customPacket.custom.parameters.Count != 0)
                triggerContent += "> ";
            triggerContent = customPacket.custom.parameters.Aggregate(triggerContent, (current, param) => 
                                                                          current + $"{param.name}:{param.value} ");
            m_Subtitle.text = triggerContent;
        }
        protected override void HandleEmotion(EmotionPacket packet) => m_CurrentEmotion = packet.emotion.ToString();

        protected override void HandleText(TextPacket packet)
        {
            if (packet.text == null || string.IsNullOrEmpty(packet.text.text))
                return;
            switch (packet.routing.source.type)
            {
                case "AGENT":
                    InworldCharacterData character = InworldController.Instance.GetCharacter(packet.routing.source.name);
                    string charName = character?.givenName ?? "Character";
                    string title = $"{charName}({m_CurrentEmotion}):";
                    m_Subtitle.text = $"{title} {packet.text.text}";
                    break;
                case "PLAYER":
                    m_Subtitle.text = $"{InworldController.Player}: {packet.text.text}";
                    break;
            }
        }
    }
}

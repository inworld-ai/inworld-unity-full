using Inworld.Sample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inworld;
using Inworld.Packet;
using TMPro;

namespace Inworld.Sample.Innequin
{
    public class PlayerControllerInnequin : PlayerController3D
    {
        [SerializeField] TMP_Text m_Subtitle;
        protected override void HandleText(TextPacket packet)
        {
            if (!m_Subtitle)
                return;
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
    }
}
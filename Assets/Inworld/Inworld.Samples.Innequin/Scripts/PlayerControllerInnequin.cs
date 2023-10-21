/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using UnityEngine;
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
                    InworldCharacterData character = InworldController.CharacterHandler.GetCharacterDataByID(packet.routing.source.name);
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
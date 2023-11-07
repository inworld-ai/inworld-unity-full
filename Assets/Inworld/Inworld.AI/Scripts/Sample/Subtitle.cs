/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Entities;
using Inworld.Packet;
using TMPro;
using UnityEngine;

namespace Inworld.Sample
{
    /// <summary>
    /// A simple sample for only displaying subtitle
    /// </summary>
    public class Subtitle : MonoBehaviour
    {
        [SerializeField] TMP_Text m_Subtitle;
        
        string m_CurrentEmotion;
        string m_CurrentContent;
        void OnEnable()
        {
            InworldController.Instance.OnCharacterInteraction += OnInteraction;
        }
        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnCharacterInteraction -= OnInteraction;
        }
        protected virtual void OnInteraction(InworldPacket packet)
        {
            if (!m_Subtitle)
                return;
            if (!(packet is TextPacket playerPacket))
                return;
            switch (packet.routing.source.type.ToUpper())
            {
                case "PLAYER":
                    m_Subtitle.text = $"{InworldAI.User.Name}: {playerPacket.text.text}";
                    break;
                case "AGENT":
                    InworldCharacterData character = InworldController.CharacterHandler.GetCharacterDataByID(packet.routing.source.name);
                    if (character == null)
                        return;
                    m_Subtitle.text = $"{character.givenName}: {playerPacket.text.text}";
                    break;
            }
        }
    }
}

﻿/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Data;
using Inworld.Packet;
using TMPro;
using UnityEngine;

namespace Inworld.Sample
{
    /// <summary>
    /// A simple sample for only displaying subtitle
    /// </summary>
    public class Subtitle : StatusPanel
    {
        [SerializeField] TMP_Text m_Subtitle;
        
        string m_CurrentEmotion;
        string m_CurrentContent;
        protected override void OnEnable()
        {
            base.OnEnable();
            InworldController.Client.OnPacketSent += OnInteraction;
            InworldController.CharacterHandler.Event.onCharacterListJoined.AddListener(OnCharacterJoined);
            InworldController.CharacterHandler.Event.onCharacterListLeft.AddListener(OnCharacterLeft);
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            if (!InworldController.Instance)
                return;
            InworldController.Client.OnPacketSent -= OnInteraction;
            InworldController.CharacterHandler.Event.onCharacterListJoined.RemoveListener(OnCharacterJoined);
            InworldController.CharacterHandler.Event.onCharacterListLeft.RemoveListener(OnCharacterLeft);
        }
        protected virtual void OnCharacterJoined(InworldCharacter character)
        {
            if (InworldController.Instance && InworldController.CurrentCharacter == null)
                InworldController.CurrentCharacter = character;
            // YAN: Clear existing event listener to avoid adding multiple times.
            character.Event.onPacketReceived.RemoveListener(OnInteraction); 
            character.Event.onPacketReceived.AddListener(OnInteraction);
        }

        protected virtual void OnCharacterLeft(InworldCharacter character)
        {
            character.Event.onPacketReceived.RemoveListener(OnInteraction); 
        }
        protected virtual void OnInteraction(InworldPacket packet)
        {
            if (!m_Subtitle)
                return;
            if (!(packet is TextPacket playerPacket))
                return;
            switch (packet.Source)
            {
                case SourceType.PLAYER:
                    m_Subtitle.text = $"{InworldAI.User.Name}: {playerPacket.text.text}";
                    break;
                case SourceType.AGENT:
                    InworldCharacterData charData = InworldController.Client.GetCharacterDataByID(packet.routing.source.name);
                    if (charData != null)
                        m_Subtitle.text = $"{charData.givenName}: {playerPacket.text.text}";
                    break;
            }
        }
    }
}

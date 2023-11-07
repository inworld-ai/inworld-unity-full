/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Entities;
using Inworld.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Inworld.Sample
{
    public class ConnectPanel : BubblePanel
    {
        [SerializeField] Button m_ConnectButton;
        [SerializeField] CharacterButton m_CharSelectorPrefab;

        public override bool IsUIReady => base.IsUIReady && m_ConnectButton && m_CharSelectorPrefab;
        void OnEnable()
        {
            InworldController.Client.OnStatusChanged += OnStatusChanged;
            InworldController.CharacterHandler.OnCharacterRegistered += OnCharacterRegistered;
        }

        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Client.OnStatusChanged -= OnStatusChanged;
            InworldController.CharacterHandler.OnCharacterRegistered -= OnCharacterRegistered;
        }

        void OnStatusChanged(InworldConnectionStatus newStatus)
        {
            if(m_ConnectButton)
                m_ConnectButton.interactable = newStatus == InworldConnectionStatus.Idle || newStatus == InworldConnectionStatus.Connected;
        }

        void OnCharacterRegistered(InworldCharacterData charData)
        {
            if (!IsUIReady)
                return;
            InsertBubble(charData.brainName, m_CharSelectorPrefab, charData.givenName);
            StartCoroutine((m_Bubbles[charData.brainName] as CharacterButton)?.SetData(charData));
        }
    }
}

/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Entities;
using Inworld.UI;
using UnityEngine;

namespace Inworld.Sample
{
    public class CharSelectorPanel : BubblePanel
    {
        [SerializeField] CharacterButton m_CharSelectorPrefab;

        public override bool IsUIReady => base.IsUIReady && m_CharSelectorPrefab;
        void OnEnable()
        {
            InworldController.Client.OnStatusChanged += StatusChanged;
        }

        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Client.OnStatusChanged -= StatusChanged;
        }

        void StatusChanged(InworldConnectionStatus incomingStatus)
        {
            if (!IsUIReady)
                return;
            if (incomingStatus != InworldConnectionStatus.Connected)
                return;
            foreach (InworldCharacterData charData in InworldController.Client.LiveSessionData.Values)
            {
                InsertBubble(charData.brainName, m_CharSelectorPrefab, charData.givenName);
                StartCoroutine((m_Bubbles[charData.brainName] as CharacterButton)?.SetData(charData));
            }
        }
    }
}

/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
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

        public override bool IsUIReady => base.IsUIReady  && m_CharSelectorPrefab;
        void OnEnable()
        {
            InworldController.CharacterHandler.OnCharacterRegistered += OnCharacterRegistered;
        }

        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.CharacterHandler.OnCharacterRegistered -= OnCharacterRegistered;
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

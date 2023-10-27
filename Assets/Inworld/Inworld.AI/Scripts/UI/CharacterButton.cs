/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using UnityEngine;

namespace Inworld.UI
{
    public class CharacterButton : InworldUIElement
    {
        [SerializeField] InworldCharacterData m_Data;
        [SerializeField] InworldCharacter m_Char;

        /// <summary>
        /// Set the character's data.
        /// </summary>
        /// <param name="data">the data to set</param>
        public void SetData(InworldCharacterData data)
        {
            m_Data = data;
            if (data.thumbnail)
                m_Icon.texture = data.thumbnail;
        }
        /// <summary>
        /// Select this character to interact with.
        /// </summary>
        public void SelectCharacter()
        {
            if (InworldController.Status != InworldConnectionStatus.Connected)
                return;

            InworldCharacter iwChar = GetCharacter();
            if (!iwChar)
            {
                iwChar = Instantiate(m_Char, InworldController.Instance.transform);
                iwChar.transform.name = m_Data.givenName;
            }
            iwChar.Data = m_Data;
            iwChar.RegisterLiveSession();
            InworldController.CurrentCharacter = iwChar;
        }
        /// <summary>
        /// Get this character.
        /// </summary>
        InworldCharacter GetCharacter()
        {
            foreach (Transform child in InworldController.Instance.transform)
            {
                InworldCharacter iwChar = child.GetComponent<InworldCharacter>();
                if (iwChar && iwChar.Data.brainName == m_Data.brainName)
                    return iwChar;
            }
            return null;
        }
    }
}

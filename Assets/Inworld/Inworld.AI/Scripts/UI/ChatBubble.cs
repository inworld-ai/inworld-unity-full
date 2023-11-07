/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using TMPro;
using UnityEngine;

namespace Inworld.UI
{
    /// <summary>
    ///     This class is for each detailed chat bubble.
    /// </summary>
    public class ChatBubble : InworldUIElement
    {
        [SerializeField] TMP_Text m_TextField;

    #region Properties
        /// <summary>
        ///     Get/Set the bubble's main content.
        /// </summary>
        public string Text
        {
            get => m_TextField.text;
            set => m_TextField.text = value;
        }

        /// <summary>
        ///     Set the bubble's property.
        /// </summary>
        /// <param name="charName">The bubble's owner's name</param>
        /// <param name="thumbnail">The bubble's owner's thumbnail</param>
        /// <param name="text">The bubble's content</param>
        public void SetBubble(string charName, Texture2D thumbnail, string text = null)
        {
            m_Title.text = charName;
            if (m_Icon)
                m_Icon.texture = thumbnail;
            if (!string.IsNullOrEmpty(text))
                m_TextField.text = text;
        }
    #endregion
    }

}


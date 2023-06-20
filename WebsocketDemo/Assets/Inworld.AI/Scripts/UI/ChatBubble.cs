/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld;
using TMPro;
using UnityEngine;

/// <summary>
///     This class is for each detailed chat bubble.
/// </summary>
public class ChatBubble : InworldUIElement
{
    #region Inspector Variables
    [SerializeField] TMP_Text m_TextField;
    [SerializeField] TMP_Text m_CharacterName;

    string m_CharName;
    string m_Emotion;
    #endregion

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
    ///     Get/Set the bubble's speaker's name.
    /// </summary>
    public string CharacterName
    {
        get => m_CharacterName.text;
        set => m_CharacterName.text = value;
    }
    /// <summary>
    ///     Set the bubble's property.
    /// </summary>
    /// <param name="charName">The bubble's owner's name</param>
    /// <param name="thumbnail">The bubble's owner's thumbnail</param>
    /// <param name="text">The bubble's content</param>
    public void SetBubble(string charName, Texture2D thumbnail, string text = null)
    {
        m_CharacterName.text = charName;
        if (m_Icon)
            m_Icon.texture = thumbnail;
        if (!string.IsNullOrEmpty(text))
            m_TextField.text = text;
    }
    #endregion
}


/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inworld.UI
{
    public class InworldUIElement : MonoBehaviour
    {    
        [SerializeField] protected RawImage m_Icon;
        [SerializeField] protected TMP_Text m_Title;
        /// <summary>
        ///     Get the bubble's height.
        /// </summary>
        public float Height => GetComponent<RectTransform>().sizeDelta.y + 20;
        /// <summary>
        ///     Gets/Sets the Thumbnail of the bubble.
        ///     NOTE: For the worldspace's floating text, Icon will not be displayed.
        /// </summary>
        public RawImage Icon
        {
            get => m_Icon;
            set => m_Icon = value;
        }
        /// <summary>
        ///     Gets/Sets the name of the thumbnail
        /// </summary>
        public TMP_Text Title
        {
            get => m_Title;
            set => m_Title = value;
        }
        /// <summary>
        ///     Set the bubble's property.
        /// </summary>
        /// <param name="charName">The bubble's owner's name</param>
        /// <param name="thumbnail">The bubble's owner's thumbnail</param>
        /// <param name="text">The bubble's content</param>
        public virtual void SetBubble(string charName, Texture2D thumbnail = null, string text = null)
        {
            if (m_Title)
                m_Title.text = charName;
            if (m_Icon && thumbnail)
                m_Icon.texture = thumbnail;
        }
        public virtual void AttachBubble(string text)
        {
            
        }
    }
}

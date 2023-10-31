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
    }
}

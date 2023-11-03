/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using UnityEngine;


namespace Inworld.UI
{
    public class ChatPanel : MonoBehaviour
    {
        [SerializeField] protected RectTransform m_BubbleContentAnchor;
        [SerializeField] protected ChatBubble m_BubbleLeftPrefab;
        [SerializeField] protected ChatBubble m_BubbleRightPrefab;
    }
}

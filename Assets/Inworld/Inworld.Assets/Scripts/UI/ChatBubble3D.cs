/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.UI;
using UnityEngine.EventSystems;


namespace Inworld.Sample
{
    public class ChatBubble3D : ChatBubble
    {
        public override void OnPointerUp(PointerEventData eventData)
        {
            // YAN: Do NOT trigger feedback canvas.
        }
    }
}

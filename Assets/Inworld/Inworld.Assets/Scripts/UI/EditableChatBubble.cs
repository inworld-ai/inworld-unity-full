/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Assets;
using Inworld.Sample;
using UnityEngine.EventSystems;

namespace Inworld.UI
{
    public class EditableChatBubble : ChatBubble, IPointerUpHandler, IPointerDownHandler
    {
        ContentEditingCanvas m_ContentEditingDlg;
        // Start is called before the first frame update
        void Start()
        {
            if (!PlayerController.Instance)
                return;
            m_ContentEditingDlg = PlayerController.Instance.GetComponentInChildren<ContentEditingCanvas>();
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (m_Title.text == InworldAI.User.Name) // Send by player.
                return;
            if (!m_ContentEditingDlg)
                return;
            m_ContentEditingDlg.Open();
            m_ContentEditingDlg.Init(m_InteractionID, m_CorrelationID);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // To make PointerUp working, PointerDown is required.
        }
    }
}


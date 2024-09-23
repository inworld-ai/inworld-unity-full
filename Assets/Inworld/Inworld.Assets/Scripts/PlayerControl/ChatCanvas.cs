/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.UI;
using UnityEngine;

namespace Inworld.Sample
{
    public class ChatCanvas : PlayerCanvas
    {
        [SerializeField] protected BubblePanel m_BubblePanel;
        protected CharSelectingMethod m_PrevSelectingMethod;
        
        protected override void OnCanvasOpen()
        {
            m_PrevSelectingMethod = InworldController.CharacterHandler.SelectingMethod;
            InworldController.CharacterHandler.SelectingMethod = CharSelectingMethod.Manual;
            if (m_BubblePanel)
                m_BubblePanel.UpdateContent();
        }
        protected override void OnCanvasClosed()
        {
            InworldController.CharacterHandler.SelectingMethod = m_PrevSelectingMethod;
        }
    }
}

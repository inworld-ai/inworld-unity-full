/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Util;
using UnityEngine;
#if UNITY_EDITOR
namespace Inworld.Editor.States
{
    /// <summary>
    ///     Error page triggers when default resource is not correct, or failed to received token.
    /// </summary>
    public class EditorError : EditorState
    {
        #region State Functions
        public override void OnEnter()
        {
            InworldEditor.Title = $"Disconnected. {InworldEditor.SecToReconnect} Seconds to Auto-Reconnect";
            InworldEditor.SecToReconnect = Random.Range(InworldEditor.SecToReconnect, InworldEditor.SecToReconnect * 2);
            InworldEditor.CurrentCountDown = InworldEditor.SecToReconnect;
            m_BotPanel = InstallPanel(InworldAI.UI.ErrorBotPanel);
            SetupButton("BtnCancel", () => InworldEditor.Status = InworldEditorStatus.Init);
        }

        public override void PostUpdate()
        {
            InworldEditor.CurrentCountDown -= Time.deltaTime;
            if (InworldEditor.CurrentCountDown > 0)
                InworldEditor.Title = $"Disconnected. {InworldEditor.CurrentCountDown} Seconds to Auto-Reconnect";
            else
                InworldEditor.Status = InworldEditor.CurrentProgress > 95 ? InworldEditorStatus.CharacterChooser : InworldEditorStatus.WorkspaceChooser;
        }
        #endregion
    }
}
#endif

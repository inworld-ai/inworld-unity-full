/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/

using Inworld.Util;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
#if UNITY_EDITOR
namespace Inworld.Editor.States
{
    /// <summary>
    ///     This state triggered once user token has been pasted and login has been clicked.
    ///     In this state, it'll fetch user token from Inworld server, and then list all the workspaces of that inworld user.
    /// </summary>
    public class EditorWorkspaceChooser : EditorState
    {
        #region Call backs
        void OnWorkspaceChanged(string newValue)
        {
            // 1. Get Correspondent wsData.
            InworldEditor.ErrorMessage = "";
            InworldWorkspaceData wsData = InworldAI.User.Workspaces.FirstOrDefault(data => data.Value.title == newValue).Value;
            if (!wsData)
                return;
            InworldAI.Game.CurrentWorkspace = wsData;
            InworldEditor.Status = InworldEditorStatus.SceneChooser;
        }
         #endregion

        #region Private Variables
        DropdownField m_WorkspaceChooser;
        bool m_DataInitialized;
        #endregion

        #region State Functions
        public override void OnEnter()
        {
            InworldEditor.ErrorMessage = "";
            InworldEditor.Title = "Connecting Inworld...";
            m_DataInitialized = false;
            _SetupContentPanel(InworldAI.UI.WorkspaceChooser);
            _SetupBotPanel(InworldAI.UI.WSChooserBot);
        }
        public override void OnExit()
        {
            m_WorkspaceChooser = null;
            base.OnExit();
        }
        public override void OnError()
        {
            InworldEditor.Status = InworldEditorStatus.Error;
        }
        public override void PostUpdate()
        {
            _HandleLoadingBar();
            _HandleTokenRefresh();
        }
        public override void OnConnected()
        {
            InworldEditor.Title = $"Welcome {InworldAI.User.Name}!";
        }
        #endregion

        #region UI Functions
        protected override void _SetupContentPanel(VisualTreeAsset contentPanel = null)
        {
            base._SetupContentPanel(contentPanel);
            InworldWorkspaceData wsData = InworldAI.Game.currentWorkspace;
            // Actual perform:
            InworldEditor.Instance.GetUserToken(InworldAI.User.IDToken);
            if (wsData)
            {
                m_WorkspaceChooser = SetupDropDown
                (
                    "WorkspaceChooser",
                    InworldAI.User.Workspaces.Select(kvp => kvp.Value.title).ToList(),
                    OnWorkspaceChanged, wsData.title == "Default Workspace" ? "---SELECT WORKSPACE---" : wsData.title
                );
            }
            else
            {
                m_WorkspaceChooser = SetupDropDown
                (
                    "WorkspaceChooser",
                    null,
                    OnWorkspaceChanged, null, false
                );
            }
        }
        protected override void _SetupBotPanel(VisualTreeAsset botPanel = null)
        {
            base._SetupBotPanel(botPanel);
            _SetupBotContents();
        }
        protected override void _SetupBotContents()
        {
            SetupButton("BtnRefresh", InworldEditor.Instance.Reconnect);
            SetupButton("BtnLogout", () => InworldEditor.Status = InworldEditorStatus.Init);
        }
        #endregion

        #region Private Functions
        void _HandleLoadingBar()
        {
            if (m_DataInitialized)
                return;
            EditorUtility.DisplayProgressBar("InworldAI", $"Downloading Workspaces {InworldEditor.CurrentProgress}% Completed", InworldEditor.CurrentProgress * 0.01f);
            if (InworldEditor.CurrentProgress < 95)
                return;
            m_DataInitialized = true;
            EditorUtility.ClearProgressBar();
            m_WorkspaceChooser.choices = InworldAI.User.Workspaces.Select(kvp => kvp.Value.title).ToList();
            m_WorkspaceChooser.visible = true;
        }
        void _HandleTokenRefresh()
        {
            if (InworldAI.User.IsExpired)
                InworldEditor.Instance.Reconnect();
        }
        #endregion
    }
}
#endif

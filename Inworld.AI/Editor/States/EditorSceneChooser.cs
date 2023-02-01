/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Studio;
using Inworld.Util;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
#if UNITY_EDITOR
namespace Inworld.Editor.States
{
    /// <summary>
    ///     This State triggers when Workspace has all been selected.
    ///     Start Fetching API Keys/Scenes of that workspace,
    ///     and provided for choosing once fetched from internet.
    /// </summary>
    public class EditorSceneChooser : EditorState
    {
        #region Private Variables
        Button m_HyperLink;
        Button m_Tutorial;
        DropdownField m_KeyChooser;
        DropdownField m_SceneChooser;
        bool m_DataInitialized;
        #endregion

        #region State Functions
        public override void OnEnter()
        {
            InworldEditor.ErrorMessage = "";
            InworldEditor.Title = $"Welcome {InworldAI.User.Name}!\n";
            m_DataInitialized = true;
            _SetupContentPanel(InworldAI.UI.SceneChooser);
            _SetupBotPanel(InworldAI.UI.WSChooserBot);
        }
        public override void OnExit()
        {
            m_KeyChooser = null;
            m_SceneChooser = null;
            m_HyperLink = null;
            m_Tutorial = null;
            base.OnExit();
        }
        public override void PostUpdate()
        {
            InworldWorkspaceData wsData = InworldAI.Game.currentWorkspace;
            if (!wsData)
                return;
            if (!InworldEditor.Progress.ContainsKey(wsData))
                return;
            if (m_DataInitialized)
                return;
            float fetchingProgress = InworldEditor.Progress[wsData].Progress;
            if (fetchingProgress > 99f)
            {
                m_DataInitialized = true;
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
                if (wsData.IsValid)
                {
                    if (m_KeyChooser != null)
                    {
                        m_KeyChooser.choices = wsData.integrations.Select(key => key.ShortName).ToList();
                        m_KeyChooser.visible = true;
                    }
                    if (m_SceneChooser != null)
                    {
                        m_SceneChooser.choices = wsData.scenes.Select(key => key.ShortName).ToList();
                        m_SceneChooser.visible = true;
                    }
                }
                else
                {
                    m_HyperLink.visible = true;
                    m_Tutorial.visible = true;
                }
            }
            else
            {
                EditorUtility.DisplayProgressBar("InworldAI", $"Downloading SceneData {fetchingProgress}% Completed", fetchingProgress * 0.01f);
            }
        }
        public override void OnConnected()
        {
            _LoadingScenes();
        }
        #endregion

        #region UI Functions
        protected override void _SetupContentPanel(VisualTreeAsset contentPanel = null)
        {
            base._SetupContentPanel(contentPanel);
            _SetupContents();
            // Actual TO DO:
            _LoadingScenes();
        }
        protected override void _SetupContents()
        {
            SetupDropDown
            (
                "WorkspaceChooser",
                InworldAI.User.Workspaces.Select(kvp => kvp.Value.title).ToList(),
                OnWorkspaceChanged, InworldAI.Game.currentWorkspace.title
            );

            string targetKey = InworldAI.Game.currentKey ? InworldAI.Game.currentKey.ShortName : null;
            string targetScene = InworldAI.Game.currentScene ? InworldAI.Game.currentScene.ShortName : null;

            if (string.IsNullOrEmpty(targetKey))
                m_KeyChooser = SetupDropDown("KeyChooser", null, OnKeyChanged, null, false);
            else
                m_KeyChooser = SetupDropDown
                (
                    "KeyChooser", InworldAI.Game.currentWorkspace.integrations.Select(key => key.ShortName).ToList(),
                    OnKeyChanged, targetKey
                );
            if (string.IsNullOrEmpty(targetScene))
                m_SceneChooser = SetupDropDown("SceneChooser", null, OnSceneChanged, null, false);
            else
                m_SceneChooser = SetupDropDown
                (
                    "SceneChooser", InworldAI.Game.currentWorkspace.scenes.Select(scene => scene.ShortName).ToList(),
                    OnSceneChanged, targetKey
                );

            m_HyperLink = SetupButton("HyperLink", () => Help.BrowseURL($"{InworldAI.Game.currentServer.web}/{InworldAI.Game.currentWorkspace.fullName}"), false);
            m_Tutorial = SetupButton("Tutorial", () => Help.BrowseURL($"{InworldAI.Game.currentServer.tutorialPage}/{InworldAI.Game.currentWorkspace.fullName}"), false);
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

        #region Callbacks
        void OnWorkspaceChanged(string newValue)
        {
            // 1. Get Correspondent wsData.
            InworldEditor.ErrorMessage = "";
            InworldWorkspaceData wsData = InworldAI.User.Workspaces.FirstOrDefault(data => data.Value.title == newValue).Value;
            if (!wsData)
                return;
            if (wsData.title == InworldAI.Game.currentWorkspace.title)
                return;
            // 2. Show Dialog
            if (EditorUtility.DisplayDialog
            (
                "Change Workspace", k_SwitchWSMSG, "Proceed", "Cancel",
                DialogOptOutDecisionType.ForThisMachine, k_SwitchWSKey
            ))
            {
                InworldAI.Game.CurrentWorkspace = wsData;
                InworldEditor.Status = InworldEditorStatus.WorkspaceChooser;
            }
        }
        void OnKeyChanged(string newValue)
        {
            if (InworldAI.Game.currentKey && InworldAI.Game.currentKey.ShortName == newValue)
                return;
            InworldAI.Game.currentKey = InworldAI.Game.currentWorkspace.integrations.FirstOrDefault(key => key.ShortName == newValue);
            _CheckProceed();
        }
        void OnSceneChanged(string newValue)
        {
            if (InworldAI.Game.currentScene && InworldAI.Game.currentScene.ShortName == newValue)
                return;
            // 2. Show Dialog
            InworldAI.Game.currentScene = InworldAI.Game.currentWorkspace.scenes.FirstOrDefault(scene => scene.ShortName == newValue);
            _CheckProceed();
        }
        #endregion

        #region Private Functions
        void _CheckProceed()
        {
            if (InworldAI.Game.currentWorkspace && InworldAI.Game.currentScene && InworldAI.Game.currentKey)
                InworldEditor.Status = InworldEditorStatus.CharacterChooser;
        }
        void _LoadingScenes()
        {
            if (!InworldAI.Game.currentWorkspace)
                return;
            m_DataInitialized = false; // YAN: Start Displaying LoadBar.
            InworldWorkspaceData wsData = InworldAI.Game.currentWorkspace;
            if (!InworldEditor.Progress.ContainsKey(wsData))
                InworldEditor.Progress[wsData] = new WorkspaceFetchingProgress();
            InworldEditor.Instance.ListAPIKey(wsData);
            InworldEditor.Instance.ListScenes(wsData);
            InworldEditor.Instance.ListCharacters(wsData);
        }
        #endregion
    }
}
#endif

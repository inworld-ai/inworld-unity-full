/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Util;
using UnityEditor;
using UnityEngine.UIElements;
#if UNITY_EDITOR
namespace Inworld.Editor.States
{
    /// <summary>
    ///     This is the page for developers to copy/paste their studio tokens.
    /// </summary>
    public class EditorInit : EditorState
    {
        const string k_DefaultTitle = "Please paste Auth token here:";
        const string k_TokenIncorrect = "Token Incorrect. Please paste again.";

        #region State Functions
        public override void OnEnter()
        {
            InworldEditor.ErrorMessage = "";
            InworldAI.User.LogOut();
            InworldEditor.Title = k_DefaultTitle;
            _SetupContentPanel(InworldAI.UI.InputField);
            _SetupBotPanel(InworldAI.UI.InitBotPanel);
        }
        #endregion

        #region UI Functions
        protected override void _SetupContentPanel(VisualTreeAsset contentPanel = null)
        {
            base._SetupContentPanel(contentPanel);
            _SetupContents();
        }
        protected override void _SetupContents()
        {
            TextField txtTokenForExchange = InworldEditor.Root.Q<TextField>("TokenForExchange");
            SetupButton("HyperLink", () => Help.BrowseURL(InworldAI.Game.currentServer.web));
            SetupButton("HyperLinkTutorial", () => Help.BrowseURL(InworldAI.Game.currentServer.tutorialPage));
            txtTokenForExchange?.RegisterValueChangedCallback
            (
                evt =>
                {
                    InworldEditor.TokenForExchange = evt.newValue;
                    Button btnInitLogin = InworldEditor.Root.Q<Button>("BtnInitLogin");
                    btnInitLogin?.SetEnabled(!string.IsNullOrEmpty(txtTokenForExchange.text));
                }
            );
        }
        protected override void _SetupBotPanel(VisualTreeAsset botPanel = null)
        {
            base._SetupBotPanel(botPanel);
            _SetupBotContents();
        }
        protected override void _SetupBotContents()
        {
            Button btnInitLogin = InworldEditor.Root.Q<Button>("BtnInitLogin");
            if (btnInitLogin != null)
            {
                btnInitLogin.clickable.clicked += () =>
                {
                    if (InworldEditor.IsTokenValid)
                        InworldEditor.Status = InworldEditorStatus.WorkspaceChooser;
                    else
                        InworldEditor.ErrorMessage = k_TokenIncorrect;
                };
                btnInitLogin.SetEnabled(false);
            }
            SetupButton("BtnInitBack", () => InworldEditor.Status = InworldEditorStatus.Default);
        }
        #endregion
    }
}
#endif

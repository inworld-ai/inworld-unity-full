/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/

using Inworld.Util;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
namespace Inworld.Editor.States
{
    /// <summary>
    ///     State Machine Base class.
    ///     Also provide Editor State API to Add UI Elements
    /// </summary>
    public class EditorState
    {
        #region Protected Variables
        protected TemplateContainer m_ContentPanel;
        protected TemplateContainer m_BotPanel;
        protected const string k_SwitchSceneKey = "Inworld_SwitchScene";
        protected const string k_SwitchWSKey = "Inworld_SwitchWorkspace";
        protected const string k_SwitchSceneMSG = "If you change Inworld Scene, all old Inworld Scene characters in the Unity scene would not be active.\nDo you want to proceed?";
        protected const string k_SwitchWSMSG = "If you change workspaces, all old workspace characters in the Unity scene would not be active.\nDo you want to proceed?";
        protected const string k_LoadingPlayerControllerMSG = "Adding player controller would delete current main camera.\nContinue?";
        protected delegate void OnDataChanged(string newValue);
        #endregion

        #region State Functions
        /// <summary>
        ///     Register when a new state enters.
        /// </summary>
        public virtual void OnEnter() {}
        /// <summary>
        ///     Register when an old state exits.
        /// </summary>
        public virtual void OnExit()
        {
            ClearContainer(ref m_ContentPanel);
            ClearContainer(ref m_BotPanel);
        }
        /// <summary>
        ///     Usually triggers when setting InworldEditor.ErrorMsg
        ///     Register when error need to be additionally processed.
        /// </summary>
        public virtual void OnError() {}
        /// <summary>
        ///     PostUpdate called in OnInspectorGUI()
        ///     The refresh rate was a fixed 0.1s.
        /// </summary>
        public virtual void PostUpdate() {}
        /// <summary>
        ///     After "Reconnect" button clicked.
        ///     OnConnected Called when Inworld token has been received,
        /// </summary>
        public virtual void OnConnected() {}
        #endregion

        #region UI Functions
        /// Each page is divided by 3 parts:
        /// 1. Header icon ("Inworld")
        /// 2. Content Panel.
        /// 3. Bot Panel (Mainly for buttons)
        /// <summary>
        ///     Load Content uxml.
        /// </summary>
        /// <param name="contentPanel">A VisualTreeAsset (".uxml") to load</param>
        protected virtual void _SetupContentPanel(VisualTreeAsset contentPanel = null)
        {
            if (contentPanel != null)
                m_ContentPanel = InstallPanel(contentPanel);
        }
        /// <summary>
        ///     Actual binding logic for the UI elements in contents.
        ///     Adding Dropdown, buttons, progress bar, etc.
        /// </summary>
        protected virtual void _SetupContents() {}
        /// <summary>
        ///     Load Bot Panel uxml.
        /// </summary>
        /// <param name="botPanel">A VisualTreeAsset (".uxml") to load</param>
        protected virtual void _SetupBotPanel(VisualTreeAsset botPanel = null)
        {
            if (botPanel != null)
                m_BotPanel = InstallPanel(botPanel);
        }
        /// <summary>
        ///     Actual binding logic for the UI elements in bot panel.
        ///     Adding Dropdown, buttons, progress bar, etc.
        /// </summary>
        protected virtual void _SetupBotContents() {}
        #endregion

        #region API for Editor States
        /// <summary>
        ///     Whenever a state exits, content panel and bot panel should be removed.
        ///     And load new content/bot panel.
        ///     Calling this function to do so.
        /// </summary>
        /// <param name="containerToClear">An instatiated panel to remove.</param>
        static void ClearContainer(ref TemplateContainer containerToClear)
        {
            if (InworldEditor.Root.Contains(containerToClear))
                InworldEditor.Root.Remove(containerToClear);
            containerToClear = null;
        }
        /// <summary>
        ///     It would be automatically called in _SetupContentPanel() and _SetupBotPanel()
        ///     Use this function if you have additional UXML to load.
        /// </summary>
        /// <param name="panel">A VisualTreeAsset (".uxml") to load.</param>
        /// <returns> The instantiated panel. </returns>
        protected static TemplateContainer InstallPanel(VisualTreeAsset panel)
        {
            TemplateContainer container = panel.Instantiate();
            InworldEditor.Root.Add(container);
            return container;
        }
        /// <summary>
        ///     Bind a Dropdown field to the current panel.
        /// </summary>
        /// <param name="tag">
        ///     The component name in your uxml.
        ///     NOTE: in uxml, it starts with "#", but as a string parameter, don't put "#".
        /// </param>
        /// <param name="isVisible">
        ///     Set false if you'd like to hide at first.
        ///     You could always set "visible" to "true" to activate.
        /// </param>
        /// <returns>The instantiated DropdownField.</returns>
        protected static DropdownField SetupDropDown(string tag, bool isVisible = true)
        {
            DropdownField dropdown = InworldEditor.Root.Q<DropdownField>(tag);
            if (dropdown != null)
                dropdown.visible = isVisible;
            return dropdown;
        }
        /// <summary>
        ///     Bind a Dropdown field to the current panel.
        /// </summary>
        /// <param name="tag">
        ///     The component name in your uxml.
        ///     NOTE: in uxml, it starts with "#", but as a string parameter, don't put "#".
        /// </param>
        /// <param name="input">The string list of all the options.</param>
        /// <param name="onDataChanged">The callback function.</param>
        /// <param name="targetValue">the default value.</param>
        /// <param name="isVisible">
        ///     Set false if you'd like to hide at first.
        ///     You could always set "visible" to "true" to activate.
        /// </param>
        /// <returns>The instantiated DropdownField objects.</returns>
        protected static DropdownField SetupDropDown(string tag, List<string> input, OnDataChanged onDataChanged, string targetValue = null, bool isVisible = true)
        {
            DropdownField dropdown = InworldEditor.Root.Q<DropdownField>(tag);
            if (dropdown == null)
                return null;
            dropdown.visible = isVisible;
            dropdown.value = targetValue;
            if (input != null)
                dropdown.choices = input;
            dropdown.RegisterValueChangedCallback(evt => onDataChanged(evt.newValue));
            return dropdown;
        }
        /// <summary>
        ///     Bind a progress bar to the current panel.
        /// </summary>
        /// <param name="tag">
        ///     The component name in your uxml.
        ///     NOTE: in uxml, it starts with "#", but as a string parameter, don't put "#".
        /// </param>
        /// <param name="isVisible">
        ///     Set false if you'd like to hide at first.
        ///     You could always set "visible" to "true" to activate.
        /// </param>
        /// <returns>The instantiated progress bar objects.</returns>
        protected static ProgressBar SetupProgressBar(string tag, bool isVisible = true)
        {
            ProgressBar progressBar = InworldEditor.Root.Q<ProgressBar>(tag);
            if (progressBar != null)
                progressBar.visible = isVisible;
            return progressBar;
        }
        /// <summary>
        ///     Add an Inworld character chooser button to the current panel.
        ///     Its background is the character's thumbnail,
        ///     with its CharacterName displayed at bottom center.
        /// </summary>
        /// <param name="charData">The InworldCharacterData</param>
        /// <returns>The Instantiated Inworld character chooser button</returns>
        protected static Button CreateCharacterButton(InworldCharacterData charData)
        {
            return new Button
            {
                style =
                {
                    backgroundImage = charData.Thumbnail,
                    width = 80,
                    height = 80,
                    marginLeft = 5,
                    marginRight = 5,
                    marginBottom = 5,
                    marginTop = 5,
                    unityTextAlign = TextAnchor.LowerCenter,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    whiteSpace = WhiteSpace.Normal
                },
                text = charData.characterName
            };
        }
        /// <summary>
        ///     Bind a Toggle to the current panel.
        /// </summary>
        /// <param name="tag">
        ///     The component name in your uxml.
        ///     NOTE: in uxml, it starts with "#", but as a string parameter, don't put "#".
        /// </param>
        /// <param name="isVisible">
        ///     Set false if you'd like to hide at first.
        ///     You could always set "visible" to "true" to activate.
        /// </param>
        /// <returns>The instantiated toggle object.</returns>
        protected static Toggle SetupToggle(string tag, bool isVisible = true)
        {
            Toggle toggle = InworldEditor.Root.Q<Toggle>(tag);
            if (toggle != null)
                toggle.visible = isVisible;
            return toggle;
        }
        /// <summary>
        ///     Bind a Label (A line of text) to the current panel.
        /// </summary>
        /// <param name="tag">
        ///     The component name in your uxml.
        ///     NOTE: in uxml, it starts with "#", but as a string parameter, don't put "#".
        /// </param>
        /// </param>
        /// <param name="isVisible">
        ///     Set false if you'd like to hide at first.
        ///     You could always set "visible" to "true" to activate.
        /// </param>
        /// <returns>The instantiated Label object.</returns>
        protected static Label SetupLabel(string tag, bool isVisible = true)
        {
            Label label = InworldEditor.Root.Q<Label>(tag);
            if (label != null)
                label.visible = isVisible;
            return label;
        }
        /// <summary>
        ///     Bind a button to the current panel.
        /// </summary>
        /// <param name="tag">
        ///     The component name in your uxml.
        ///     NOTE: in uxml, it starts with "#", but as a string parameter, don't put "#".
        /// </param>
        /// <param name="action">The callback funtion if this button clicked.</param>
        /// <param name="isVisible">
        ///     Set false if you'd like to hide at first.
        ///     You could always set "visible" to "true" to activate.
        /// </param>
        /// <returns>The instantiated Button object.</returns>
        protected static Button SetupButton(string tag, Action action, bool isVisible = true)
        {
            Button button = InworldEditor.Root.Q<Button>(tag);
            if (button != null)
            {
                button.clickable.clicked += action;
                button.visible = isVisible;
            }
            return button;
        }
        /// <summary>
        ///     If you've already binded a dropdown object, but didn't set the choices at the first.
        ///     Call this function to activate.
        /// </summary>
        /// <param name="target">The dropdown object.</param>
        /// <param name="input">The string list of choices.</param>
        /// <param name="onDataChanged">The call back function if choice changed.</param>
        /// <param name="targetValue">The default value.</param>
        protected static void ActivateDropDown(ref DropdownField target, List<string> input, OnDataChanged onDataChanged, string targetValue = null)
        {
            if (target == null)
                return;
            target.value = targetValue;
            target.choices = input;
            target.RegisterValueChangedCallback(evt => onDataChanged(evt.newValue));
            target.visible = true;
        }
        /// <summary>
        ///     Set the callback for the instantiated toggle.
        /// </summary>
        /// <param name="toggle">The target toggle to set callback</param>
        /// <param name="callback">the callback function if value changed.</param>
        protected static void ActivateToggle(ref Toggle toggle, EventCallback<ChangeEvent<bool>> callback)
        {
            if (toggle == null)
                return;
            toggle.RegisterValueChangedCallback(callback);
            toggle.visible = true;
        }
        #endregion
    }
}
#endif

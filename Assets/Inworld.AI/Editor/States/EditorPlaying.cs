/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Util;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
namespace Inworld.Editor.States
{
    /// <summary>
    ///     This State would be triggered only by 2 conditions:
    ///     1. It's the default state for runtime.
    ///     2. If Editor code had been changed,
    ///     the m_State of InworldEditor would be null, and need to be updated.
    /// </summary>
    public class EditorPlaying : EditorState
    {
        #region Private Variables.
        Label m_AppPlaying;
        VisualElement m_AppEditing;
        Button m_BtnRefresh;
        #endregion

        #region State Functions
        public override void OnEnter()
        {
            _SetupContentPanel(InworldAI.UI.ApplicationPlaying);
        }
        public override void PostUpdate()
        {
            m_AppPlaying.visible = Application.isPlaying;
            m_AppEditing.visible = !Application.isPlaying;
            m_BtnRefresh.visible = !Application.isPlaying;
        }
        public override void OnExit()
        {
            m_AppEditing = null;
            m_AppPlaying = null;
            m_BtnRefresh = null;
            base.OnExit();
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
            m_AppPlaying = SetupLabel("AppPlaying");
            m_AppEditing = InworldEditor.Root.Q<VisualElement>("AppEditing");
            m_BtnRefresh = SetupButton("BtnRefresh", () => InworldEditor.Instance.Init(), false);
        }
        #endregion
    }
}
#endif

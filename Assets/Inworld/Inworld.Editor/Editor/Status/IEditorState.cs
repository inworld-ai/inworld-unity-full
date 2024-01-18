/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
namespace Inworld.Editors
{
    /// <summary>
    /// The state interface used for multiple editor pages.
    /// </summary>
    public interface IEditorState
    {
        /// <summary>
        /// Triggers when open editor window.
        /// </summary>
        public void OnOpenWindow();
        /// <summary>
        /// Triggers when drawing the title of the editor panel page.
        /// </summary>
        public void DrawTitle();
        /// <summary>
        /// Triggers when drawing the content of the editor panel page.
        /// </summary>
        public void DrawContent();
        /// <summary>
        /// Triggers when drawing the buttons at the bottom of the editor panel page.
        /// </summary>
        public void DrawButtons();
        /// <summary>
        /// Triggers when this state exits.
        /// </summary>
        public void OnExit();
        /// <summary>
        /// Triggers when this state enters.
        /// </summary>
        public void OnEnter();
        /// <summary>
        /// Triggers when other general update logic has been finished.
        /// </summary>
        public void PostUpdate();

    }
}
#endif
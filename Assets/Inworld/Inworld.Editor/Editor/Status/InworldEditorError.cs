/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Inworld.Editors
{
    public class InworldEditorError : IEditorState
    {
        /// <summary>
        /// Triggers when open editor window.
        /// </summary>
        public void OnOpenWindow()
        {
            
        }
        /// <summary>
        /// Triggers when drawing the title of the editor panel page.
        /// </summary>
        public void DrawTitle()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(InworldEditor.Instance.Error, InworldEditor.Instance.ErrorStyle);
            EditorGUILayout.Space();
        }
        /// <summary>
        /// Triggers when drawing the content of the editor panel page.
        /// </summary>
        public void DrawContent()
        {

        }
        /// <summary>
        /// Triggers when drawing the buttons at the bottom of the editor panel page.
        /// </summary>
        public void DrawButtons()
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Back", InworldEditor.Instance.BtnStyle))
            {
                InworldEditor.Instance.Status = EditorStatus.Init;
            }
        }
        /// <summary>
        /// Triggers when this state exits.
        /// </summary>
        public void OnExit()
        {
            
        }
        /// <summary>
        /// Triggers when this state enters.
        /// </summary>
        public void OnEnter()
        {
            
        }
        /// <summary>
        /// Triggers when other general update logic has been finished.
        /// </summary>
        public void PostUpdate()
        {
            
        }
    }
}
#endif
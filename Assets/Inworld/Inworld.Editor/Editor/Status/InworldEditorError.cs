/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace Inworld.AI.Editor
{
    public class InworldEditorError : IEditorState
    {
        public void OnOpenWindow()
        {
            
        }
        public void DrawTitle()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(InworldEditor.Instance.Error, InworldEditor.Instance.ErrorStyle);
            EditorGUILayout.Space();
        }
        public void DrawContent()
        {

        }
        public void DrawButtons()
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Back", InworldEditor.Instance.BtnStyle))
            {
                InworldEditor.Instance.Status = EditorStatus.Init;
            }
        }
        public void OnExit()
        {
            
        }
        public void OnEnter()
        {
            
        }
        public void PostUpdate()
        {
            
        }
    }
}
#endif
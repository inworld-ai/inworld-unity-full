/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using UnityEditor;
using UnityEngine;
                                                                                                                                                                                                                                                                                                                                                                                                                                                                       #if UNITY_EDITOR

namespace Inworld.Editors
{
    public class InworldEditorSelectGameMode : IEditorState
    {

        public void OnOpenWindow()
        {

        }
        public void DrawTitle()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Please setup the LLM config.", InworldEditor.Instance.TitleStyle);
            EditorGUILayout.Space();

        }
        public void DrawContent()
        {

        }
        public void DrawButtons()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Back", InworldEditor.Instance.BtnStyle))
            {
                InworldEditor.Instance.Status = EditorStatus.SelectGameData;
            }
            GUILayout.EndHorizontal();
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
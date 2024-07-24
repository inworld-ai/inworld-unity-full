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
    public class InworldEditorSelectMode : IEditorState
    {
        
        public void OnOpenWindow()
        {
            
        }
        public void DrawTitle()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Welcome, {InworldAI.User.Name}! Please select mode:", InworldEditor.Instance.TitleStyle);
            EditorGUILayout.Space();
        }
        public void DrawContent()
        {
            if (GUILayout.Button("Chat in Editor", InworldEditor.Instance.ContentBtnStyle))
            {
                InworldEditor.Instance.Status = EditorStatus.LLM;
            }
            if (GUILayout.Button("Setup in Runtime", InworldEditor.Instance.ContentBtnStyle))
            {
                InworldEditor.Instance.Status = EditorStatus.SelectGameData;
            }
        }
        public void DrawButtons()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Back", InworldEditor.Instance.BtnStyle))
            {
                InworldEditor.Instance.Status = EditorStatus.Init;
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
/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
                                                                                                                                                                                                                                                                                                                                                                                                                                                                       #if UNITY_EDITOR
namespace Inworld.Editors
{
    public class InworldEditorLLM : IEditorState
    {
        string m_TextInput;
        List<string> m_ChatHistory = new List<string>();
        bool m_IsFromPlayer;
        Vector2 m_ScrollPos;
        GUIStyle PlayerLabel => new GUIStyle
        {
            wordWrap = true,
            padding = new RectOffset(10, 10, 0, 0),
            normal = new GUIStyleState
            {
                textColor = Color.white
            },
            alignment = TextAnchor.MiddleLeft
        };
        GUIStyle BotLabel => new GUIStyle
        {
            wordWrap = true,
            padding = new RectOffset(10, 10, 0, 0),
            normal = new GUIStyleState()
            {
                textColor = Color.white
            },
            alignment = TextAnchor.MiddleRight
        };
        GUIStyle SendBtnStyle => new GUIStyle(GUI.skin.button)
        {
            fontSize = 11,
            fixedWidth = 60,
            margin = new RectOffset(10, 10, 0, 0),
        };
        public void OnOpenWindow()
        {
            
        }
        public void DrawTitle()
        {
            
        }
        public void DrawContent()
        {
            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos, GUILayout.ExpandHeight(true));
            foreach (string item in m_ChatHistory)
            {
                GUILayout.Box(item, m_IsFromPlayer ? PlayerLabel : BotLabel);
                m_IsFromPlayer = !m_IsFromPlayer;
            }
            EditorGUILayout.EndScrollView();
        }
        public void DrawButtons()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUI.SetNextControlName("TextInput");
            m_TextInput = GUILayout.TextArea(m_TextInput);
            if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
            {
                _SendText();
                Event.current.Use(); // Mark the event as used to prevent further processing
            }
            if (GUILayout.Button("Send", SendBtnStyle))
            {
                _SendText();
            }
            GUI.FocusControl("TextInput");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Back", InworldEditor.Instance.BtnStyle))
            {
                InworldEditor.Instance.Status = EditorStatus.SelectMode;
            }
            if (GUILayout.Button("Clear Dialog", InworldEditor.Instance.BtnStyle))
            {
                m_ChatHistory.Clear();
            }
            GUILayout.EndHorizontal();
        }
        void _SendText()
        {
            if (string.IsNullOrEmpty(m_TextInput)) 
                return;
            m_ChatHistory.Add($"You: {m_TextInput}"); 
            m_ChatHistory.Add("Bot: Hi"); 
            m_TextInput = "";
            GUI.FocusControl("TextInput");
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
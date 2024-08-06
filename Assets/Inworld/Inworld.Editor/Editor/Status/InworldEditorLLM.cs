/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Entities;
using Inworld.Entities.LLM;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
                                                                                                                                                                                                                                                                                                                                                                                                                                                                       #if UNITY_EDITOR
namespace Inworld.Editors
{
    public class InworldEditorLLM : IEditorState
    {
        string m_TextInput;
        List<Message> m_ChatHistory = new List<Message>();
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
            normal = new GUIStyleState
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
            foreach (Message item in m_ChatHistory)
            {
                GUILayout.Box(item.ToMessage, item.IsPlayer ? PlayerLabel : BotLabel);
            }
            EditorGUILayout.EndScrollView();
        }
        public void DrawButtons()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUI.SetNextControlName("TextInput");
            m_TextInput = GUILayout.TextArea(m_TextInput);
            Event e = Event.current;
            if (e.type == EventType.KeyUp && e.keyCode == KeyCode.Return)
            {
                if (e.shift)
                    m_TextInput += "\n";
                else
                    _SendText();
                e.Use(); 
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
            m_ChatHistory.Add(MessageFactory.Create(m_TextInput)); 
            m_TextInput = "";
            string token = $"Basic UHlDWENQOTRCTnZ0THp5OHJBSUFWTHdObnNWd0lKSXU6V09OcmNiNGFBN0lnVDJkaE85NHlVTTlWeHp3RzFCa0c3azJ5VWx6enQzSEpLVTdxZHBBM3lJdkx1QUE3dFNwNg==";
            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                ["Authorization"] = InworldEditor.RuntimeToken
            };
            string jsonData = new CompleteChatRequest(new Serving(), m_ChatHistory, InworldEditor.Instance.LLMService.config).ToJson;
            Debug.Log($"YAN SEND {jsonData}");
            InworldEditorUtil.SendWebPostRequest(InworldEditor.CompleteChatURL, headers,  jsonData, OnChatCompleted);
        }
        void OnChatCompleted(AsyncOperation obj)
        {
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(obj);
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(InworldEditor.GetError(uwr.error));
                Debug.Log(uwr.downloadHandler.text);
                return;
            }
            NetworkCompleteChatResponse response = JsonConvert.DeserializeObject<NetworkCompleteChatResponse>(uwr.downloadHandler.text);
            foreach (var choice in response.result.choices)
            {
                Debug.Log(choice.message);
                m_ChatHistory.Add(MessageFactory.CreateFromAgent(choice.message.content)); 
            }
            
            GUI.FocusControl("TextInput");
        }
        public void OnExit()
        {
            
        }
        public void OnEnter()
        {
            //if (!InworldEditor.IsRuntimeTokenValid)
                InworldEditor.Instance.GetRuntimeAccessTokenAsync();
        }
        public void PostUpdate()
        {
            
        }
    }
}
#endif
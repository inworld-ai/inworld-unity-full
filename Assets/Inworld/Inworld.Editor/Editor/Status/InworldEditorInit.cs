/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using Inworld.Entities;
using Newtonsoft.Json;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
namespace Inworld.Editors
{
    public class InworldEditorInit : IEditorState
    {
        const string k_InputUserName = "Please input your name (Required)";
        const string k_DefaultTitle = "Please paste Studio API Key or Auth token here:";
        const string k_TitleLegacyToken = "Legacy Token Detected";
        const string k_ContentLegacyToken = "You're using legacy token. It would be deprecated by Aug 2025. Please use Studio API Key instead";
        const string k_DefaultPlayerName = "player";
        const string k_EmptyUserName = "Please input your name.";

        bool m_IsUserNameEmpty = false;
        Vector2 m_ScrollPosition = Vector2.zero;
        
        /// <summary>
        /// Triggers when open editor window.
        /// </summary>
        public void OnOpenWindow()
        {
            m_IsUserNameEmpty = false;
            InworldEditor.TokenForExchange = "";
        }
        /// <summary>
        /// Triggers when drawing the title of the editor panel page.
        /// </summary>
        public void DrawTitle()
        {
            if (m_IsUserNameEmpty)
                EditorGUILayout.LabelField(k_EmptyUserName, InworldEditor.Instance.ErrorStyle);
        }
        /// <summary>
        /// Triggers when drawing the content of the editor panel page.
        /// </summary>
        public void DrawContent()
        {
            GUILayout.Label(k_InputUserName, EditorStyles.boldLabel);
            InworldEditor.InputUserName = GUILayout.TextArea(InworldEditor.InputUserName);
            GUILayout.Label(k_DefaultTitle, EditorStyles.boldLabel);
            GUIStyle customStyle = new GUIStyle(GUI.skin.textArea)
            {
                padding = new RectOffset(10, 10, 10, 200),
                wordWrap = true 
            };
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            InworldEditor.TokenForExchange = GUILayout.TextArea(InworldEditor.TokenForExchange, customStyle);
            EditorGUILayout.EndScrollView();
        }
        /// <summary>
        /// Triggers when drawing the buttons at the bottom of the editor panel page.
        /// </summary>
        public void DrawButtons()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Connect", InworldEditor.Instance.BtnStyle))
            {
                if (InworldEditor.IsLegacyEntry)
                    EditorUtility.DisplayDialog(k_TitleLegacyToken, k_ContentLegacyToken, "OK");
                if (!string.IsNullOrEmpty(InworldEditor.InputUserName) && InworldEditor.InputUserName.ToLower() != k_DefaultPlayerName)
                    _CreateUserDirectory();
                else
                    m_IsUserNameEmpty = true;
            }
            GUILayout.EndHorizontal();

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
            InworldEditor.TokenForExchange = "";
        }
        /// <summary>
        /// Triggers when other general update logic has been finished.
        /// </summary>
        public void PostUpdate()
        {
            
        }

        void _ListWorkspace()
        {
            InworldEditorUtil.SendWebGetRequest(InworldEditor.ListWorkspaceURL, true, OnListWorkspaceCompleted);
            EditorUtility.DisplayProgressBar("Inworld", "Getting Workspace data...", 0.75f);
        }
        void _CreateUserDirectory()
        {
            // Create a new SO.
            InworldUserSetting newUser = ScriptableObject.CreateInstance<InworldUserSetting>();
            
            if (!Directory.Exists(InworldEditor.UserDataPath))
            {
                Directory.CreateDirectory(InworldEditor.UserDataPath);
            }
            if (!Directory.Exists($"{InworldEditor.UserDataPath}/{InworldEditor.InputUserName}"))
            {
                Directory.CreateDirectory($"{InworldEditor.UserDataPath}/{InworldEditor.InputUserName}");
            }
            string fileName = $"{InworldEditor.UserDataPath}/{InworldEditor.InputUserName}/{InworldEditor.InputUserName}.asset";
            AssetDatabase.CreateAsset(newUser, fileName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            InworldAI.User = newUser;
            InworldAI.User.Name = InworldEditor.InputUserName;
            EditorUtility.DisplayProgressBar("Inworld", "Create User Profile Completed!", 0.5f);
            InworldAI.LogEvent("Login_Studio");
            _ListWorkspace();
        }
        void OnListWorkspaceCompleted(AsyncOperation obj)
        {
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(obj);
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                EditorUtility.ClearProgressBar();
                InworldEditor.Instance.Error = $"List Workspace Failed: {InworldEditor.GetError(uwr.error)}";
                return;
            }
            EditorUtility.DisplayProgressBar("Inworld", "Getting Workspace data Completed", 1f);
            ListWorkspaceResponse response = JsonConvert.DeserializeObject<ListWorkspaceResponse>(uwr.downloadHandler.text);
            InworldAI.User.Workspace.Clear();
            InworldAI.User.Workspace.AddRange(response.workspaces);
            EditorUtility.ClearProgressBar();
            EditorUtility.SetDirty(InworldAI.User);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            InworldEditor.Instance.Status = EditorStatus.SelectGameData;
            
        }
    }
}
#endif
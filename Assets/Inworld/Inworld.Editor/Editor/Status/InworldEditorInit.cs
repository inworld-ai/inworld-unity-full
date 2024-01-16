/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using Inworld.Entities;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
namespace Inworld.Editors
{
    public class InworldEditorInit : IEditorState
    {
        const string k_DefaultTitle = "Please paste Auth token here:";

        Vector2 m_ScrollPosition = Vector2.zero;
        
        /// <summary>
        /// Triggers when open editor window.
        /// </summary>
        public void OnOpenWindow()
        {
            InworldEditor.TokenForExchange = "";
        }
        /// <summary>
        /// Triggers when drawing the title of the editor panel page.
        /// </summary>
        public void DrawTitle()
        {
            GUILayout.Label(k_DefaultTitle, EditorStyles.boldLabel);
        }
        /// <summary>
        /// Triggers when drawing the content of the editor panel page.
        /// </summary>
        public void DrawContent()
        {
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
                _GetBillingAccount();
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
        void _GetBillingAccount()
        {
            InworldEditorUtil.SendWebGetRequest(InworldEditor.BillingAccountURL, true, OnBillingAccountCompleted);
            EditorUtility.DisplayProgressBar("Inworld", "Getting Billing Account...", 0.25f);
        }

        void _ListWorkspace()
        {
            InworldEditorUtil.SendWebGetRequest(InworldEditor.ListWorkspaceURL, true, OnListWorkspaceCompleted);
            EditorUtility.DisplayProgressBar("Inworld", "Getting Workspace data...", 0.75f);
        }
        void OnBillingAccountCompleted(AsyncOperation obj)
        {
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(obj);
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                InworldEditor.Instance.Error = $"Get User Failed: {InworldEditor.GetError(uwr.error)}";
                EditorUtility.ClearProgressBar();
                return;
            }
            BillingAccountRespone date = JsonUtility.FromJson<BillingAccountRespone>(uwr.downloadHandler.text);
            if (date.billingAccounts.Count == 1)
            {
                string displayName = date.billingAccounts[0].displayName.Split('@')[0];
                if (!InworldAI.User || date.billingAccounts[0].name != InworldAI.User.BillingAccount)
                {
                    // Create a new SO.
                    InworldUserSetting newUser = ScriptableObject.CreateInstance<InworldUserSetting>();
                    
                    if (!Directory.Exists(InworldEditor.UserDataPath))
                    {
                        Directory.CreateDirectory(InworldEditor.UserDataPath);
                    }
                    if (!Directory.Exists($"{InworldEditor.UserDataPath}/{displayName}"))
                    {
                        Directory.CreateDirectory($"{InworldEditor.UserDataPath}/{displayName}");
                    }
                    string fileName = $"{InworldEditor.UserDataPath}/{displayName}/{displayName}.asset";
                    AssetDatabase.CreateAsset(newUser, fileName);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    InworldAI.User = newUser;
                }
                InworldAI.User.BillingAccount = date.billingAccounts[0].name;
                InworldAI.User.Name = displayName;
            }
            EditorUtility.DisplayProgressBar("Inworld", "Getting Billing Account Completed!", 0.5f);
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
            ListWorkspaceResponse response = JsonUtility.FromJson<ListWorkspaceResponse>(uwr.downloadHandler.text);
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
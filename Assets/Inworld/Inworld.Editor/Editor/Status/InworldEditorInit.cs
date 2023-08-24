using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
namespace Inworld.AI.Editor
{
    public class InworldEditorInit : IEditorState
    {
        const string k_DefaultTitle = "Please paste Auth token here:";
        const string k_UserDataPath = "Assets/Inworld/Inworld.Editor/Data";

        string m_Token = "";
        Vector2 m_ScrollPosition = Vector2.zero;
        public void DrawTitle()
        {
            GUILayout.Label(k_DefaultTitle, EditorStyles.boldLabel);
        }
        public void DrawContent()
        {
            GUIStyle customStyle = new GUIStyle(GUI.skin.textArea)
            {
                wordWrap = true 
            };
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            InworldEditor.TokenForExchange = GUILayout.TextArea(InworldEditor.TokenForExchange, customStyle);
            EditorGUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
        }
        public void DrawButtons()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Connect", InworldEditor.Instance.BtnStyle))
            {
                m_Token = InworldEditor.TokenForExchange.Split(':')[0];
                _GetBillingAccount();
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
        void _GetBillingAccount()
        {
            UnityWebRequest uwr = new UnityWebRequest(InworldEditor.BillingAccountURL,"GET");
            uwr.SetRequestHeader("Authorization", $"Bearer {m_Token}");
            uwr.downloadHandler = new DownloadHandlerBuffer();
            EditorUtility.DisplayProgressBar("Inworld", "Getting Billing Account...", 0.25f);
            UnityWebRequestAsyncOperation updateRequest = uwr.SendWebRequest();
            updateRequest.completed += OnBillingAccountCompleted;
        }

        void _ListWorkspace()
        {
            EditorUtility.DisplayProgressBar("Inworld", "Getting Workspace data...", 0.75f);
            UnityWebRequest uwr = new UnityWebRequest(InworldEditor.ListWorkspaceURL,"GET");
            uwr.SetRequestHeader("Authorization", $"Bearer {m_Token}");
            uwr.downloadHandler = new DownloadHandlerBuffer();
            UnityWebRequestAsyncOperation updateRequest = uwr.SendWebRequest();
            updateRequest.completed += OnListWorkspaceCompleted;
        }
        void OnBillingAccountCompleted(AsyncOperation obj)
        {
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(obj);
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                InworldEditor.Instance.Error = uwr.error;
                EditorUtility.ClearProgressBar();
                return;
            }
            BillingAccountRespone date = JsonUtility.FromJson<BillingAccountRespone>(uwr.downloadHandler.text);
            if (date.billingAccounts.Count == 1)
            {
                if (!InworldAI.User || date.billingAccounts[0].name != InworldAI.User.BillingAccount)
                {
                    // Create a new SO.
                    InworldUserSetting newUser = ScriptableObject.CreateInstance<InworldUserSetting>();
                    
                    if (!Directory.Exists(k_UserDataPath))
                    {
                        Directory.CreateDirectory(k_UserDataPath);
                    }
                    string fileName = $"{k_UserDataPath}/{date.billingAccounts[0].displayName}.asset";
                    AssetDatabase.CreateAsset(newUser, fileName);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    InworldAI.User = newUser;
                }
                InworldAI.User.BillingAccount = date.billingAccounts[0].name;
                string displayName = date.billingAccounts[0].displayName.Split('@')[0];
                InworldAI.User.Name = displayName;
            }
            EditorUtility.DisplayProgressBar("Inworld", "Getting Billing Account Completed!", 0.5f);
            _ListWorkspace();
        }
        void OnListWorkspaceCompleted(AsyncOperation obj)
        {
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(obj);
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                EditorUtility.ClearProgressBar();
                InworldEditor.Instance.Status = EditorStatus.Error;
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
            InworldEditor.Instance.Status = EditorStatus.SelectWorkspace;
            
        }
    }

}

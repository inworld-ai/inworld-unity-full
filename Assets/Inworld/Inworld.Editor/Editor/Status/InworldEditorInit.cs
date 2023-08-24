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
        const string k_TokenIncorrect = "Token Incorrect. Please paste again.";
        
        string m_ErrorMessage = "";
        string m_Token = "";
        Vector2 m_ScrollPosition = Vector2.zero;
        public void DrawTitle()
        {
            
        }
        public void DrawContent()
        {
            GUIStyle customStyle = new GUIStyle(GUI.skin.textArea)
            {
                wordWrap = true 
            };
            GUILayout.Label(k_DefaultTitle, EditorStyles.boldLabel);
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
                m_ErrorMessage = "";
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
            UnityWebRequestAsyncOperation updateRequest = uwr.SendWebRequest();
            updateRequest.completed += OnBillingAccountCompleted;
        }

        void _ListWorkspace()
        {
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
                return;
            }
            BillingAccountRespone date = JsonUtility.FromJson<BillingAccountRespone>(uwr.downloadHandler.text);
            if (date.billingAccounts.Count == 1)
            {
                if (!InworldAI.User || date.billingAccounts[0].name != InworldAI.User.BillingAccount)
                {
                    // Create a new SO.
                    InworldUserSetting newUser = ScriptableObject.CreateInstance<InworldUserSetting>();
                    string path = "Assets/Inworld/Inworld.Editor/Data";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string fileName = $"{path}/{date.billingAccounts[0].displayName}.asset";
                    AssetDatabase.CreateAsset(newUser, fileName);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    InworldAI.User = newUser;
                }
                InworldAI.User.BillingAccount = date.billingAccounts[0].name;
                string displayName = date.billingAccounts[0].displayName.Split('@')[0];
                InworldAI.User.Name = displayName;
            }
            _ListWorkspace();
        }
        void OnListWorkspaceCompleted(AsyncOperation obj)
        {
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(obj);
            if (uwr.result != UnityWebRequest.Result.Success)
                Debug.LogError(uwr.error);
            ListWorkspaceResponse response = JsonUtility.FromJson<ListWorkspaceResponse>(uwr.downloadHandler.text);
            InworldAI.User.Workspace.Clear();
            InworldAI.User.Workspace.AddRange(response.workspaces);
            EditorUtility.SetDirty(InworldAI.User);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            InworldEditor.Instance.Status = EditorStatus.SelectWorkspace;
        }
    }

}

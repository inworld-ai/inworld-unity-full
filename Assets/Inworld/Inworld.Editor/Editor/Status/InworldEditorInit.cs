#if UNITY_EDITOR
using Inworld.VSP;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
namespace Inworld.AI.Editor
{
    public class InworldEditorInit : IEditorState
    {
        const string k_DefaultTitle = "Please paste Auth token here:";

        Vector2 m_ScrollPosition = Vector2.zero;
        public void OnOpenWindow()
        {
            InworldEditor.TokenForExchange = "";
        }
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
        }
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
        public void OnExit()
        {
            
        }
        public void OnEnter()
        {
            InworldEditor.TokenForExchange = "";
        }
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
#if VSP
            if (!string.IsNullOrEmpty(InworldAI.User.Account))
                VSAttribution.SendAttributionEvent("Login Studio", InworldAI.k_CompanyName, InworldAI.User.Account);
#endif
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
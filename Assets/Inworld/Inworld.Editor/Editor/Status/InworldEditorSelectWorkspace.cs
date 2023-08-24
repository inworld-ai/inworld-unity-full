using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
namespace Inworld.AI.Editor
{
    // YAN: At this moment, the ws data has already filled.
    public class InworldEditorSelectWorkspace : IEditorState
    {
        string m_DefaultWorkspace = "--- SELECT WORKSPACE ---";
        string m_DefaultScene = "--- SELECT SCENE ---";
        string m_DefaultKey = "--- SELECT KEY---";
        string m_CurrentWorkspace = "--- SELECT WORKSPACE ---";
        string m_CurrentScene = "--- SELECT SCENE ---";
        string m_CurrentKey = "--- SELECT KEY---";

        public void DrawTitle()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Welcome, {InworldAI.User.Name}", InworldEditor.Instance.TitleStyle);
            EditorGUILayout.Space();
        }
        public void DrawContent()
        {
            EditorGUILayout.LabelField("Choose Workspace:", InworldEditor.Instance.TitleStyle);
            InworldEditorUtil.DrawDropDown(
                m_CurrentWorkspace, 
                InworldAI.User.Workspace.Select(ws => ws.displayName).ToList(), 
                s =>
                {
                    m_CurrentWorkspace = s;
                    m_CurrentScene = m_DefaultScene;
                    m_CurrentKey = m_DefaultKey; // YAN: Reset data.
                    _ListScenes();
                    _ListKeys();
                }
            );
            // if (m_CurrentWorkspace != m_DefaultWorkspace)
            // {
            //     _DrawSceneChooser();
            //     _DrawKeyChooser();
            // }
        }
        void _ListKeys()
        {
            string wsFullName = InworldAI.User.GetWorkspaceFullName(m_CurrentWorkspace);
            if (string.IsNullOrEmpty(wsFullName))
                return;
            InworldEditorUtil.SendWebGetRequest(InworldEditor.ListKeyURL(wsFullName), true, _ListKeyCompleted);
        }
        void _ListScenes()
        {
            string wsFullName = InworldAI.User.GetWorkspaceFullName(m_CurrentWorkspace);
            if (string.IsNullOrEmpty(wsFullName))
                return;
            InworldEditorUtil.SendWebGetRequest(InworldEditor.ListScenesURL(wsFullName), true, _ListSceneCompleted);
        }
        void _ListSceneCompleted(AsyncOperation obj)
        {
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(obj);
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                InworldEditor.Instance.Error = $"List Scene Failed: {uwr.error}";
                EditorUtility.ClearProgressBar();
                return;
            }
            ListSceneResponse resp = JsonUtility.FromJson<ListSceneResponse>(uwr.downloadHandler.text);
            InworldWorkspaceData ws = InworldAI.User.GetWorkspaceByDisplayName(m_CurrentWorkspace);
            ws.scenes ??= new List<InworldSceneData>();
            ws.scenes.Clear();
            ws.scenes.AddRange(resp.scenes); 
        }
        void _ListKeyCompleted(AsyncOperation obj)
        {
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(obj);
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                InworldEditor.Instance.Error = $"List Key Failed: {uwr.error}";
                EditorUtility.ClearProgressBar();
                return;
            }
            Debug.Log(uwr.downloadHandler.text);
            ListKeyResponse resp = JsonUtility.FromJson<ListKeyResponse>(uwr.downloadHandler.text);
            InworldWorkspaceData ws = InworldAI.User.GetWorkspaceByDisplayName(m_CurrentWorkspace);
            ws.keySecrets ??= new List<InworldKeySecret>();
            ws.keySecrets.Clear();
            ws.keySecrets.AddRange(resp.apiKeys); 
        }
        
        void _DrawKeyChooser()
        {
            throw new System.NotImplementedException();
        }
        void _DrawSceneChooser()
        {
            throw new System.NotImplementedException();
        }
        public void DrawButtons()
        {

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

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
        const string k_DefaultWorkspace = "--- SELECT WORKSPACE ---";
        const string k_DefaultScene = "--- SELECT SCENE ---";
        const string k_DefaultKey = "--- SELECT KEY---";
        const string k_DataMissing = "Some data is missing.\nPlease make sure you have at least one scene and one key/secret in your workspace";
        string m_CurrentWorkspace = "--- SELECT WORKSPACE ---";
        string m_CurrentScene = "--- SELECT SCENE ---";
        string m_CurrentKey = "--- SELECT KEY---";
        bool m_DisplayDataMissing = false;

        public void DrawTitle()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Welcome, {InworldAI.User.Name}", InworldEditor.Instance.TitleStyle);
            EditorGUILayout.Space();
        }
        public void DrawContent()
        {
            _DrawWorkspaceDropDown();
            _DrawSceneDropDown();
            _DrawKeyDropDown();
            if (m_DisplayDataMissing)
                EditorGUILayout.LabelField(k_DataMissing, InworldEditor.Instance.TitleStyle);
        }
        public void DrawButtons()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Back", InworldEditor.Instance.BtnStyle))
            {
                InworldEditor.Instance.Status = EditorStatus.Init;
            }
            if (m_CurrentWorkspace != k_DefaultWorkspace && !string.IsNullOrEmpty(m_CurrentWorkspace))
            {
                if (GUILayout.Button("Refresh", InworldEditor.Instance.BtnStyle))
                {
                    _SelectWorkspace(m_CurrentWorkspace);
                }
            }
            if (m_CurrentKey != k_DefaultKey && !string.IsNullOrEmpty(m_CurrentKey) && m_CurrentScene != k_DefaultScene && !string.IsNullOrEmpty(m_CurrentScene))
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Next", InworldEditor.Instance.BtnStyle))
                {
                    InworldEditor.Instance.Status = EditorStatus.SelectCharacter;
                }
            }
            GUILayout.EndHorizontal();
        }
        public void OnExit()
        {
            
        }
        public void OnEnter()
        {
            m_DisplayDataMissing = false;
            if (InworldAI.User.Workspace.Count != 1)
                return;
            _SelectWorkspace(InworldAI.User.Workspace[0].displayName);
        }
        public void PostUpdate()
        {
            
        }
        void _DrawWorkspaceDropDown()
        {
            EditorGUILayout.LabelField("Choose Workspace:", InworldEditor.Instance.TitleStyle);
            List<string> wsList = InworldAI.User.Workspace.Select(ws => ws.displayName).ToList();
            InworldEditorUtil.DrawDropDown(m_CurrentWorkspace, wsList, _SelectWorkspace);
        }
        void _DrawSceneDropDown()
        {
            if (m_CurrentWorkspace == k_DefaultWorkspace || string.IsNullOrEmpty(m_CurrentWorkspace))
                return;
            InworldWorkspaceData ws = InworldAI.User.GetWorkspaceByDisplayName(m_CurrentWorkspace);
            if (ws == null)
                return;
            List<string> sceneList = ws.scenes.Select(scene => scene.displayName).ToList();
            EditorGUILayout.LabelField("Choose Scene:", InworldEditor.Instance.TitleStyle);
            InworldEditorUtil.DrawDropDown(m_CurrentScene, sceneList, _SelectScenes);
            m_CurrentScene = sceneList.Count == 1 ? sceneList[0] : m_CurrentScene;
        }

        void _DrawKeyDropDown()
        {
            if (m_CurrentWorkspace == k_DefaultWorkspace || string.IsNullOrEmpty(m_CurrentWorkspace))
                return;
            InworldWorkspaceData ws = InworldAI.User.GetWorkspaceByDisplayName(m_CurrentWorkspace);
            if (ws == null)
                return;
            List<string> keyList = ws.keySecrets.Where(key => key.state == "ACTIVE").Select(key => key.key).ToList();
            EditorGUILayout.LabelField("Choose API Key:", InworldEditor.Instance.TitleStyle);
            InworldEditorUtil.DrawDropDown(m_CurrentKey, keyList, _SelectKeys);
            m_CurrentKey = keyList.Count == 1 ? keyList[0] : m_CurrentKey;
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
            if (resp.scenes.Count == 0)
            {
                m_DisplayDataMissing = true;
            }
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
            ListKeyResponse resp = JsonUtility.FromJson<ListKeyResponse>(uwr.downloadHandler.text);
            if (resp.apiKeys.Count == 0)
                m_DisplayDataMissing = true;
            InworldWorkspaceData ws = InworldAI.User.GetWorkspaceByDisplayName(m_CurrentWorkspace);
            ws.keySecrets ??= new List<InworldKeySecret>();
            ws.keySecrets.Clear();
            ws.keySecrets.AddRange(resp.apiKeys); 
        }


        void _SelectWorkspace(string workspaceDisplayName)
        {
            m_CurrentWorkspace = workspaceDisplayName;
            m_CurrentScene = k_DefaultScene;
            m_CurrentKey = k_DefaultKey; // YAN: Reset data.
            _ListScenes();
            _ListKeys();
        }
        void _SelectScenes(string sceneDisplayName)
        {
            m_CurrentScene = sceneDisplayName;
        }
        void _SelectKeys(string keyDisplayName)
        {
            m_CurrentKey = keyDisplayName;
        }
    }
}

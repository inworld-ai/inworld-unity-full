using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Inworld.AI.Editor
{
    // YAN: At this moment, the ws data has already filled.
    public class InworldEditorSelectWorkspace : IEditorState
    {
        string m_DefaultValue = "--- SELECT WORKSPACE ---";
        string m_CurrentValue = "--- SELECT WORKSPACE ---";
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
                m_CurrentValue, 
                InworldAI.User.Workspace.Select(ws => ws.displayName).ToList(), 
                s =>
                {
                    m_CurrentValue = s;
                    Debug.Log($"Chosen: {s}");
                }
            );
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

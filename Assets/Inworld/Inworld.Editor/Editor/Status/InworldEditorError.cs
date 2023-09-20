#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace Inworld.AI.Editor
{
    public class InworldEditorError : IEditorState
    {

        public void DrawTitle()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(InworldEditor.Instance.Error, InworldEditor.Instance.TitleStyle);
            EditorGUILayout.Space();
        }
        public void DrawContent()
        {

        }
        public void DrawButtons()
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Back", InworldEditor.Instance.BtnStyle))
            {
                InworldEditor.Instance.Status = EditorStatus.Init;
            }
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
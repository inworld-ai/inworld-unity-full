using UnityEditor;
using UnityEngine;
namespace Inworld.AI.Editor
{
    public class InworldEditorSelectCharacter: IEditorState
    {

        public void DrawTitle()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Please select characters", InworldEditor.Instance.TitleStyle);
            EditorGUILayout.Space();
        }
        public void DrawContent()
        {

        }
        public void DrawButtons()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Back", InworldEditor.Instance.BtnStyle))
            {
                InworldEditor.Instance.Status = EditorStatus.SelectGameData;
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
    }
}

using UnityEditor;
using UnityEngine;
namespace Inworld.AI.Editor
{
    public class InworldEditorInit : IEditorState
    {
        const string k_DefaultTitle = "Please paste Auth token here:";
        const string k_TokenIncorrect = "Token Incorrect. Please paste again.";
        
        string m_ErrorMessage = "";
        public void DrawTitle()
        {
            
        }
        public void DrawContent()
        {
            InworldEditor.TokenForExchange = EditorGUILayout.TextField("Token: ", InworldEditor.TokenForExchange);
            EditorGUILayout.Space(40);
            EditorGUILayout.LabelField(m_ErrorMessage);
            EditorGUILayout.Space(200);
        }
        public void DrawButtons()
        {
            if (GUILayout.Button("Connect", InworldEditor.Instance.BtnStyle))
            {
                m_ErrorMessage = "";
                string[] data = InworldEditor.TokenForExchange.Split(':');
                if (data.Length >= 0)
                    InworldEditor.Status = EditorStatus.SelectWorkspace;
                else
                    m_ErrorMessage = k_TokenIncorrect;
            }
        }
    }
}

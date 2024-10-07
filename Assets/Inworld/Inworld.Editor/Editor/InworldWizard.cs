/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Inworld.Editors
{
    public class InworldWizard : EditorWindow
    {
        Texture2D m_Logo;
        Texture2D m_Yes;
        Texture2D m_No;
        Texture2D m_Empty;
        Vector2 m_ScrollPosition;
        GUIStyle m_CenteredStyle;
        Color selectedColor = new Color(0.24f, 0.49f, 0.91f, 1.0f); // Selected blue color
        public static InworldWizard Instance => GetWindow<InworldWizard>("Inworld Package Management");
        string[] m_PackageNames = 
        {
            "Inworld Playground",
            "Apple Vision Pro Module",
            "Digital Human Module"
        };
        
        
        string[] m_PackageSize = 
        {
            "291 MB",
            "1.37 GB",
            "1.4 GB"
        };
        public void ShowPanel()
        {
            titleContent = new GUIContent("Inworld Package Management");
            Show();
        }
        
        private void OnEnable()
        {
            // Load your logo texture here
            m_Logo = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/unity-logo.png", typeof(Texture2D));
            m_Yes = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Yes.png", typeof(Texture2D));
            m_No = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/No.png", typeof(Texture2D));

        }
        Texture2D GetPreLogo(int i)
        {
            switch (i)
            {
                case 0:
                    return m_Yes;
                case 1:
                    return null;
                case 2:
                    return m_No;
            }
            return null;
        }
        
        void OnGUI()
        {
            _DrawBanner();
            if (!Instance)
                return;
            m_CenteredStyle = new GUIStyle(GUI.skin.label);
            m_CenteredStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("Please select the package to import", EditorStyles.boldLabel);
            GUILayout.Label("", EditorStyles.boldLabel);
            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition, false, true);

            for (int i = 0; i < m_PackageNames.Length; i++)
            {
                GUI.enabled = i != 2;
                Rect rect = EditorGUILayout.BeginHorizontal("box");
                if (i == 1) // Change the background color for the second item
                {
                    EditorGUI.DrawRect(rect, selectedColor);
                }
                GUILayout.Label(GetPreLogo(i), GUILayout.Width(50), GUILayout.Height(50));
                GUILayout.Label(m_Logo, GUILayout.Width(50), GUILayout.Height(50));
                GUILayout.Label(m_PackageNames[i], m_CenteredStyle,GUILayout.Width(200));
                GUILayout.Label(m_PackageSize[i], GUILayout.Width(50));
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
        
        void _DrawBanner()
        {
            EditorGUILayout.Space();
            EditorGUIUtility.fieldWidth = 1200f;
            Texture2D banner = InworldEditor.Banner;
            GUI.DrawTexture(new Rect(20, 20, banner.width * 0.1f, banner.height * 0.1f), banner);
            EditorGUILayout.Space(100);
        }
    }
}
#endif
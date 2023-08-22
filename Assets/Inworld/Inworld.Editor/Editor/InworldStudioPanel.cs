using System;
using UnityEditor;
using UnityEngine;

namespace Inworld.AI.Editor
{
    public class InworldStudioPanel : EditorWindow
    {
        /// <summary>
        ///     Get Instance of the InworldEditor.
        ///     It'll create a Inworld Studio Panel if the panel hasn't opened.
        /// </summary>
        public static InworldStudioPanel Instance => GetWindow<InworldStudioPanel>("Inworld Studio");
        /// <summary>
        ///     Open Inworld Studio Panel
        ///     It will detect and pop import window if you dont have TMP imported.
        /// </summary>
        public void ShowPanel()
        {
            titleContent = new GUIContent("Inworld Studio");
            Show();
        }
        void OnGUI()
        {
            _DrawBanner();
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

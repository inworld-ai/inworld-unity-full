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
        void OnEnable()
        {
            InworldEditor.Instance.CurrentState.OnOpenWindow();
        }
        void OnGUI()
        {
            _DrawBanner();
            if (!InworldEditor.Instance)
                return;
            InworldEditor.Instance.CurrentState.DrawTitle();
            InworldEditor.Instance.CurrentState.DrawContent();
            InworldEditor.Instance.CurrentState.DrawButtons();
        }
        void Update()
        {
            InworldEditor.Instance.CurrentState.PostUpdate();
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
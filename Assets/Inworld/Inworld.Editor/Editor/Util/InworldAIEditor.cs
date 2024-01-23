/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Inworld.Editors
{
    public class InworldAIEditor : EditorWindow
    {
        const string k_DefaultTitle = "Inworld Server will send you:";
        /// <summary>
        ///     Get Instance of the InworldAIEditor.
        ///     It'll create a Inworld AI Editing Panel for you to modify Scriptable Object Inworld.AI as it's in package folder.
        /// </summary>
        public static InworldAIEditor Instance => GetWindow<InworldAIEditor>("Inworld.AI Settings");
        /// <summary>
        ///     Open Inworld AI Editor Panel
        ///     It will detect and pop import window if you dont have TMP imported.
        /// </summary>
        public void ShowPanel()
        {
            titleContent = new GUIContent("Inworld.AI Settings");
            Show();
        }

        void OnGUI()
        {
            _DrawBanner();
            _DrawCapabilities();
        }

        void _DrawBanner()
        {
            EditorGUILayout.Space();
            EditorGUIUtility.fieldWidth = 1200f;
            Texture2D banner = InworldEditor.Banner;
            GUI.DrawTexture(new Rect(20, 20, banner.width * 0.1f, banner.height * 0.1f), banner);
            EditorGUILayout.Space(100);
        }
        void _DrawCapabilities()
        {
            GUILayout.Label(k_DefaultTitle, EditorStyles.boldLabel);
            GUILayout.Space(10);
            InworldAI.Capabilities.text = EditorGUILayout.Toggle("Text", InworldAI.Capabilities.text);
            InworldAI.Capabilities.audio = EditorGUILayout.Toggle("Audio", InworldAI.Capabilities.audio);
            InworldAI.Capabilities.emotions = EditorGUILayout.Toggle("Emotion", InworldAI.Capabilities.emotions);
            InworldAI.Capabilities.triggers = EditorGUILayout.Toggle("Triggers", InworldAI.Capabilities.triggers);
            InworldAI.Capabilities.interruptions = EditorGUILayout.Toggle("Interruptions", InworldAI.Capabilities.interruptions);
            InworldAI.Capabilities.relations = EditorGUILayout.Toggle("Relations", InworldAI.Capabilities.relations);
            InworldAI.Capabilities.narratedActions = EditorGUILayout.Toggle("Narrated Actions", InworldAI.Capabilities.narratedActions);
            InworldAI.Capabilities.phonemeInfo = EditorGUILayout.Toggle("Lipsync", InworldAI.Capabilities.phonemeInfo);
            GUILayout.Space(20);
            InworldAI.IsDebugMode = EditorGUILayout.Toggle("Debug Mode", InworldAI.IsDebugMode);
            if (GUI.changed)
            {
                EditorUtility.SetDirty(InworldAI.Instance);
            }
        }

    }
}
#endif

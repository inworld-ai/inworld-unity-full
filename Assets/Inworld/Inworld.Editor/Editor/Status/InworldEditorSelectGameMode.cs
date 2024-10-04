/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

#if UNITY_EDITOR
using Inworld.LLM;
using Inworld.LLM.ModelConfig;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Inworld.Editors
{
    public class InworldEditorSelectGameMode : IEditorState
    {
        ModelName m_ModelName;
        TextGenerationConfig m_TextGenerationConfig;
        SerializedObject m_SerializedObject;
        int m_MaxChatHistorySize = 100;
        
        public void OnOpenWindow()
        {
            if (!InworldController.Instance)
            {
                return;
            }
            m_ModelName = InworldController.LLM.Model;
            m_TextGenerationConfig = InworldController.LLM.Config;
            m_MaxChatHistorySize = InworldController.LLM.HistorySize;
        }
        public void DrawTitle()
        {

        }
        public void DrawContent()
        {
            // Create fields in the custom editor window

            GUILayout.Label("Model Configuration", EditorStyles.boldLabel);

            // ModelName enum field
            m_ModelName = (ModelName)EditorGUILayout.EnumPopup("Model Name", m_ModelName);

            // Max chat history size field
            m_MaxChatHistorySize = EditorGUILayout.IntField(new GUIContent("Max Chat History Size", 
                "How many chat history items displayed.\nNote: Too many items will confuse the service."), m_MaxChatHistorySize);

            GUILayout.Space(10);

            GUILayout.Label("Text Generation Configuration", EditorStyles.boldLabel);
            if (m_TextGenerationConfig == null)
                return;

            // Frequency penalty
            m_TextGenerationConfig.frequency_penalty = EditorGUILayout.FloatField(new GUIContent("Frequency Penalty",
                "Penalizes new tokens based on their frequency in the text."), m_TextGenerationConfig.frequency_penalty);

            // Max tokens
            m_TextGenerationConfig.max_tokens = EditorGUILayout.IntField(new GUIContent("Max Tokens",
                "Maximum number of output tokens."), m_TextGenerationConfig.max_tokens);

            // Presence penalty
            m_TextGenerationConfig.presence_penalty = EditorGUILayout.FloatField(new GUIContent("Presence Penalty",
                "Encourages the model to talk about new topics."), m_TextGenerationConfig.presence_penalty);

            // Stream option
            m_TextGenerationConfig.stream = EditorGUILayout.Toggle(new GUIContent("Stream", 
                "If set, partial message deltas will be sent."), m_TextGenerationConfig.stream);

            // Temperature and Top P
            m_TextGenerationConfig.temperature = EditorGUILayout.Slider(new GUIContent("Temperature",
                "Controls randomness in the output."), m_TextGenerationConfig.temperature, 0, 2);
            m_TextGenerationConfig.top_p = EditorGUILayout.Slider(new GUIContent("Top P",
                "Nucleus sampling threshold."), m_TextGenerationConfig.top_p, 0, 1);
            if (GUILayout.Button("Add PlayerController to Scene", GUILayout.ExpandWidth(true)))
            {
                InworldEditorUtil.AddPlayerController(InworldEditor.PlayerControllerLLM);
            }
        }
        public void DrawButtons()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Back", InworldEditor.Instance.BtnStyle))
            {
                InworldEditor.Instance.Status = EditorStatus.SelectGameData;
            }
            if (GUILayout.Button("Refresh", InworldEditor.Instance.BtnStyle))
            {
                _Refresh();
            }
            if (GUILayout.Button("Apply", InworldEditor.Instance.BtnStyle))
            {
                _Apply();
            }
            GUILayout.EndHorizontal();
        }
        public void OnExit()
        {

        }
        public void OnEnter()
        {
            _Refresh();
        }
        
        public void PostUpdate()
        {

        }
        public void OnClose()
        {
            _Apply(false);
        }
        void _Apply(bool needDialog = true)
        {
            if (!InworldController.Instance)
            {
                InworldEditor.Instance.Error = "Cannot Find InworldController";
                return;
            }
            InworldController.LLM.Model = m_ModelName;
            InworldController.LLM.Config = m_TextGenerationConfig;
            InworldController.LLM.HistorySize = m_MaxChatHistorySize;
            if (needDialog && !EditorUtility.DisplayDialog("Inworld", "LLM Config updated!", "OK"))
                return;
            PrefabUtility.RecordPrefabInstancePropertyModifications(InworldController.LLM);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        void _Refresh()
        {
            if (!InworldController.Instance)
            {
                InworldEditor.Instance.Error = "Cannot Find InworldController";
                return;
            }
            m_ModelName = InworldController.LLM.Model;
            m_TextGenerationConfig = InworldController.LLM.Config;
            m_MaxChatHistorySize = InworldController.LLM.HistorySize;
            m_SerializedObject = new SerializedObject(InworldController.Instance);  
        }
    }
}
#endif
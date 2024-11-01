/*************************************************************************************************
 * Copyright 2024 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR

using Inworld.BehaviorEngine;
using UnityEditor;
using UnityEngine;

namespace Inworld.Editors
{
    [CustomEditor(typeof(Task))]
    public class TaskCustomInspector : Editor
    {
        SerializedProperty m_TaskHandlerProperty;
        Task targetTask;

        void OnEnable()
        {
            m_TaskHandlerProperty = serializedObject.FindProperty("m_TaskHandler");
            
            targetTask = serializedObject.targetObject as Task;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(m_TaskHandlerProperty);

            TaskHandler taskHandler = BehaviorEngineEditorUtil.GetTaskHandlerObject(targetTask.TaskShortName);

            if (!BehaviorEngineEditorUtil.GetTaskHandlerScript(targetTask.TaskShortName))
            {
                if (GUILayout.Button("Create Default TaskHandler Script"))
                {
                    BehaviorEngineEditorUtil.CreateTaskHandlerScript(targetTask);
                }
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Generate Default TaskHandler ScriptableObject");
                GUILayout.Button("Link Default TaskHandler");
                EditorGUI.EndDisabledGroup();
            }
            else if(!taskHandler)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Create Default TaskHandler Script");
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("Generate Default TaskHandler ScriptableObject"))
                {
                    BehaviorEngineEditorUtil.GenerateTaskHandlerObject(BehaviorEngineEditorUtil.GetTaskHandlerClassName(targetTask.TaskShortName));
                }
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Link Default TaskHandler");
                EditorGUI.EndDisabledGroup();
            }
            else if(m_TaskHandlerProperty.objectReferenceValue != taskHandler)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Create Default TaskHandler Script");
                GUILayout.Button("Generate Default TaskHandler ScriptableObject");
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("Link Default TaskHandler"))
                {
                    m_TaskHandlerProperty.objectReferenceValue = taskHandler;
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Create TaskHandler Script");
                GUILayout.Button("Generate TaskHandler ScriptableObject");
                GUILayout.Button("Link Default TaskHandler");
                EditorGUI.EndDisabledGroup();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif

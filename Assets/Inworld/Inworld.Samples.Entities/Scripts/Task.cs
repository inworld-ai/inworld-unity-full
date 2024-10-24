/*************************************************************************************************
 * Copyright 2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;

namespace Inworld.BehaviorEngine
{
    [CustomEditor(typeof(Task))]
    public class TaskCustomInspector : Editor
    {
        MonoScript m_TaskHandlerMonoScript;
        GUIContent[] m_TaskHandlerOptions;
        List<MonoScript> m_TaskHandlers;

        SerializedProperty m_TaskHandlerProperty;

        int m_SelectedTaskHandlerIndex = -1;
        
        void OnEnable()
        {
            m_TaskHandlerProperty = serializedObject.FindProperty("m_TaskHandlerMonoScript");
            MonoScript m_CurrentTaskHandler = m_TaskHandlerProperty.objectReferenceValue as MonoScript;
            m_SelectedTaskHandlerIndex = -1;
            
            List<GUIContent> m_TaskHandlerOptionsList = new List<GUIContent>();
            m_TaskHandlers = new List<MonoScript>();
            MonoScript[] monoScripts = Resources.FindObjectsOfTypeAll<MonoScript>();
            for (int i = 0; i < monoScripts.Length; i++)
            {
                MonoScript monoScript = monoScripts[i];
                if (monoScript.GetClass() != null && monoScript.GetClass().BaseType == typeof(TaskHandler))
                {
                    m_TaskHandlers.Add(monoScript);
                    m_TaskHandlerOptionsList.Add(new GUIContent(monoScript.GetClass().Name));

                    if (m_CurrentTaskHandler == monoScript)
                        m_SelectedTaskHandlerIndex = m_TaskHandlers.Count - 1;
                }
            }
            m_TaskHandlerOptions = m_TaskHandlerOptionsList.ToArray();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Task Handler: ");
            m_SelectedTaskHandlerIndex = EditorGUILayout.Popup(m_SelectedTaskHandlerIndex, m_TaskHandlerOptions);
            EditorGUILayout.EndHorizontal();

            if (m_SelectedTaskHandlerIndex >= 0)
                m_TaskHandlerProperty.objectReferenceValue = m_TaskHandlers[m_SelectedTaskHandlerIndex];

            serializedObject.ApplyModifiedProperties();
        }
    }
    
    public class Task : ScriptableObject
    {
        public string TaskName => m_TaskName;
        public string TaskShortName => m_TaskName.Substring(m_TaskName.LastIndexOf('/') + 1);
        public ReadOnlyCollection<TaskParameter> TaskParameters => new ReadOnlyCollection<TaskParameter>(m_TaskParameters);
  
        [SerializeField] protected string m_TaskName;
        [SerializeField] protected List<TaskParameter> m_TaskParameters;
        [SerializeField] [HideInInspector] protected MonoScript m_TaskHandlerMonoScript;

        protected TaskHandler m_TaskHandler;

        [SerializeField] ReadOnlyCollection<TaskParameter> m_ReadOnlyTaskParameters;
        
        public void Initialize(string taskName, List<TaskParameter> parameters)
        {
            m_TaskName = taskName;
            m_TaskParameters = parameters;
            m_ReadOnlyTaskParameters = new ReadOnlyCollection<TaskParameter>(m_TaskParameters);
        }
        
        public bool Validate(InworldCharacter inworldCharacter, Dictionary<string, string> parameters, out string message)
        {
            if(m_TaskHandler == null)
                InworldAI.LogException("This task is missing a Task Handler.");
            
            return m_TaskHandler.Validate(inworldCharacter, parameters, out message);
        }

        public IEnumerator Execute(InworldCharacter inworldCharacter, Dictionary<string, string> parameters, TaskHandler.CompleteTask completeCallback, TaskHandler.FailTask failCallback)
        {
            if(m_TaskHandler == null)
                InworldAI.LogException("This task is missing a Task Handler.");

            m_TaskHandler.ClearEventListeners();
            m_TaskHandler.onTaskComplete += completeCallback;
            m_TaskHandler.onTaskFail += failCallback;
            
            return m_TaskHandler.Execute(inworldCharacter, parameters);
        }
        
        void OnEnable()
        {
            if(m_TaskHandlerMonoScript)
                m_TaskHandler = Activator.CreateInstance(m_TaskHandlerMonoScript.GetClass()) as TaskHandler;
        }
        
        
    }
}

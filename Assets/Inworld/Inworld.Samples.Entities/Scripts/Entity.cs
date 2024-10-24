/*************************************************************************************************
 * Copyright 2024 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;

namespace Inworld.BehaviorEngine
{
    [CustomEditor(typeof(Entity))]
    public class EntityCustomInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
        }
    }
    
    public class Entity : ScriptableObject
    {
        public string ID => m_Id;
        public string ShortID => m_Id.Substring(m_Id.LastIndexOf('/') + 1);
        public string DisplayName => m_DisplayName;
        public string Description => m_Description;
        
        public ReadOnlyCollection<Task> Tasks => new ReadOnlyCollection<Task>(m_Tasks);
        
        [SerializeField] protected string m_Id;
        [SerializeField] protected string m_DisplayName;
        [SerializeField] protected string m_Description;
        [SerializeField] protected List<Task> m_Tasks;

        [SerializeField] ReadOnlyCollection<Task> m_ReadOnlyTasks;

        public void Initialize(string id, string displayName, string description, List<Task> tasks)
        {
            m_Id = id;
            m_DisplayName = displayName;
            m_Description = description;
            m_Tasks = tasks;
            m_ReadOnlyTasks = new ReadOnlyCollection<Task>(m_Tasks);
        }
    }
}

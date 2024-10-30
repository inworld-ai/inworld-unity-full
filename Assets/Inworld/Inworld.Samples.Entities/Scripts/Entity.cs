/*************************************************************************************************
 * Copyright 2024 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Data;
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

        public bool Compare(InworldEntityData inworldEntityData)
        {
            if (inworldEntityData.name != m_Id ||
                inworldEntityData.displayName != m_DisplayName ||
                inworldEntityData.description != m_Description ||
                inworldEntityData.customTasks == null || m_Tasks == null ||
                inworldEntityData.customTasks.Count != m_Tasks.Count)
                return false;

            for (int i = 0; i < m_Tasks.Count; i++)
            {
                if (m_Tasks[i].TaskName != inworldEntityData.customTasks[i].task)
                    return false;
            }
            return true;
        }
        
        public void Initialize(string id, string displayName, string description, List<Task> tasks)
        {
            m_Id = id;
            m_DisplayName = displayName;
            m_Description = description;
            m_Tasks = tasks;
        }
    }
}

/*************************************************************************************************
 * Copyright 2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Data;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Inworld.BehaviorEngine
{
    public class Task : ScriptableObject
    {
        public string TaskName => m_TaskName;
        public string TaskShortName => m_TaskName.Substring(m_TaskName.LastIndexOf('/') + 1);
        public ReadOnlyCollection<TaskParameter> TaskParameters => new ReadOnlyCollection<TaskParameter>(m_TaskParameters);
        public TaskHandler TaskHandler => m_TaskHandler;
        
        [SerializeField] protected string m_TaskName;
        [SerializeField] protected List<TaskParameter> m_TaskParameters;
        [SerializeField] [HideInInspector] protected TaskHandler m_TaskHandler;

        public bool Compare(InworldTaskData inworldTaskData)
        {
            if (inworldTaskData.name != m_TaskName ||
                inworldTaskData.parameters == null ||
                m_TaskParameters == null ||
                inworldTaskData.parameters.Count != m_TaskParameters.Count)
                return false;

            for (int i = 0; i < m_TaskParameters.Count; i++)
            {
                if (!m_TaskParameters[i].Equals(inworldTaskData.parameters[i]))
                    return false;
            }
            return true;
        }
        
        public void Initialize(string taskName, List<TaskParameter> parameters)
        {
            m_TaskName = taskName;
            m_TaskParameters = parameters;
        }
        
        public bool Validate(InworldCharacter inworldCharacter, Dictionary<string, string> parameters, out string message)
        {
            if(m_TaskHandler == null)
                InworldAI.LogException("This task is missing a Task Handler.");
            
            return m_TaskHandler.Validate(inworldCharacter, parameters, out message);
        }

        public IEnumerator Execute(InworldCharacter inworldCharacter, TaskHandler.CompleteTask completeCallback, TaskHandler.FailTask failCallback)
        {
            if(m_TaskHandler == null)
                InworldAI.LogException("This task is missing a Task Handler.");

            m_TaskHandler.ClearEventListeners();
            m_TaskHandler.onTaskComplete += completeCallback;
            m_TaskHandler.onTaskFail += failCallback;
            
            return m_TaskHandler.Execute(inworldCharacter);
        }
    }
}

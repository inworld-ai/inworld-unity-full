/*************************************************************************************************
 * Copyright 2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inworld.BehaviorEngine
{
    public abstract class TaskHandler : ScriptableObject
    {
        public delegate void CompleteTask();
        public delegate void FailTask(string reason);
        public event CompleteTask onTaskComplete;
        public event FailTask onTaskFail;

        protected Task m_Task;

        protected Dictionary<string, EntityItem> m_EntityItems = new Dictionary<string, EntityItem>();

        public void Initialize(Task task)
        {
            m_Task = task;
        }
        
        public virtual bool Validate(InworldCharacter inworldCharacter, Dictionary<string, string> parameters, out string message)
        {
            foreach (TaskParameter taskParameter in m_Task.TaskParameters)
            {
                if (parameters.TryGetValue(taskParameter.Name, out string itemID) && EntityManager.Instance.FindItem(itemID, out EntityItem entityItem))
                    m_EntityItems.Add(itemID, entityItem);
                else
                {
                    message = $"Could not find the Entity Item: {itemID} for the given task: {m_Task.TaskShortName}";
                    return false;
                }
            }
            message = "";
            return true;
        }
        
        public abstract IEnumerator Execute(InworldCharacter inworldCharacter);

            public void ClearEventListeners()
            {
                onTaskComplete = null;
                onTaskFail = null;
            }
        }
}
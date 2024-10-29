/*************************************************************************************************
 * Copyright 2024 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System.Collections.Generic;
using Inworld.Data;
using Inworld.Packet;
using UnityEngine;
using UnityEngine.Events;

namespace Inworld.BehaviorEngine
{
    [RequireComponent(typeof(InworldCharacter))]
    public class InworldCharacterTaskHandler : SingletonBehavior<InworldCharacterTaskHandler>
    {
        public UnityEvent<string, string, Dictionary<string, string>> onTaskStart;
        public UnityEvent<string, string, Dictionary<string, string>> onTaskComplete;
        public UnityEvent<string, string, string, Dictionary<string, string>> onTaskFail;

        protected const string k_DoNothingTask = "do_nothing";
        
        protected InworldCharacter m_InworldCharacter;
        protected Task m_CurrentTask;
        protected Dictionary<string, string> m_CurrentTaskParameters;

        protected virtual void Awake()
        {
            m_InworldCharacter = GetComponent<InworldCharacter>();
        }
        
        protected virtual void OnEnable()
        {
            m_InworldCharacter.Event.onTaskReceived.AddListener(OnTaskReceived);
        }

        protected virtual void OnDisable()
        {
            if(m_InworldCharacter)
                m_InworldCharacter.Event.onTaskReceived.RemoveListener(OnTaskReceived);
        }

        protected virtual void OnTaskReceived(string brainName, string taskName, List<TriggerParameter> parameters)
        {
            InworldCharacter inworldCharacter = InworldController.CharacterHandler.GetCharacterByBrainName(brainName);
            if (EntityManager.Instance.FindTask(taskName, out Task task))
            {
                Dictionary<string, string> parametersDictionary = ParseParameters(parameters);
                if(task.Validate(inworldCharacter, parametersDictionary, out string message))
                    StartTask(task, parametersDictionary);
                else
                {
                    m_CurrentTask = task;
                    m_CurrentTaskParameters = parametersDictionary;
                    FailCurrentTask(message);
                }
            }
            else if(!taskName.Equals(k_DoNothingTask))
                InworldAI.LogWarning($"Unsupported task: {taskName}");
        }
        
        protected Dictionary<string, string> ParseParameters(List<TriggerParameter> parameters)
        {
            Dictionary<string, string> parameterDictionary = new Dictionary<string, string>();
            foreach (TriggerParameter triggerParameter in parameters)
                parameterDictionary.Add(triggerParameter.name, triggerParameter.value);
            return parameterDictionary;
        }

        protected virtual void StartTask(Task task, Dictionary<string, string> parameters)
        {
            if (m_CurrentTask)
            {
                InworldAI.LogError($"{m_InworldCharacter.Name} attempted to start a new task ({task.TaskName}) while one was ongoing.");
                return;
            }
            
            m_CurrentTask = task;
            m_CurrentTaskParameters = parameters;
            
            onTaskStart?.Invoke(m_InworldCharacter.BrainName, task.TaskName, parameters);
            StartCoroutine(task.Execute(m_InworldCharacter, CompleteCurrentTask, FailCurrentTask));
            InworldAI.Log($"{m_InworldCharacter.Name} started task: {task.name}");
        }
        
        protected virtual void CompleteCurrentTask()
        {
            if (!m_CurrentTask || m_CurrentTaskParameters == null)
                return;
            
            InworldMessenger.SendTaskSucceeded(m_CurrentTaskParameters["task_id"], m_InworldCharacter.BrainName);
            onTaskComplete?.Invoke(m_InworldCharacter.BrainName, m_CurrentTask.TaskName, m_CurrentTaskParameters);
            InworldAI.Log($"{m_InworldCharacter.Name} completed task: {m_CurrentTask.name}");
            
            m_CurrentTask = null;
            m_CurrentTaskParameters = null;
        }
        
        protected virtual void FailCurrentTask(string reason)
        {
            if (!m_CurrentTask || m_CurrentTaskParameters == null)
                return;
            
            InworldMessenger.SendTaskFailed(m_CurrentTaskParameters["task_id"], reason, m_InworldCharacter.BrainName);
            onTaskFail?.Invoke(m_InworldCharacter.BrainName, m_CurrentTask.TaskName, reason, m_CurrentTaskParameters);
            InworldAI.Log($"{m_InworldCharacter.Name} failed task: {m_CurrentTask.name} because: {reason}");

            m_CurrentTask = null;
            m_CurrentTaskParameters = null;
        }
    }
}

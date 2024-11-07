/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

#if UNITY_EDITOR
using Inworld.BehaviorEngine;
using Inworld.Data;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Inworld.Editors
{
    public class InworldEditorSelectBehaviorEngine : IEditorState
    {
        InworldWorkspaceData m_CurrentWorkspace;
        InworldGameData m_CurrentGameData;
        
        Vector2 m_EntitiesScrollPosition, m_TasksScrollPosition;
        
        public void OnOpenWindow()
        {
            if (!InworldController.Instance || !InworldController.Instance.GameData)
            {
                InworldEditor.Instance.Status = EditorStatus.SelectGameData;
            }
            else
                _InitDataSelection();
        }
        public void DrawTitle()
        {

        }
        public void DrawContent()
        {
            if (m_CurrentWorkspace == null || !m_CurrentGameData)
                return;

            GUILayout.Label("Behavior Engine Setup", EditorStyles.whiteLargeLabel);
   
            GUILayout.BeginHorizontal();
            _ListEntities();
            _ListTasks();
            _ListTaskHandlers();
            GUILayout.EndHorizontal();
            
            _DrawEntityManagerButtons();
        }

        void _DrawEntityManagerButtons()
        {
            GameObject entityManagerGameObject = BehaviorEngineEditorUtil.GetEntityManagerObject();

            if (!entityManagerGameObject)
                return;
            
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Entity Manager", GUILayout.MaxWidth(140));
            
            if(GUILayout.Button("Prefab", GUILayout.MaxWidth(100)))
            {
                Selection.activeObject = entityManagerGameObject;
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            if(GUILayout.Button("Add to Scene", GUILayout.MaxWidth(100)))
            {
                if(!Object.FindObjectOfType<EntityManager>())
                    PrefabUtility.InstantiatePrefab(entityManagerGameObject);
                else 
                    Debug.LogError("Scene already contains an Entity Manager.");
            }
            GUILayout.EndHorizontal();
        }
        
        void _ListEntities()
        {
            GUILayout.BeginVertical(GUILayout.MaxWidth(256));
            GUILayout.Label("Entities", EditorStyles.label);
            m_EntitiesScrollPosition = GUILayout.BeginScrollView(m_EntitiesScrollPosition);
            GUILayout.BeginVertical();
            foreach (InworldEntityData entityData in m_CurrentWorkspace.entities.Where
                (entityData => GUILayout.Button(entityData.displayName, GUILayout.MaxWidth(200))))
            {
                Selection.activeObject = BehaviorEngineEditorUtil.GetEntityObject(entityData.displayName);
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
        
        void _ListTasks()
        {
            GUILayout.BeginVertical(GUILayout.MaxWidth(256));
            GUILayout.Label("Tasks", EditorStyles.label);
            m_TasksScrollPosition = GUILayout.BeginScrollView(m_TasksScrollPosition);
            GUILayout.BeginVertical();
            foreach (InworldTaskData taskData in m_CurrentWorkspace.tasks)
            {
                Task task = BehaviorEngineEditorUtil.GetTaskObject(taskData.ShortName);
                if(!task) continue;
                
                TaskHandler taskHandlerObject = task.TaskHandler;
                MonoScript taskHandlerScript;
                if (taskHandlerObject)
                    taskHandlerScript = MonoScript.FromScriptableObject(taskHandlerObject);
                else
                    taskHandlerScript = BehaviorEngineEditorUtil.GetTaskHandlerScript(taskData.ShortName);

                Color taskTextColor = Color.red;
                if (taskHandlerObject)
                    taskTextColor = Color.green;
                else if (taskHandlerScript)
                    taskTextColor = Color.yellow;
                
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    normal = new GUIStyleState()
                    {
                        textColor = taskTextColor
                    }
                };

                if (GUILayout.Button(taskData.ShortName, buttonStyle, GUILayout.MaxWidth(200)))
                    _PingObject(task);
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
        
        void _ListTaskHandlers()
        {
            GUILayout.BeginVertical(GUILayout.MaxWidth(512));
            GUILayout.Label("TaskHandlers", EditorStyles.label);
            m_TasksScrollPosition = GUILayout.BeginScrollView(m_TasksScrollPosition);
            GUILayout.BeginVertical();
            int scriptCount = 0, objectCount = 0;
            foreach (InworldTaskData taskData in m_CurrentWorkspace.tasks)
            {
                Task task = BehaviorEngineEditorUtil.GetTaskObject(taskData.ShortName);
                if(!task) continue;
                
                TaskHandler taskHandlerObject = task.TaskHandler;
                MonoScript taskHandlerScript;
                if (taskHandlerObject)
                    taskHandlerScript = MonoScript.FromScriptableObject(taskHandlerObject);
                else
                    taskHandlerScript = BehaviorEngineEditorUtil.GetTaskHandlerScript(taskData.ShortName);
                
                GUIStyle blueButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    normal = new GUIStyleState()
                    {
                        textColor = Color.blue
                    }
                };
                
                GUILayout.BeginHorizontal();
                if (taskHandlerScript)
                {
                    scriptCount++;
                    if (GUILayout.Button(taskHandlerScript.name + " (Script)", blueButtonStyle, GUILayout.MaxWidth(243)))
                        _PingObject(taskHandlerScript);
                    
                    if (taskHandlerObject)
                    {
                        objectCount++;
                        if (GUILayout.Button(taskHandlerObject.name + " (Object)", blueButtonStyle, GUILayout.MaxWidth(243)))
                            _PingObject(taskHandlerObject);
                    }
                    else
                    {
                        if (GUILayout.Button("Link to Task", GUILayout.MaxWidth(120)))
                        {
                            BehaviorEngineEditorUtil.GenerateTaskHandlerObject(taskHandlerScript.GetClass().ToString(), false);
                            BehaviorEngineEditorUtil.LinkTaskHandlerObject(task);
                        } 
                    }
                }
                else
                {
                    if (GUILayout.Button("Generate Script", GUILayout.MaxWidth(120)))
                    {
                        BehaviorEngineEditorUtil.CreateTaskHandlerScript(task);
                    } 
                    EditorGUI.BeginDisabledGroup(true);
                    GUILayout.Button("Link to Task", GUILayout.MaxWidth(120));
                    EditorGUI.EndDisabledGroup();
                }
                
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (scriptCount != m_CurrentWorkspace.tasks.Count)
            {
                if (GUILayout.Button("Generate all Scripts", GUILayout.MaxWidth(160)))
                {
                    _GenerateAllTaskHandlerScripts();
                } 
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Generate all Objects", GUILayout.MaxWidth(160));
                EditorGUI.EndDisabledGroup();
            }
            else if (objectCount != m_CurrentWorkspace.tasks.Count)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Generate all Scripts", GUILayout.MaxWidth(160));
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("Generate all Objects", GUILayout.MaxWidth(160)))
                    _GenerateAllTaskHandlerObjects();
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Generate all Scripts", GUILayout.MaxWidth(160));
                GUILayout.Button("Generate all Objects", GUILayout.MaxWidth(160));
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
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
                BehaviorEngineEditorUtil.DownloadEntitiesTasks(m_CurrentWorkspace);
            }
            GUILayout.EndHorizontal();
        }
        public void OnExit()
        {

        }
        public void OnEnter()
        {
            EditorUtility.ClearProgressBar();
            _InitDataSelection();
        }
        
        public void PostUpdate()
        {

        }
        public void OnClose()
        {

        }
        
        void _GenerateAllTaskHandlerScripts()
        {
            foreach (InworldTaskData taskData in m_CurrentWorkspace.tasks)
            {
                Task task = BehaviorEngineEditorUtil.GetTaskObject(taskData.ShortName);
                if(!task) continue;
                        
                TaskHandler taskHandlerObject = task.TaskHandler;
                MonoScript taskHandlerScript;
                if (taskHandlerObject)
                    taskHandlerScript = MonoScript.FromScriptableObject(taskHandlerObject);
                else
                    taskHandlerScript = BehaviorEngineEditorUtil.GetTaskHandlerScript(taskData.ShortName);
                        
                if(!taskHandlerScript)
                    BehaviorEngineEditorUtil.CreateTaskHandlerScript(task, false);
            }
            AssetDatabase.Refresh();
        }
        
        void _GenerateAllTaskHandlerObjects()
        {
            foreach (InworldTaskData taskData in m_CurrentWorkspace.tasks)
            {
                Task task = BehaviorEngineEditorUtil.GetTaskObject(taskData.ShortName);
                TaskHandler taskHandlerObject = task.TaskHandler;
                MonoScript taskHandlerScript;
                if (taskHandlerObject)
                {
                    taskHandlerScript = MonoScript.FromScriptableObject(taskHandlerObject);
                }
                else
                {
                    taskHandlerObject = BehaviorEngineEditorUtil.GetTaskHandlerObject(taskData.ShortName);
                    taskHandlerScript = BehaviorEngineEditorUtil.GetTaskHandlerScript(taskData.ShortName);
                }
                        
                if(!taskHandlerObject)
                    BehaviorEngineEditorUtil.GenerateTaskHandlerObject(taskHandlerScript.GetClass().ToString(), false);
                BehaviorEngineEditorUtil.LinkTaskHandlerObject(task, false);
            }
            AssetDatabase.Refresh();
        }
        
        void _InitDataSelection()
        {
            if (!InworldController.Instance)
                return;
            m_CurrentGameData = InworldController.Instance.GameData;
            if (InworldAI.User && InworldAI.User.Workspace != null && InworldAI.User.Workspace.Count != 0)
                m_CurrentWorkspace = InworldAI.User.Workspace.FirstOrDefault(ws => ws.name == m_CurrentGameData.workspaceFullName);
        }
        
        void _PingObject(Object obj)
        {
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
    }
}
#endif
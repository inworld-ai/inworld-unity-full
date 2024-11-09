/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR

using Inworld.BehaviorEngine;
using Inworld.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Inworld.Editors
{
    public static class BehaviorEngineEditorUtil
    {
        static string k_TaskHandlerDirectoryPath = Path.Combine(InworldEditorUtil.UserDataPath, InworldEditor.TaskHandlerPath);
        static string k_TaskHandlerScriptDirectoryPath = Path.Combine(k_TaskHandlerDirectoryPath, "Scripts");
        static string k_TaskHandlerObjectDirectoryPath = Path.Combine(k_TaskHandlerDirectoryPath, "ScriptableObjects");
        static string k_TaskDirectoryPath = Path.Combine(InworldEditorUtil.UserDataPath, InworldEditor.TaskPath);
        static string k_EntityDirectoryPath = Path.Combine(InworldEditorUtil.UserDataPath, InworldEditor.EntityPath);
        static string k_EntityManagerDirectoryPath = Path.Combine(InworldEditorUtil.UserDataPath, InworldEditor.BehaviorEngineDirectoryPath);
        static string k_EntityManagerPath = Path.Combine(k_EntityManagerDirectoryPath, InworldEditor.EntityManagerPrefab.name + ".prefab");

        static int m_EntitiesDownloadProgress = 0;
        
        public static string GetTaskHandlerClassName(string taskShortName)
        {
            return _CapitalizeString(taskShortName) + "TaskHandler";
        }
        
        public static MonoScript GetTaskHandlerScript(string taskShortName)
        {
            return AssetDatabase.LoadAssetAtPath<MonoScript>(_GetTaskHandlerScriptPath(taskShortName));
        }
        
        public static TaskHandler GetTaskHandlerObject(string taskShortName)
        {
            return AssetDatabase.LoadAssetAtPath<TaskHandler>(_GetTaskHandlerObjectPath(taskShortName));
        }

        public static Entity GetEntityObject(string entityDisplayName)
        {
            return AssetDatabase.LoadAssetAtPath<Entity>(_GetEntityPath(entityDisplayName));
        }
        
        public static Task GetTaskObject(string taskShortName)
        {
            return AssetDatabase.LoadAssetAtPath<Task>(_GetTaskPath(taskShortName));
        }
        
        public static GameObject GetEntityManagerObject()
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(k_EntityManagerPath);
        }

        public static void LinkTaskHandlerObject(Task task, bool refreshDatabase = true)
        {
            TaskHandler taskHandler = AssetDatabase.LoadAssetAtPath<TaskHandler>(_GetTaskHandlerObjectPath(task.TaskShortName));
            SerializedObject taskSO = new SerializedObject(task);
            taskSO.FindProperty("m_TaskHandler").objectReferenceValue = taskHandler;
            taskSO.ApplyModifiedProperties();
            
            if(refreshDatabase)
                AssetDatabase.Refresh();
        }

        public static void GenerateTaskHandlerObject(string taskHandlerClassName, bool refreshDatabase = true)
        {
            string taskHandlerObjectPath = Path.Combine(k_TaskHandlerObjectDirectoryPath, taskHandlerClassName + ".asset");

            if (AssetDatabase.LoadAssetAtPath<TaskHandler>(taskHandlerObjectPath))
                return;
            
            TaskHandler taskHandler = ScriptableObject.CreateInstance(taskHandlerClassName) as TaskHandler;
            if (!taskHandler)
            {
                InworldAI.LogError("Failed to create Task Handler object.");
                return;
            }
            
            if (!Directory.Exists(k_TaskHandlerObjectDirectoryPath))
                Directory.CreateDirectory(k_TaskHandlerObjectDirectoryPath);
            
            AssetDatabase.CreateAsset(taskHandler, taskHandlerObjectPath);
            Debug.Log($"Task Handler ScriptableObject: {taskHandler.name}, created at: {taskHandlerObjectPath}");
            
            AssetDatabase.SaveAssets();
            
            if(refreshDatabase)
                AssetDatabase.Refresh();
        }
        
        public static void CreateTaskHandlerScript(Task task, bool refreshDatabase = true)
        {
            if (!Directory.Exists(k_TaskHandlerScriptDirectoryPath))
                Directory.CreateDirectory(k_TaskHandlerScriptDirectoryPath);

            string taskHandlerClassName = GetTaskHandlerClassName(task.TaskShortName);
            string taskHandlerScriptFilePath = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')), 
                                                            _GetTaskHandlerScriptPath(task.TaskShortName));
            
            if (File.Exists(taskHandlerScriptFilePath))
                return;
            
            const string templateScriptName = "TemplateTaskHandler";
            string[] guids = AssetDatabase.FindAssets("t:Script " + templateScriptName);

            if (guids.Length == 0)
            {
                Debug.LogError("Could not find TemplateTaskHandler script.");
                return;
            }

            string templateScriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);

            string scriptText = File.ReadAllText(templateScriptPath);
            
            scriptText = scriptText.Replace("TemplateTaskHandler", taskHandlerClassName);
            
            File.WriteAllText(taskHandlerScriptFilePath, scriptText);
            Debug.Log($"Task Handler script: {taskHandlerClassName}, created at: {taskHandlerScriptFilePath}");
            
            if(refreshDatabase)
                AssetDatabase.Refresh();
        }

        public static void DownloadEntitiesTasks(InworldWorkspaceData inworldWorkspaceData)
        {
            string wsFullName = inworldWorkspaceData.name;
            if (string.IsNullOrEmpty(wsFullName))
                return;
            m_EntitiesDownloadProgress = 0;
            InworldEditorUtil.SendWebGetRequest(InworldEditor.GetEntitiesURL(wsFullName), true, (obj) => _DownloadEntitiesCompleted(obj, inworldWorkspaceData));
            InworldEditorUtil.SendWebGetRequest(InworldEditor.GetTasksURL(wsFullName), true, (obj) => _DownloadTasksCompleted(obj, inworldWorkspaceData));
        }
        
        static void _DownloadEntitiesCompleted(AsyncOperation obj, InworldWorkspaceData ws) 
        {
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(obj);
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                InworldEditor.Instance.Error = $"Get Entities Failed: {InworldEditor.GetError(uwr.error)}";
                EditorUtility.ClearProgressBar();
                return;
            }            

            ListEntityResponse resp = JsonConvert.DeserializeObject<ListEntityResponse>(uwr.downloadHandler.text);
            
            if (ws.entities == null)
                ws.entities = new List<InworldEntityData>();
            ws.entities.Clear();
            ws.entities.AddRange(resp.entities);
            ws.entities.Sort
            (
                (entity1, entity2) =>
                {
                    if (entity1 == null && entity2 == null) return 0;
                    if (entity1 == null) return 1;
                    if (entity2 == null) return -1;
                    
                    return String.Compare(entity1.displayName, entity2.displayName, StringComparison.OrdinalIgnoreCase);
                }
            );
            
            if (++m_EntitiesDownloadProgress >= 2)
                _CreateObjects(ws);
        }
        
        static void _DownloadTasksCompleted(AsyncOperation obj, InworldWorkspaceData ws) 
        {
            UnityWebRequest uwr = InworldEditorUtil.GetResponse(obj);
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                InworldEditor.Instance.Error = $"Get Tasks Failed: {InworldEditor.GetError(uwr.error)}";
                EditorUtility.ClearProgressBar();
                return;
            }            

            ListTaskResponse resp = JsonConvert.DeserializeObject<ListTaskResponse>(uwr.downloadHandler.text);
            if (ws.tasks == null)
                ws.tasks = new List<InworldTaskData>();
            ws.tasks.Clear();
            ws.tasks.AddRange(resp.customTasks); 
            ws.tasks.Sort
            (
                (task1, task2) =>
                {
                    if (task1 == null && task2 == null) return 0;
                    if (task1 == null) return 1;
                    if (task2 == null) return -1;
                    
                    return String.Compare(task1.ShortName, task2.ShortName, StringComparison.OrdinalIgnoreCase);
                }
            );
            
            if (++m_EntitiesDownloadProgress >= 2)
                _CreateObjects(ws);
        }

        static void _CreateObjects(InworldWorkspaceData inworldWorkspaceData)
        {
            List<Entity> entities = _CreateEntities(inworldWorkspaceData);
            List<Task> tasks = _CreateTasks(inworldWorkspaceData, entities);
            _CreateEntityManager(tasks);
        }
        
        static List<Entity> _CreateEntities(InworldWorkspaceData inworldWorkspaceData)
        {
            if (!Directory.Exists(k_EntityDirectoryPath))
                Directory.CreateDirectory(k_EntityDirectoryPath);

            List<Entity> entities = new List<Entity>();
                
            foreach (InworldEntityData inworldEntityData in inworldWorkspaceData.entities)
            {
                string newAssetPath = _GetEntityPath(inworldEntityData.displayName);
                Entity entity = AssetDatabase.LoadAssetAtPath<Entity>(newAssetPath);
                bool isLoaded = entity != null;
                if (!isLoaded)
                    entity = ScriptableObject.CreateInstance<Entity>();
                
                entity.Initialize(inworldEntityData.name, inworldEntityData.displayName, inworldEntityData.description);
                
                if(!isLoaded)
                    AssetDatabase.CreateAsset(entity, newAssetPath);
                
                entities.Add(entity);
            }
                
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return entities;
        }
        
        static List<Task> _CreateTasks(InworldWorkspaceData inworldWorkspaceData, List<Entity> entities)
        {
            if (!Directory.Exists(k_TaskDirectoryPath))
                Directory.CreateDirectory(k_TaskDirectoryPath);

            List<Task> tasks = new List<Task>();
                
            foreach (InworldTaskData inworldTaskData in inworldWorkspaceData.tasks)
            {
                string newAssetPath = _GetTaskPath(inworldTaskData.ShortName);
                Task task = AssetDatabase.LoadAssetAtPath<Task>(newAssetPath);
                bool isLoaded = task != null;
                if (!isLoaded)
                    task = ScriptableObject.CreateInstance<Task>();

                List<TaskParameter> taskParameters = new List<TaskParameter>();
                foreach (TaskParameterData taskParameterData in inworldTaskData.parameters)
                {
                    List<Entity> parameterEntities = new List<Entity>();
                    foreach (string entityID in taskParameterData.entities)
                    {
                        Entity entity = entities.Find((entity) => entity.ID == entityID);
                        
                        if(entity)
                            parameterEntities.Add(entity);
                    }
                    taskParameters.Add(new TaskParameter(taskParameterData.name, taskParameterData.description, parameterEntities));
                }

                task.Initialize(inworldTaskData.name, taskParameters);
                
                if(!isLoaded)
                    AssetDatabase.CreateAsset(task, newAssetPath);

                tasks.Add(task);
            }
                
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return tasks;
        }
        
        static void _CreateEntityManager(List<Task> tasks)
        {
            GameObject entityManagerObject = GetEntityManagerObject();
            bool objectExists = entityManagerObject != null;
            if(!objectExists)
                entityManagerObject = PrefabUtility.InstantiatePrefab(InworldEditor.EntityManagerPrefab) as GameObject;
            
            if (entityManagerObject == null)
                return;

            EntityManager entityManager = entityManagerObject.GetComponent<EntityManager>();
            SerializedObject so = new SerializedObject(entityManager);
            SerializedProperty tasksProperty = so.FindProperty("m_Tasks");
            
            tasksProperty.ClearArray();
            tasksProperty.arraySize = tasks.Count;
            for (int i = 0; i < tasks.Count; i++)
            {
                SerializedProperty taskProperty = tasksProperty.GetArrayElementAtIndex(i);
                taskProperty.objectReferenceValue = tasks[i];
            }
            so.ApplyModifiedProperties();
            
            if (!Directory.Exists(k_EntityManagerDirectoryPath))
                Directory.CreateDirectory(k_EntityManagerDirectoryPath);
            PrefabUtility.SaveAsPrefabAsset(entityManagerObject, k_EntityManagerPath);
            
            if(!objectExists)
                UnityEngine.Object.DestroyImmediate(entityManagerObject);
        }
        
        static string _CapitalizeString(string value)
        {
            char[] charArray = value.ToCharArray();
            charArray[0] = char.ToUpper(charArray[0]);
            return new string(charArray);
        }
        
        static string _GetTaskHandlerScriptPath(string taskShortName)
        {
            return Path.Combine(k_TaskHandlerScriptDirectoryPath, GetTaskHandlerClassName(taskShortName) + ".cs");
        }
        
        static string _GetTaskHandlerObjectPath(string taskShortName)
        {
            return Path.Combine(k_TaskHandlerObjectDirectoryPath, GetTaskHandlerClassName(taskShortName) + ".asset");
        }
        
        static string _GetTaskPath(string taskShortName)
        {
            return Path.Combine(k_TaskDirectoryPath, taskShortName.ToLower() + "Task.asset");
        }

        static string _GetEntityPath(string entityDisplayName)
        {
            return Path.Combine(k_EntityDirectoryPath, entityDisplayName + " Entity.asset");
        }
    }
}
#endif
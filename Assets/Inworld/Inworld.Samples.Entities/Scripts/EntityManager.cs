/*************************************************************************************************
 * Copyright 2024 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace Inworld.BehaviorEngine
{
    public class EntityManager : SingletonBehavior<EntityManager>
    {
        protected Dictionary<string, Task> m_TaskDictionary;
        protected List<EntityItem> m_Items;

        [SerializeField] List<Task> m_Tasks;

        public void AddItem(EntityItem entityItem)
        {
            if (m_Items.Contains(entityItem))
            {
                InworldAI.LogWarning($"Attempted to add an entity item that already exists: {entityItem.ID}");
                return;
            }
            
            m_Items.Add(entityItem);
            if(InworldController.Client)
                InworldController.Client.CreateOrUpdateItems(new List<Packet.EntityItem>() { entityItem.Get() }, entityItem.GetEntityIDs());
        }

        public void UpdateItem(EntityItem entityItem)
        {
            if (!m_Items.Contains(entityItem))
            {
                InworldAI.LogWarning($"Attempted to update an entity item that does not exist: {entityItem.ID}");
                return;
            }
            
            if(InworldController.Client)
                InworldController.Client.CreateOrUpdateItems(new List<Packet.EntityItem>() { entityItem.Get() }, entityItem.GetEntityIDs());
        }

        public void RemoveItem(EntityItem entityItem)
        {
            if (!m_Items.Contains(entityItem))
            {
                InworldAI.LogWarning($"Attempted to remove an entity item that does not exist: {entityItem.ID}");
                return;
            }

            m_Items.Remove(entityItem);
            if(InworldController.Client)
                InworldController.Client.DestroyItems(new List<string>() { entityItem.ID });
        }

        public bool FindTask(string taskName, out Task task)
        {
            return m_TaskDictionary.TryGetValue(taskName, out task);
        }

        public bool FindItem(string id, out EntityItem item)
        {
            item = m_Items.Find((entityItem => entityItem.ID == id));
            return item != null;
        }
        
        protected virtual void Awake()
        {
            m_Items = new List<EntityItem>();
            m_TaskDictionary = new Dictionary<string, Task>();
            
            foreach (Task task in m_Tasks)
                m_TaskDictionary.TryAdd(task.TaskShortName, task);
        }
    }
}

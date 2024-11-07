/*************************************************************************************************
 * Copyright 2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Inworld.BehaviorEngine
{
    [Serializable]
    public class TaskParameter
    {
        public string Name => m_Name;
        public string Description => m_Description;
        public ReadOnlyCollection<Entity> Entities => new ReadOnlyCollection<Entity>(m_Entities);

        [SerializeField]
        protected string m_Name;
        [SerializeField]
        protected string m_Description;
        [SerializeField]
        protected List<Entity> m_Entities;

        public TaskParameter(string name, string description, List<Entity> entities)
        {
            m_Name = name;
            m_Description = description;
            m_Entities = entities;
        }
    }
}
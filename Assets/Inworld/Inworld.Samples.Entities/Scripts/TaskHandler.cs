/*************************************************************************************************
 * Copyright 2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Inworld.BehaviorEngine
{
    public abstract class TaskHandler : MonoScript
    {
        public delegate void CompleteTask();
        public delegate void FailTask(string reason);
        public event CompleteTask onTaskComplete;
        public event FailTask onTaskFail;

        public abstract bool Validate(InworldCharacter inworldCharacter, Dictionary<string, string> parameters, out string message);
        public abstract IEnumerator Execute(InworldCharacter inworldCharacter, Dictionary<string, string> parameters);

        public void ClearEventListeners()
        {
            onTaskComplete = null;
            onTaskFail = null;
        }
    }
}

/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using UnityEngine.Scripting;

namespace Inworld.Entities
{
   
    [Serializable]
    class WebGLCommandData<T>
    {
        public WebGLCommand<T> command;
    }
    
    [Serializable]
    class WebGLCommand<T>
    {
        public string command;
        public T data;

        [Preserve] public WebGLCommand() {}

        [Preserve] public WebGLCommand(string command, T data)
        {
            this.command = command;
            this.data = data;
        }
    }
}

/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using UnityEngine;

namespace Inworld
{
    public class SingletonBehavior<T> : MonoBehaviour where T : Object
    {
        static T __inst;

        /// <summary>
        /// Gets the instance of this singleton.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (__inst)
                    return __inst;
                __inst = FindFirstObjectByType<T>(FindObjectsInactive.Include);
                return __inst;
            }
        }
    }
}

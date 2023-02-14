/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using UnityEngine;
namespace Inworld.Util
{
    /// <summary>
    ///     The data of Inworld Key/Secret.
    ///     Inworld Key/Secret is used to get SessionToken in Runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "New Key Secret Pair", menuName = "Inworld/Key Secret Pair", order = 6)]
    public class InworldKeySecret : ScriptableObject
    {
        public string key;
        public string secret;
        public string ShortName => string.IsNullOrEmpty(name) ? $"Key-{key}" : name;
    }
}

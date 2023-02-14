/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using System;
using System.Collections;
using UnityEngine;
namespace Inworld.Model
{
    /// <summary>
    ///     Dummy Avatar Loader.
    ///     If you don't want to have a 3D model,
    ///     Drag Dummy Avatar Loader Prefab into "Avatar Loader" of Inworld.AI
    /// </summary>
    public class DummyAvatarLoader : MonoBehaviour, IAvatarLoader
    {
        public void ConfigureModel(InworldCharacter character, GameObject model)
        {
            AvatarLoaded?.Invoke(character);
        }
        public IEnumerator Import(string url)
        {
            return null;
        }
        public GameObject LoadData(byte[] content)
        {
            return null;
        }
        public GameObject LoadData(string fileName)
        {
            return null;
        }
        public event Action<InworldCharacter> AvatarLoaded;
    }
}

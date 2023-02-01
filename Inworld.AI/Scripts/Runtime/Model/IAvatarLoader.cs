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
    ///     Interface of loading avatars. Implemented by different providers.
    ///     If you'd like to load your customized avatar loader,
    ///     please inherit this interface,
    ///     create a prefab with your own inherit avatar loader script attached,
    ///     then set it as InworldAI's "Avatar Loader".
    /// </summary>
    public interface IAvatarLoader
    {
        public void ConfigureModel(InworldCharacter character, GameObject model);
        public IEnumerator Import(string url);
        public GameObject LoadData(byte[] content);
        public GameObject LoadData(string fileName);
        public event Action<InworldCharacter> AvatarLoaded;
    }
}

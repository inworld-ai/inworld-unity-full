/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using System.Collections.Generic;
using UnityEngine;
namespace Inworld.Util
{
    /// <summary>
    ///     This class is used for Inworld Workspace Data scriptableObjects.
    ///     NOTE:
    ///     The file is saved locally with name of `Title`,
    ///     However, title is not unique,
    ///     Please make sure your data has `unique` to prevent data collision.
    /// </summary>
    [CreateAssetMenu(fileName = "New Workspace", menuName = "Inworld/Workspace", order = 3)]
    public class InworldWorkspaceData : ScriptableObject
    {
        /// <summary>
        ///     Check if this workspace has correct data.
        /// </summary>
        public bool IsValid => integrations.Count != 0 && scenes.Count != 0 && characters.Count != 0;

        /// <summary>
        ///     Get this workspace' default key (The first one if exists)
        /// </summary>
        public InworldKeySecret DefaultKey => integrations.Count > 0 ? integrations[0] : null;

        #region Public Variables
        public string title;
        public string fullName;
        public List<InworldCharacterData> characters;
        public List<InworldSceneData> scenes;
        public List<InworldKeySecret> integrations;
        public int index;
        #endregion
    }
}

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
    ///     InworldSceneData is the scriptableObject for InworldScene.
    ///     You could create locally, but mostly it's downloaded from server.
    ///     InworldScene is stored in InworldController, and contains inworld characters.
    ///     Usually, For each Unity Scene, you should prepare a InworldController that contains an InworldScene.
    /// </summary>
    [CreateAssetMenu(fileName = "New Scene", menuName = "Inworld/Scene", order = 2)]
    public class InworldSceneData : ScriptableObject
    {
        /// <summary>
        ///     Get the InworldScene's ShortName.
        ///     NOTE:
        ///     ShortName is also used as stored file name in your resources,
        ///     However, shortName is not unique retrieving from server.
        ///     Please make sure your data of `ShortName` is unique to prevent data collision.
        /// </summary>
        public string ShortName => !string.IsNullOrEmpty(shortName)
            ? shortName
            : !string.IsNullOrEmpty(fullName)
                ? fullName.Substring(fullName.LastIndexOf('/') + 1)
                : "";
        /// <summary>
        ///     Copy the data from other InworldScene with same fullName.
        /// </summary>
        /// <param name="rhs">The reference of rhs.</param>
        public void CopyFrom(InworldSceneData rhs)
        {
            if (fullName != rhs.fullName)
                return;
            shortName = rhs.shortName;
            description = rhs.description;
            characters = rhs.characters;
            triggers = rhs.triggers;
        }

        #region Inspector Variables
        public string fullName;
        public string shortName;
        public string description;
        public List<string> characters;
        public List<string> triggers;
        public int index;
        #endregion
    }
}

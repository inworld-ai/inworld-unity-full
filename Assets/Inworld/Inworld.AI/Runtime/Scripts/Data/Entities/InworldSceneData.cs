/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Inworld.Entities
{
    [Serializable]
    public class InworldSceneData
    {
        public string name; // Full name
        public string displayName;
        public string description;
        public string timePeriod;
        public List<SceneTrigger> sceneTriggers;
        public List<string> commonKnowledges;
        public List<CharacterReference> characterReferences;
        public SceneAssets defaultSceneAssets;
        public List<SceneCharacterReference> characters;

        /// <summary>
        /// Get the generated name for the scriptable object.
        /// </summary>
        [JsonIgnore]

        public string SceneFileName
        {
            get
            {
                string[] data = name.Split('/');
                return data.Length > 0 ? data[^1] : name;
            }
        }
        /// <summary>
        /// Returns true if all the characters are inside this scene.
        /// </summary>
        /// <param name="characterNames">the brainName of all the characters.</param>
        /// <returns></returns>
        public bool Contains(List<string> characterNames)
        {
            if (characterReferences.Count != 0)
                return characterNames.All(c => characterReferences.Any(cr => cr.character == c));
            if (characters.Count != 0)
                return characterNames.All(c => characters.Any(scr => scr.character == c));
            return false;
        }
        public List<InworldCharacterData> GetCharacterDataByReference(InworldWorkspaceData wsData)
        {
            List<InworldCharacterData> result = new List<InworldCharacterData>();
            if (wsData == null)
                return result;
            if (characterReferences != null && characterReferences.Count != 0)
            {
                result.AddRange(characterReferences.Select
                    (cr => wsData.characters.FirstOrDefault(c => c.brainName == cr.character))
                    .Where(charData => charData != null));
            }
            else if (characters != null && characters.Count != 0)
            {
                result.AddRange(characters.Select
                    (scr => wsData.characters.FirstOrDefault(c => c.brainName == scr.character))
                    .Where(characterData => characterData != null));
            }
            return result;
        }
    }

    [Serializable]
    public class ListSceneResponse
    {
        public List<InworldSceneData> scenes;
    }
    [Serializable]
    public class ListCharacterResponse
    {
        public List<CharacterOverLoad> characters;
    }
    [Serializable]
    public class SceneTrigger
    {
        public string trigger;
        public string description;
    }
    [Serializable]
    public class SceneCharacterReference
    {
        public string character;
        public string displayTitle;
        public string imageUri;
        public string additionalAgentInfo;
    }
    [Serializable]
    public class SceneAssets
    {
        public string sceneImg;
        public string sceneImgOriginal;
    }
}

/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Entities;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Inworld
{
    public class InworldGameData : ScriptableObject
    {
        public string sceneFullName;
        public string apiKey;
        public string apiSecret;
        public List<InworldCharacterData> characters;
        public Capabilities capabilities;
    
        /// <summary>
        /// Get the generated name for the scriptable object.
        /// </summary>
        public string SceneFileName
        {
            get
            {
                string[] data = sceneFullName.Split('/');
                return data.Length < 4 ? sceneFullName : $"{data[3]}_{data[1]}";
            }
        }
        public float Progress => characters?.Count > 0 ? characters.Sum(character => character.characterAssets.Progress) / characters.Count : 1;
    
        /// <summary>
        /// Set the data for the scriptable object instantiated.
        /// </summary>
        /// <param name="sceneData">The InworldSceneData to load</param>
        /// <param name="keySecret">The API key secret to use</param>
        public void SetData(InworldSceneData sceneData, InworldKeySecret keySecret)
        {
            if (sceneData != null)
            {
                sceneFullName = sceneData.name;
                if (characters == null)
                    characters = new List<InworldCharacterData>();
                characters.Clear();
                foreach (CharacterReference charRef in sceneData.characterReferences)
                {
                    characters.Add(new InworldCharacterData(charRef));
                }
            }
            if (keySecret != null)
            {
                apiKey = keySecret.key;
                apiSecret = keySecret.secret;
            }
            capabilities = new Capabilities(InworldAI.Capabilities);
    #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
    #endif
        }
    }
}


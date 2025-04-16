/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Entities;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;


namespace Inworld
{
    public class InworldGameData : ScriptableObject
    {
        public string workspaceName; 
        public string sceneName; 
        public string apiKey;
        public string apiSecret;
        public List<InworldCharacterData> characters;
        public Capabilities capabilities;

        public string WsFileName => workspaceName.Replace("workspaces/", "");

        /// <summary>
        /// Get the generated name for the scriptable object.
        /// </summary>
        public string SceneFileName
        {
            get
            {
                string[] data = sceneName.Split('/');
                return data.Length < 4 ? sceneName : data[3];
            }
        }
        /// <summary>
        /// Gets the progress of the assets downloading.
        /// </summary>
        public float Progress => characters?.Count > 0 ? characters.Sum(character => character.characterAssets.Progress) / characters.Count : 1;

        /// <summary>
        /// Set the data for the scriptable object instantiated.
        /// </summary>
        /// <param name="workspace">The workspace loading.</param>
        /// <param name="keySecret">The API key secret to use</param>
        public void Init(string workspace, InworldKeySecret keySecret)
        {
            if (!string.IsNullOrEmpty(workspace))
                workspaceName = workspace;
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

        /// <summary>
        /// Set Initial InworldSceneData
        /// </summary>
        /// <param name="sceneData"></param>
        public void SetInworldScene(InworldSceneData sceneData)
        {
            if (sceneData != null)
            {
                sceneName = sceneData.name;
                if (characters == null)
                    characters = new List<InworldCharacterData>();
                if (sceneData.characterReferences.Count > 0)
                {
                    characters.Clear();
                    foreach (CharacterReference charRef in sceneData.characterReferences)
                    {
                        characters.Add(new InworldCharacterData(charRef));
                    }
                }
            }
    #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
    #endif
        }
        
        /// <summary>
        /// Set InitialCharacterData
        /// </summary>
        /// <param name="characterNames"></param>
        public void SetInworldCharacter(List<InworldCharacterData> characterNames)
        {
            if (characterNames != null && characterNames.Count != 0)
            {
                if (characters == null)
                    characters = new List<InworldCharacterData>();
                characters.Clear();
                foreach (var charData in characterNames)
                {
                    characters.Add(charData);
                }
            }
    #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
    #endif
        }
    }
}


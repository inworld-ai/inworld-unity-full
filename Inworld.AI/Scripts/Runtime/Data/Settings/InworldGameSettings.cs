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
    ///     Its assets' name start as "GameSettings XXX".
    ///     Could be multiple copies. saved in "InworldAI"
    ///     Settings for runtime use. Might be hidden in inspector in future.
    /// </summary>
    public class InworldGameSettings : ScriptableObject
    {
        #region Inspector Variables
        [Space(10)]
        public InworldServerConfig currentServer;
        public InworldWorkspaceData currentWorkspace;
        public InworldSceneData currentScene;
        public InworldCharacterData currentCharacter;
        public InworldKeySecret currentKey;
        #endregion

        #region Properties
        /// <summary>
        ///     Get the server acceptable URL of runtime server.
        ///     Runtime server is the server you communicate with when enters play mode.
        ///     format would be https://xxx:port
        /// </summary>
        public string RuntimeServer => currentServer.RuntimeServer;
        /// <summary>
        ///     Get the server acceptable URL of studio server.
        ///     Studio server is the server for fetching all kinds of data,
        ///     include Runtime access token.
        ///     format would be https://xxx:port
        /// </summary>
        public string StudioServer => currentServer.StudioServer;
        /// <summary>
        ///     Get/Set the current Workspace.
        /// </summary>
        public InworldWorkspaceData CurrentWorkspace
        {
            get => currentWorkspace;
            set
            {
                currentWorkspace = value;
                currentScene = null;
                currentKey = null;
            }
        }
        /// <summary>
        ///     Get/Set the current KeySecret pair.
        /// </summary>
        public InworldKeySecret KeySecret => currentKey ? currentKey : currentWorkspace.DefaultKey;
        /// <summary>
        ///     Get/Set the current ApiKey of KeySecret pair.
        /// </summary>
        public string APIKey => KeySecret.key;
        /// <summary>
        ///     Get/Set the current ApiSecret of KeyScret pair.
        /// </summary>
        public string APISecret => KeySecret.secret;
        #endregion
    }
}

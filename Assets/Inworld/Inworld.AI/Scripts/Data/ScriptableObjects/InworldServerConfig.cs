/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using UnityEngine;

namespace Inworld
{
    public class InworldServerConfig : ScriptableObject
    {
        [Header("Server Info:")]
        public string runtime;
        public string web;
        public string tutorialPage; //TODO(Yan): Add reference in Editor panel.
        public int port;
        public string TokenServer => $"https://{web}/{k_TokenURL}"; 
        const string k_SessionURL = "v1/session/default?session_id=";
        const string k_TokenURL = "v1/sessionTokens/token:generate";
        /// <summary>
        /// Get the URL of runtime server.
        /// </summary>
        public string RuntimeServer => $"{runtime}:{port}";
        /// <summary>
        /// Get the URL for loadscene request.
        /// </summary>
        /// <param name="sceneFullName">the full name of the scene you want to load</param>
        public string LoadSceneURL(string sceneFullName) => $"https://{web}/v1/{sceneFullName}:load";
        /// <summary>
        /// Get the URL for the websocket session.
        /// </summary>
        /// <param name="sessionID">The current session ID obtained from the response of the `LoadSceneRequest`.</param>
        public string SessionURL(string sessionID) => $"wss://{web}/{k_SessionURL}{sessionID}";
        /// <summary>
        /// Get the URL for loading previous content 
        /// </summary>
        /// <param name="sessionFullName">
        ///     the full name of the scene you want to load。
        ///     Format should be workspaces/{workspaceName}/sessions/{sessionID}
        /// </param>
        public string LoadSessionURL(string sessionFullName) => $"https://{web}/v1/{sessionFullName}/state?name={sessionFullName}";
    }
}

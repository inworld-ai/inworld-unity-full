/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
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
        
        const string k_SessionURL = "v1/session/open?session_id="; 
        const string k_TokenURL = "v1/sessionTokens/token:generate";
        /// <summary>
        /// Get the URL of the token server.
        /// </summary>
        public string TokenServer => $"https://{web}/{k_TokenURL}"; 
        /// <summary>
        /// Get the URL of runtime server.
        /// </summary>
        public string RuntimeServer => $"{runtime}:{port}";
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
        public string LoadSessionStateURL(string sessionFullName) => $"https://{web}/v1/{sessionFullName}/state?name={sessionFullName}";
        /// <summary>
        /// Get the URL for the sending feedback request.
        /// </summary>
        /// <param name="callbackReference">the full name of the callback that is based on sessionID, interactionID, and correlationID.</param>
        /// <returns></returns>
        public string FeedbackURL(string callbackReference) => $"https://{web}/v1/feedback/{callbackReference}/feedbacks";
        /// <summary>
        /// Gets the URL for complete Chat.
        /// </summary>
        public string CompleteChatURL => $"https://{web}/llm/v1alpha/completions:completeChat";
        /// <summary>
        /// Gets the URL for complete Text.
        /// </summary>
        public string CompleteTextURL => $"https://{web}/llm/v1alpha/completions:completeText";
    }
}

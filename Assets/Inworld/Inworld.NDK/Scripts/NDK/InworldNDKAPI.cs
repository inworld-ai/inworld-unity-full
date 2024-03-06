/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Entities;
using UnityEngine;
using SystemInfo = UnityEngine.Device.SystemInfo;

namespace Inworld.NDK
{
    /// <summary>
    /// This static class contains all the static API functions to interact with DLL.
    /// </summary>
    public static class InworldNDKAPI
    {
        /// <summary>
        /// Get access token.
        /// </summary>
        /// <param name="serverURL">the server that generates token by api key and secret.</param>
        /// <param name="apiKey">the input api key</param>
        /// <param name="apiSecret">the input api secret</param>
        public static void GetAccessToken(string serverURL, string apiKey, string apiSecret)
        {
            NDKInterop.Unity_GetAccessToken(serverURL, apiKey, apiSecret, InworldNDKCallBack.OnTokenGenerated);
        }
        public static void GetHistoryAsync()
        {
            NDKInterop.Unity_SaveSessionState(InworldNDKCallBack.OnSessionStateReceived);
        }
        /// <summary>
        /// Set public workspace for the scope of access token
        /// </summary>
        /// <param name="publicWorkspace">the full string of public workspace</param>
        public static void SetPublicWorkspace(string publicWorkspace)
        {
            NDKInterop.Unity_SetPublicWorkspace(publicWorkspace);
        }
        /// <summary>
        /// Send load scene request to Inworld server via NDK.
        /// </summary>
        /// <param name="sceneFullName">the full name of the Inworld scene to load.</param>
        /// <param name="continuation">the session continuation to load.</param>;
        public static void LoadScene(string sceneFullName, Continuation continuation)
        {
            Inworld.Entities.Capabilities capabilities = InworldAI.Capabilities;
            Capabilities cap = new Capabilities
            {
                Text  = capabilities.text,
                Audio = capabilities.audio,
                Emotions = capabilities.emotions,
                Interruptions  = capabilities.interruptions,
                Triggers  = capabilities.triggers,
                PhonemeInfo = capabilities.text,
                NarratedActions = capabilities.narratedActions,
                Relations = capabilities.relations
            };
            NDKInterop.Unity_SetCapabilities(ref cap);
            if (string.IsNullOrEmpty(InworldAI.User.ID))
                InworldAI.User.ID = SystemInfo.deviceUniqueIdentifier;
            NDKInterop.Unity_SetUserRequest(InworldAI.User.Name, InworldAI.User.ID);
            foreach (PlayerProfileField profile in InworldAI.User.PlayerProfiles)
            {
                NDKInterop.Unity_AddUserProfile(profile.fieldId, profile.fieldValue);
            }
            NDKInterop.Unity_SetClientRequest(InworldAI.UnitySDK.id, InworldAI.UnitySDK.version, InworldAI.UnitySDK.description);
            string history = continuation.externallySavedState;
            history = string.IsNullOrEmpty(history) ? "" : history;
            NDKInterop.Unity_LoadScene(sceneFullName, history, InworldNDKCallBack.OnSceneLoaded);
        }
        /// <summary>
        /// Init the NDK, establish all the callback.
        /// Would return a string of log from NDK to check if success.
        /// </summary>
        public static void Init()
        {
            InworldAI.Log("[NDK] Start Init");
            NDKInterop.Unity_InitWrapper();
            InworldAI.Log("[NDK] Start Set Logger");
            NDKInterop.Unity_SetLogger(InworldNDKCallBack.OnLogReceived);
            InworldAI.Log("[NDK] Start Set CallBack");
            NDKInterop.Unity_SetPacketCallback(InworldNDKCallBack.OnNDKPacketReceived,
                                               InworldNDKCallBack.OnPhonemeReceived, 
                                               InworldNDKCallBack.OnTriggerParamReceived);
        }
    }
}

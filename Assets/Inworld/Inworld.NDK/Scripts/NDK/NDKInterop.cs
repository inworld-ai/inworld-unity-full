/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;
using System.Runtime.InteropServices;


namespace Inworld.NDK
{
    public class NDKInterop
    {
        const string DLL_NAME = "InworldUnityWrapper";

#region Life Cycle
        /// <summary>
        /// Init NDK's Unity wrapper, will generate a global pointer of the NDK's instance in dll.
        /// Later, all the NDK's api need to send through that wrapper.
        /// </summary>
        /// <returns></returns>
        [DllImport(DLL_NAME)]
        public static extern IntPtr Unity_InitWrapper();
        
        /// <summary>
        /// Destroy the NDK's Unity wrapper. Please call when terminate app to prevent mem leak.
        /// </summary>
        [DllImport(DLL_NAME)]
        public static extern void Unity_DestroyWrapper();
#endregion

#region Inworld Status
        
        /// <summary>
        /// Get access token via NDK
        /// </summary>
        /// <param name="serverURL">the url of the server that dispatch the access token.</param>
        /// <param name="apiKey">the input api key</param>
        /// <param name="apiSecret">the input api secret</param>
        /// <param name="callback">the callback method that triggers once finished</param>
        [DllImport(DLL_NAME)]
        public static extern void Unity_GetAccessToken(string serverURL, string apiKey, string apiSecret, NDKCallback callback);

        /// <summary>
        /// Save the game. If successful, it'll send callback.
        /// Then you can use GetSessionInfo to get the previous state.
        /// </summary>
        /// <param name="callback">the callback which will be triggered if it's successful</param>
        [DllImport(DLL_NAME)]
        public static extern void Unity_SaveSessionState(NDKCallback callback);
        /// <summary>
        /// Send load scene request to Inworld server via NDK
        /// </summary>
        /// <param name="sceneName">the full name of the Inworld scene to send.</param>
        /// <param name="sessionState">the session state to load scene. </param>
        /// <param name="callback">the callback method that triggers once finished</param>
        [DllImport(DLL_NAME)]
        public static extern void Unity_LoadScene(string sceneName, string sessionState, NDKCallback callback);
        
        /// <summary>
        /// Start live session through NDK.
        /// </summary>
        [DllImport(DLL_NAME)]
        public static extern void Unity_StartSession();
        
        /// <summary>
        /// Terminate live session through NDK.
        /// </summary>
        [DllImport(DLL_NAME)]
        public static extern void Unity_EndSession();
#endregion
        
#region Send Data
        
        /// <summary>
        /// Send text message to character via NDK.
        /// </summary>
        /// <param name="agentID">the live session ID of the Inworld character.</param>
        /// <param name="message">the message to send.</param>
        [DllImport(DLL_NAME)]
        public static extern void Unity_SendText(string agentID, string message);
        
        /// <summary>
        /// Send audio wave data to character via NDK.
        /// </summary>
        /// <param name="agentID">the live session ID of the Inworld character.</param>
        /// <param name="data">the wave data to send.</param>
        [DllImport(DLL_NAME)]
        public static extern void Unity_SendAudio(string agentID, string data);
        
        /// <summary>
        /// Send the trigger to the character via NDK.
        /// In NDK, sending trigger with parameter cannot process at the same time.
        /// Should call those API one by one.
        /// </summary>
        /// <param name="agentID">the live session ID of the Inworld character.</param>
        /// <param name="triggerName">the trigger to send.</param>
        [DllImport(DLL_NAME)]
        public static extern void Unity_SendTrigger(string agentID, string triggerName);
        
        /// <summary>
        /// Send the parameter of the trigger. If a trigger contains multiple parameters and values.
        /// Should call this API one by one.
        /// </summary>
        /// <param name="agentID">the live session ID of the Inworld character.</param>
        /// <param name="triggerName">the name of the trigger to send.</param>
        /// <param name="param">one of the trigger's parameter name</param>
        /// <param name="paramValue">one of the trigger's parameter value.</param>
        [DllImport(DLL_NAME)]
        public static extern void Unity_SendTriggerParam(string agentID, string triggerName, string param, string paramValue);
        
        /// <summary>
        /// Send cancel response packet to Inworld character.
        /// </summary>
        /// <param name="agentID">the live session ID of the Inworld character.</param>
        /// <param name="interactionIDToCancel">the interaction id to cancel.</param>
        [DllImport(DLL_NAME)]
        public static extern void Unity_CancelResponse(string agentID, string interactionIDToCancel);
        
        /// <summary>
        /// Send AUDIO_SESSION_START control event to Inworld server to let the character accept audio.
        /// </summary>
        /// <param name="agentID">the live session ID of the Inworld character.</param>
        [DllImport(DLL_NAME)]
        public static extern void Unity_StartAudio(string agentID);
        
        /// <summary>
        /// Send AUDIO_SESSION_END control event to Inworld server to let the character stop accepting audio.
        /// </summary>
        /// <param name="agentID">the live session ID of the Inworld character.</param>
        [DllImport(DLL_NAME)]
        public static extern void Unity_StopAudio(string agentID);
#endregion Send Data

#region Setter
        /// <summary>
        /// Set the local static callback to receive logs from NDK.
        /// </summary>
        /// <param name="callback">the callback method to receive. need to be static function.</param>
        [DllImport(DLL_NAME)]
        public static extern void Unity_SetLogger(NDKLogCallBack callback);
        
        /// <summary>
        /// Set the local static callback for receiving NDK packets.
        /// In the NDK, phonemes and trigger parameters are sent separately.
        /// </summary>
        /// <param name="pktCallBack">the callback method to receive NDk packets.</param>
        /// <param name="phonemeCallBack">the call back method to receive phoneme info. </param>
        /// <param name="triggerParamCallBack">the call back method to receive parameters and their values for the specific trigger.</param>
        [DllImport(DLL_NAME)]
        public static extern void Unity_SetPacketCallback(
            NDKPacketCallBack pktCallBack,
            PhonemeCallBack phonemeCallBack,
            TriggerParamCallBack triggerParamCallBack);
        
        [DllImport(DLL_NAME)]
        public static extern void Unity_SetPublicWorkspace(string strPublicWorkspace);
        
        /// <summary>
        /// Send to the Inworld server with the client's info.
        /// </summary>
        /// <param name="strClientID">the client info, in this case should be Unity or UnityNDK</param>
        /// <param name="strClientVersion">the Inworld Unity SDK version.</param>
        [DllImport(DLL_NAME)]
        public static extern void Unity_SetClientRequest(string strClientID, string strClientVersion, string strClientDescription);
        
        /// <summary>
        /// Send to the Inworld server with the user's info.
        /// </summary>
        /// <param name="strUserName">the user name to send.</param>
        /// <param name="strPlayerID">the user ID to send.</param>
        [DllImport(DLL_NAME)]
        public static extern void Unity_SetUserRequest(string strUserName, string  strPlayerID);
        
        /// <summary>
        /// Send to Inworld server by adding the current user's one pair of player profile
        /// </summary>
        /// <param name="strProfileID">the name of the player profile.</param>
        /// <param name="strProfileValue">the value of the player profile.</param>
        [DllImport(DLL_NAME)]
        public static extern void Unity_AddUserProfile(string strProfileID, string strProfileValue);
        
        
        /// <summary>
        /// Send the capability info to Inworld server.
        /// </summary>
        /// <param name="capabilities">the capability to send.</param>
        [DllImport(DLL_NAME)]
        public static extern void Unity_SetCapabilities(ref Capabilities capabilities);
#endregion

#region Getter
        /// <summary>
        /// Get the pointer of AgentInfo by the index in the list.
        /// Will throw exception if the agent list is null or exceed the length.
        ///
        /// The AgentInfo in NDK is equivalent to InworldCharacterData in Unity.
        /// </summary>
        /// <param name="nIndex">the index of the character in the list.</param>
        [DllImport(DLL_NAME)]
        public static extern IntPtr Unity_GetAgentInfo(int nIndex);
        
        /// <summary>
        /// Get the count of the agent list in NDK.
        /// </summary>
        [DllImport(DLL_NAME)]
        public static extern int Unity_GetAgentCount();
        
        /// <summary>
        /// Get the info of the current session. Including session ID, session token, etc.
        /// Return a pointer of session info. Need to be marshalled as InworldNDKData::SessionInfo
        /// </summary>
        [DllImport(DLL_NAME)]
        public static extern IntPtr Unity_GetSessionInfo();        
#endregion        
    }
}

/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Packet;
using Inworld.Entities;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;


namespace Inworld.NDK
{
    public class InworldNDKClient : InworldClient
    {
        ConcurrentQueue<InworldPacket> m_IncomingQueue = new ConcurrentQueue<InworldPacket>();

        InworldConnectionStatus m_LastStatus, m_CurrentStatus;
        
        /// <summary>
        /// Gets the Inworld character list.
        /// </summary>
        public List<AgentInfo> AgentList { get; } = new List<AgentInfo>();
        /// <summary>
        /// Gets the current client status.
        /// </summary>
        public override InworldConnectionStatus Status
        {
            get => m_CurrentStatus;
            set
            {
                m_LastStatus = m_CurrentStatus;
                m_CurrentStatus = value;
            }
        }
        /// <summary>
        /// Disconnect with Inworld server via NDK.
        /// </summary>
        public override void Disconnect()
        {
            NDKInterop.Unity_EndSession();
        }
        /// <summary>
        /// Get the access token. The returned token data is through call back. 
        /// </summary>
        public override void GetAccessToken()
        {
            Status = InworldConnectionStatus.Initializing;
            string responseJson = m_CustomToken;
            if (!string.IsNullOrEmpty(responseJson))
                return;
            if (string.IsNullOrEmpty(m_APIKey))
            {
                Error = "Please fill API Key!";
                return;
            }
            if (string.IsNullOrEmpty(m_APISecret))
            {
                Error = "Please fill API Secret!";
                return;
            }
            if (!string.IsNullOrEmpty(m_PublicWorkspace))
                InworldNDKAPI.SetPublicWorkspace(m_PublicWorkspace);
            InworldNDKAPI.GetAccessToken(m_ServerConfig.RuntimeServer, m_APIKey, m_APISecret);
        }
       
        /// <summary>
        /// Send load scene request to Inworld server.
        /// </summary>
        /// <param name="sceneFullName">the full name of the Inworld scene to load.</param>
        /// <param name="history">the full string of the encrypted history content to send.</param>
        public override void LoadScene(string sceneFullName, string history = "") => InworldNDKAPI.LoadScene(sceneFullName); //TODO(Yan): Update NDK Dll.

        /// <summary>
        /// Gets the load scene response when load scene success is sent through call back.
        /// </summary>
        public override LoadSceneResponse GetLiveSessionInfo() => InworldNDK.From.NDKLoadSceneResponse(AgentList);
        
        /// <summary>
        /// Starts the session. Should wait several frame to let NDK fully start.
        /// </summary>
        public override void StartSession() => StartCoroutine(_StartSession());

        /// <summary>
        /// Send text to Inworld server. Will immediately generate a local packet.
        /// </summary>
        /// <param name="characterID">the live session ID of the character to send.</param>
        /// <param name="textToSend">the message to send.</param>
        public override void SendText(string characterID, string textToSend)
        {
            if (string.IsNullOrEmpty(characterID) || string.IsNullOrEmpty(textToSend))
                return;
            Dispatch(InworldNDK.To.TextPacket(characterID, textToSend));
            NDKInterop.Unity_SendText(characterID, textToSend);
        }
        
        /// <summary>
        /// Send the cancel response event to interrupt character.
        /// </summary>
        /// <param name="characterID">the live session ID of the character to send.</param>
        /// <param name="interactionID">the ID of the incoming message from the character to cancel.</param>
        public override void SendCancelEvent(string characterID, string interactionID)
        {
            if (string.IsNullOrEmpty(characterID))
                return;
            NDKInterop.Unity_CancelResponse(characterID, interactionID);
        }
        
        /// <summary>
        /// Send the trigger to the character.
        /// </summary>
        /// <param name="charID">the live session ID of the character to send.</param>
        /// <param name="triggerName">the name of the trigger to send.</param>
        /// <param name="parameters">the parameters and values of the trigger to send.</param>
        public override void SendTrigger(string charID, string triggerName, Dictionary<string, string> parameters)
        {
            if (string.IsNullOrEmpty(charID))
                return;
            if (parameters == null || parameters.Count == 0)
                NDKInterop.Unity_SendTrigger(charID, triggerName);
            else
            {
                foreach (KeyValuePair<string, string> data in parameters)
                {
                    NDKInterop.Unity_SendTriggerParam(charID, triggerName, data.Key, data.Value);
                }
            }
        }
        
        /// <summary>
        /// Send AUDIO_SESSION_START control event to let the character enable receiving packets.
        /// </summary>
        /// <param name="charID">the ID of the character to send.</param>
        public override void StartAudio(string charID)
        {
            if (string.IsNullOrEmpty(charID))
                return;
            NDKInterop.Unity_StartAudio(charID);
        }
        /// <summary>
        /// Send AUDIO_SESSION_END control event to let the character disable receiving packets.
        /// </summary>
        /// <param name="charID">the ID of the character to send.</param>
        public override void StopAudio(string charID)
        {
            if (string.IsNullOrEmpty(charID))
                return;
            NDKInterop.Unity_StopAudio(charID);
        }
        
        /// <summary>
        /// Send base64 string of wavedata to the character.
        /// The original wave data before transcoding to base64 should be short array (Int16)
        /// </summary>
        /// <param name="charID">the ID of the character to send.</param>
        /// <param name="base64">the wave data to send.</param>
        public override void SendAudio(string charID, string base64)
        {
            if (string.IsNullOrEmpty(charID) || string.IsNullOrEmpty(base64))
                return;
            NDKInterop.Unity_SendAudio(charID, base64);
        }
        
        /// <summary>
        /// Put the received but not processed packet in queue.
        /// </summary>
        /// <param name="packet">the target packet to enqueue.</param>
        public void Enqueue(InworldPacket packet)
        {
            m_IncomingQueue.Enqueue(packet);
        }
        protected virtual void OnDisable() => NDKInterop.Unity_EndSession();
        
        void Update()
        {
            _ProcessStatusChange();
            _ProcessPackage();
        }
        
        protected virtual void OnDestroy() => NDKInterop.Unity_DestroyWrapper();
        
        protected override void Init()
        {
            InworldNDKAPI.Init();
        }
        protected IEnumerator _StartSession()
        {
            if (!IsTokenValid)
                yield break;
            yield return new WaitForEndOfFrame();
            Status = InworldConnectionStatus.Connecting;
            yield return new WaitForEndOfFrame();
            NDKInterop.Unity_StartSession();
            yield return new WaitForEndOfFrame();
            Status = InworldConnectionStatus.Connected;
            yield return new WaitForEndOfFrame();
        }
        void _ProcessStatusChange()
        {
            if (m_CurrentStatus == m_LastStatus)
                return;
            ChangeStatus(m_CurrentStatus);
            m_LastStatus = m_CurrentStatus;
        }
        void _ProcessPackage()
        {
            if (m_IncomingQueue.Count == 0)
                return;
            if (m_IncomingQueue.TryDequeue(out InworldPacket packet))
                Dispatch(packet);
        }
    }
}

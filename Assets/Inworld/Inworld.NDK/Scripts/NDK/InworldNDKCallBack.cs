/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using AOT;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Inworld.NDK
{
    /// <summary>
    /// This static class stores all the callback function.
    /// Because we're receiving data from dll, they have to be static as well.
    /// </summary>
    public static class InworldNDKCallBack
    {
        static ProcessingAudioChunk s_ProcessingAudioChunk = new ProcessingAudioChunk();
        
        [MonoPInvokeCallback(typeof(NDKCallback))]
        static internal void OnTokenGenerated()
        {
            SessionInfo sessionInfo = Marshal.PtrToStructure<SessionInfo>(NDKInterop.Unity_GetSessionInfo());
            InworldController.Client.Token = InworldNDK.From.NDKToken(sessionInfo);
            InworldAI.Log("Get Session ID: " + sessionInfo.sessionId);
            InworldAI.Log("[NDK] Token Generated");
            InworldController.Client.Status = InworldConnectionStatus.Initialized;
        }
        [MonoPInvokeCallback(typeof(NDKCallback))]
        static internal void OnSessionStateReceived()
        {
            SessionInfo sessionInfo = Marshal.PtrToStructure<SessionInfo>(NDKInterop.Unity_GetSessionInfo());
            InworldController.Client.SessionHistory = sessionInfo.sessionSavedState;
            InworldAI.Log($"[NDK] Session State {InworldController.Client.SessionHistory}");
        }
        [MonoPInvokeCallback(typeof(NDKCallback))]
        static internal void OnSceneLoaded()
        {
            InworldAI.Log("[NDK] Scene loaded");
            if (InworldController.Client is not InworldNDKClient ndkClient)
                return;
            int nAgentSize = NDKInterop.Unity_GetAgentCount();
            ndkClient.AgentList.Clear();
            for (int i = 0; i < nAgentSize; i++)
            {
                ndkClient.AgentList.Add(Marshal.PtrToStructure<AgentInfo>(NDKInterop.Unity_GetAgentInfo(i)));
            }
            // InworldController.Client.Status = InworldConnectionStatus.LoadingSceneCompleted;
        }
        [MonoPInvokeCallback(typeof(NDKLogCallBack))]
        static internal void OnLogReceived(string log, int severity)
        {
            switch (severity)
            {
                case 1:
                    InworldAI.LogWarning($"[NDK]: {log}");
                    break;
                case 2:
                    InworldAI.LogError($"[NDK]: {log}");
                    // if (log.Contains("inactivity"))
                    //     InworldController.Client.Status = InworldConnectionStatus.LostConnect;
                    break;
                case 3:
                    InworldAI.LogException($"[NDK]: {log}");
                    break;
                default:
                    InworldAI.Log($"[NDK]: {log}");
                    break;
            }
        }
        [MonoPInvokeCallback(typeof(NDKPacketCallBack))]
        static internal void OnNDKPacketReceived(NDKPacket packet)
        {
            if (InworldController.Client is not InworldNDKClient ndkClient)
                return;
            switch (packet.packetType)
            {
                case "TEXT":
                    ndkClient.Enqueue(InworldNDK.From.NDKTextPacket(packet));
                    break;
                case "AUDIO":
                    s_ProcessingAudioChunk.ToInworldPacket();
                    s_ProcessingAudioChunk = new ProcessingAudioChunk(packet);
                    break;
                case "CONTROL":
                    ndkClient.Enqueue(InworldNDK.From.NDKControlPacket(packet));
                    break;
                case "EMOTION":
                    ndkClient.Enqueue(InworldNDK.From.NDKEmotionPacket(packet));
                    break;
                case "CANCEL":
                    ndkClient.Enqueue(InworldNDK.From.NDKCancelResponse(packet));
                    break;
                case "CUSTOM":
                    ndkClient.Enqueue(InworldNDK.From.NDKCustomPacket(packet));
                    break;
                case "RELATION":
                    ndkClient.Enqueue(InworldNDK.From.NDKRelationPacket(packet));
                    break;
                case "ACTION":
                    ndkClient.Enqueue(InworldNDK.From.NDKActionPacket(packet));
                    break;
                default:
                    ndkClient.Enqueue(InworldNDK.From.NDKUnknownPacket(packet));
                    break;
            }
        }
        [MonoPInvokeCallback(typeof(PhonemeCallBack))]
        static internal void OnPhonemeReceived(PhonemeInfo packet) => s_ProcessingAudioChunk.ReceivePhoneme(packet);

        [MonoPInvokeCallback(typeof(TriggerParamCallBack))]
        static internal void OnTriggerParamReceived(TriggerParam packet)
        {
            // Currently server doesn't support send trigger callback with param. 
        }
    }
}

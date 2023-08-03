/*
 * /*************************************************************************************************
 * * Copyright 2022 Theai, Inc. (DBA Inworld)
 * *
 * * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 * ************************************************************************************************
 */

using System;
using System.Runtime.InteropServices;


namespace Inworld.NDK
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ConnectionStateCallbackType(ConnectionState state);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void PacketCallbackType(IntPtr packetWrapper, int size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LoadSceneCallbackType(IntPtr serializedAgentInfoArray, int serializedAgentInfoArraySize);

    public class InworldNDKBridge : IDisposable
    {
        public IntPtr instance;

        public InworldNDKBridge(ConnectionStateCallbackType connectionStateCallback, PacketCallbackType packetCallback)
        {
            instance = ClientWrapper_create();

            if (instance == null)
            {
                InworldAI.LogError("Failed to create a wrapper from the DLL");
            }
            else
            {
                ClientWrapper_InitClient
                (
                    instance,
                    "DefaultUserNDK",
                    "DefaultClientNDK",
                    "1.0.0", connectionStateCallback, packetCallback
                );
            }
        }

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ClientWrapper_create")]
        public static extern IntPtr ClientWrapper_create();

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ClientWrapper_destroy")]
        public static extern void ClientWrapper_destroy(IntPtr wrapper);

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ClientWrapper_SendPacket")]
        public static extern void ClientWrapper_SendPacket(IntPtr wrapper, IntPtr packetWrapper);

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ClientWrapper_SendTextMessage")]
        public static extern void ClientWrapper_SendTextMessage(IntPtr wrapper, string agentId, string text);

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ClientWrapper_SendSoundMessage")]
        public static extern void ClientWrapper_SendSoundMessage(IntPtr wrapper, string agentId, byte[] data, int dataSize);

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ClientWrapper_SendSoundMessageWithAEC")]
        public static extern void ClientWrapper_SendSoundMessageWithAEC(IntPtr wrapper, string agentId, IntPtr inputData, int inputDataSize, IntPtr outputData, int outputDataSize);

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ClientWrapper_SendCustomEvent")]
        public static extern void ClientWrapper_SendCustomEvent(IntPtr wrapper, string agentId, string name);

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ClientWrapper_CancelResponse")]
        public static extern void ClientWrapper_CancelResponse(IntPtr wrapper, string agentId, string interactionId, IntPtr utteranceIds, int utteranceIdsCount);

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ClientWrapper_StartAudioSession")]
        public static extern void ClientWrapper_StartAudioSession(IntPtr wrapper, string agentId);

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ClientWrapper_StopAudioSession")]
        public static extern void ClientWrapper_StopAudioSession(IntPtr wrapper, string agentId);

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ClientWrapper_InitClient")]
        public static extern void ClientWrapper_InitClient(IntPtr wrapper, string userId, string clientId, string clientVer, ConnectionStateCallbackType connectionStateCallback, PacketCallbackType packetCallback);

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ClientWrapper_StartClientWithCallback")]
        public static extern void ClientWrapper_StartClientWithCallback(IntPtr wrapper, byte[] serializedOptions, int serializedOptionsSize, byte[] serializedSessionInfo, int sessionInfoSize, LoadSceneCallbackType loadSceneCallback);

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ClientWrapper_StartClient")]
        public static extern void ClientWrapper_StartClient(IntPtr wrapper, byte[] serializedOptions, int serializedOptionsSize);

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ClientWrapper_PauseClient")]
        public static extern void ClientWrapper_PauseClient(IntPtr wrapper);

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ClientWrapper_ResumeClient")]
        public static extern void ClientWrapper_ResumeClient(IntPtr wrapper);

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ClientWrapper_StopClient")]
        public static extern void ClientWrapper_StopClient(IntPtr wrapper);

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ClientWrapper_DestroyClient")]
        public static extern void ClientWrapper_DestroyClient(IntPtr wrapper);

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetConnectionState")]
        public static extern ConnectionState GetConnectionState(IntPtr wrapper);

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ClientWrapper_GetConnectionError")]
        public static extern bool ClientWrapper_GetConnectionError(IntPtr wrapper, IntPtr outErrorMessage, ref int outErrorCode);

        [DllImport("InworldNDK", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ClientWrapper_Update")]
        public static extern void ClientWrapper_Update(IntPtr wrapper);

        public void Dispose()
        {
            if (instance == null)
                return;
            
            InworldAI.Log("DISPOSING OF THE WRAPPER");
            ClientWrapper_destroy(instance);
            instance = IntPtr.Zero;
        }
    }
}
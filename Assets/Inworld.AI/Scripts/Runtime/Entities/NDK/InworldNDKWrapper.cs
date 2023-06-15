/*
 * /*************************************************************************************************
 * * Copyright 2022 Theai, Inc. (DBA Inworld)
 * *
 * * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 * ************************************************************************************************
 */

﻿using System;
using UnityEngine;
using System.Runtime.InteropServices;
using Inworld.NdkData;

// C-compatible structure in C#
[StructLayout(LayoutKind.Sequential)]
public struct CAgentInfoArray {
    public int agent_info_list_count;
    public IntPtr agent_info_list;
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void ConnectionStateCallbackType(ConnectionState state);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void PacketCallbackType(IntPtr packetWrapper, int size);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void LoadSceneCallbackType(IntPtr serializedAgentInfoArray, int serializedAgentInfoArraySize);

public class InworldNDKWrapper : IDisposable
{
    public IntPtr instance;

    public InworldNDKWrapper(ConnectionStateCallbackType connectionStateCallback, PacketCallbackType packetCallback)
    {
        instance = ClientWrapper_create();

        if (instance == IntPtr.Zero)
        {
            Debug.LogError("Failed to create a wrapper from the DLL");
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
            Debug.Log("Should have created a wrapper from the dll wrapper is not null");
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
        Debug.Log("DISPOSING OF THE WRAPPER");
        if (instance != IntPtr.Zero)
        {
            ClientWrapper_destroy(instance);
            instance = IntPtr.Zero;
        }
    }


    // Conversion function to convert the protobuf-generated AgentInfoArray to the C-compatible CAgentInfoArray
    public static CAgentInfoArray ToCAgentInfoArray(AgentInfoArray agentInfoArray) {
        CAgentInfoArray cAgentInfoArray = new CAgentInfoArray {
            agent_info_list_count = agentInfoArray.AgentInfoList.Count
        };

        // Allocate memory for the agent_info_list
        cAgentInfoArray.agent_info_list = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(AgentInfo)) * cAgentInfoArray.agent_info_list_count);

        // Fill the agent_info_list with AgentInfo objects
        for (int i = 0; i < cAgentInfoArray.agent_info_list_count; i++) {
            Marshal.StructureToPtr(agentInfoArray.AgentInfoList[i], IntPtr.Add(cAgentInfoArray.agent_info_list, i * Marshal.SizeOf(typeof(AgentInfo))), false);
        }

        return cAgentInfoArray;
    }
    
    // Conversion function to convert the C-compatible CAgentInfoArray back to the protobuf-generated AgentInfoArray
    public static AgentInfoArray FromCAgentInfoArray(CAgentInfoArray cAgentInfoArray) {
        AgentInfoArray agentInfoArray = new AgentInfoArray();

        for (int i = 0; i < cAgentInfoArray.agent_info_list_count; i++) {
            IntPtr agentInfoPtr = IntPtr.Add(cAgentInfoArray.agent_info_list, i * Marshal.SizeOf(typeof(AgentInfo)));
            AgentInfo agentInfo = Marshal.PtrToStructure<AgentInfo>(agentInfoPtr);
            agentInfoArray.AgentInfoList.Add(agentInfo);
        }

        return agentInfoArray;
    }
}

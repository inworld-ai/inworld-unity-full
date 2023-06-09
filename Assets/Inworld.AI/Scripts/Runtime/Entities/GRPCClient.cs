/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Inworld.Grpc;
using Inworld.Packets;
using Inworld.Runtime;
using Inworld.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using AudioChunk = Inworld.Packets.AudioChunk;
using ControlEvent = Inworld.Grpc.ControlEvent;
using CustomEvent = Inworld.Packets.CustomEvent;
using EmotionEvent = Inworld.Packets.EmotionEvent;
using GrpcPacket = Inworld.Grpc.InworldPacket;
using InworldPacket = Inworld.Packets.InworldPacket;
using Routing = Inworld.Packets.Routing;
using TextEvent = Inworld.Packets.TextEvent;
using Inworld;
using UnityEditor.PackageManager;
using UnityEditor.Sprites;
using Inworld.Studio;

#if  !INWORLD_NDK

public class GRPCClient : IInworldClient
{
    InworldClient m_Client;
    #region Private Variables

    AsyncDuplexStreamingCall<GrpcPacket, GrpcPacket> m_StreamingCall;
    InworldAuth m_InworldAuth;
    string m_SessionKey = "";
    Metadata m_Header;
    private WorldEngine.WorldEngineClient m_WorldEngineClient;
    private Channel m_Channel;

    public GRPCClient()
    {
        
    }

    public bool IsAuthenticated => !m_InworldAuth.IsExpired;

    public string SessionID => m_InworldAuth?.SessionID ?? "";//throw new NotImplementedException();

    public bool IsSessionInitialized => m_SessionKey.Length != 0;

    #endregion

    public void Initialize(InworldClient _client)
    {
        m_Client = _client;
        m_Channel = new Channel(InworldAI.Game.RuntimeServer, new SslCredentials());
        m_WorldEngineClient = new WorldEngine.WorldEngineClient(m_Channel);
    }
    public void Authenticate(string sessionToken)
    {
#if UNITY_EDITOR && VSP
            if (!string.IsNullOrEmpty(InworldAI.User.Account))
                VSAttribution.VSAttribution.SendAttributionEvent("Login Runtime", InworldAI.k_CompanyName, InworldAI.User.Account);
#endif
        m_InworldAuth = new InworldAuth();
        if (string.IsNullOrEmpty(sessionToken))
        {
            GenerateTokenRequest gtRequest = new GenerateTokenRequest
            {
                Key = InworldAI.Game.APIKey,
                Resources =
                {
                    InworldAI.Game.currentWorkspace.fullName
                }
                    
            };
            Metadata metadata = new Metadata
            {
                {
                    "authorization", m_InworldAuth.GetHeader(InworldAI.Game.RuntimeServer, InworldAI.Game.APIKey, InworldAI.Game.APISecret)
                }
            };
            try
            {
                m_InworldAuth.Token = m_WorldEngineClient.GenerateToken(gtRequest, metadata, DateTime.UtcNow.AddHours(1));
                InworldAI.Log("Init Success!");
                m_Header = new Metadata
                {
                    {"authorization", $"Bearer {m_InworldAuth.Token.Token}"},
                    {"session-id", m_InworldAuth.Token.SessionId}
                };
                m_Client.InvokeRuntimeEvent(RuntimeStatus.InitSuccess, "");
            }
            catch (RpcException e)
            {
                m_Client.InvokeRuntimeEvent(RuntimeStatus.InitFailed, e.ToString());
            }
        }
        else
        {
            _ReceiveCustomToken(sessionToken);
        }
    }

    void _ReceiveCustomToken(string sessionToken)
    {
        JObject data = JObject.Parse(sessionToken);
        if (data.ContainsKey("sessionId") && data.ContainsKey("token"))
        {
            InworldAI.Log("Init Success with Custom Token!");
            m_Header = new Metadata
                {
                    {"authorization", $"Bearer {data["token"]}"},
                    {"session-id", data["sessionId"]?.ToString()}
                };
            m_Client.InvokeRuntimeEvent(RuntimeStatus.InitSuccess, "");
        }
        else
            m_Client.InvokeRuntimeEvent(RuntimeStatus.InitFailed, "Token Invalid");
    }

    public void EndAudio(Routing routing)
    {
        if (m_Client.SessionStarted)
            m_Client.m_CurrentConnection?.outgoingEventsQueue.Enqueue
            (
                new GrpcPacket
                {
                    Timestamp = m_Client.Now,
                    Routing = routing.ToGrpc(),
                    Control = new ControlEvent
                    {
                        Action = ControlEvent.Types.Action.AudioSessionEnd
                    }
                }
            );
    }

    public async Task EndSession()
    {
        if (m_Client.SessionStarted)
        {
            m_Client.m_CurrentConnection = null;
            m_Client.SessionStarted = false;
            await m_StreamingCall.RequestStream.CompleteAsync();
            m_StreamingCall.Dispose();
        }
    }

    public void ResolvePackets(GrpcPacket packet)
    {
        m_Client.m_CurrentConnection ??= new Connection();
            if (packet.DataChunk != null)
            {
                switch (packet.DataChunk.Type)
                {
                    case DataChunk.Types.DataType.Audio:
                        m_Client.m_CurrentConnection.incomingAudioQueue.Enqueue(new AudioChunk(packet));
                        break;
                    case DataChunk.Types.DataType.State:
                        StateChunk stateChunk = new StateChunk(packet);
                        m_Client.LastState = stateChunk.Chunk.ToBase64();
                        break;
                    default:
                        InworldAI.LogError($"Unsupported incoming event: {packet}");
                        break;
                }
            }
            else if (packet.Text != null)
            {
                m_Client.m_CurrentConnection.incomingInteractionsQueue.Enqueue(new TextEvent(packet));
            }
            else if (packet.Control != null)
            {
                m_Client.m_CurrentConnection.incomingInteractionsQueue.Enqueue(new Inworld.Packets.ControlEvent(packet));
            }
            else if (packet.Emotion != null)
            {
                m_Client.m_CurrentConnection.incomingInteractionsQueue.Enqueue(new EmotionEvent(packet));
            }
            else if (packet.Action != null)
            {
                m_Client.m_CurrentConnection.incomingInteractionsQueue.Enqueue(new Inworld.Packets.ActionEvent(packet));
            }
            else if (packet.Custom != null)
            {
               m_Client.m_CurrentConnection.incomingInteractionsQueue.Enqueue(new CustomEvent(packet));
            }
            else
            {
                InworldAI.LogError($"Unsupported incoming event: {packet}");
            }
    }

    public void SendAudio(AudioChunk audioChunk)
    {
        if (m_Client.SessionStarted)
            m_Client.m_CurrentConnection?.outgoingEventsQueue.Enqueue(audioChunk.ToGrpc());
    }

    public void SendEvent(InworldPacket packet)
    {
        if (m_Client.SessionStarted)
            m_Client.m_CurrentConnection?.outgoingEventsQueue.Enqueue(packet.ToGrpc());
    }

    public void StartAudio(Routing routing)
    {
        InworldAI.Log("Start Audio Event");
        if (m_Client.SessionStarted)
            m_Client.m_CurrentConnection?.outgoingEventsQueue.Enqueue
            (
                new GrpcPacket
                {
                    Timestamp = m_Client.Now,
                    Routing = routing.ToGrpc(),
                    Control = new ControlEvent
                    {
                        Action = ControlEvent.Types.Action.AudioSessionStart
                    }
                }
            );
    }

    public async Task StartSession()
    {
        if (!IsSessionInitialized)
        {
            throw new ArgumentException("No sessionKey to start Inworld session, use CreateWorld first.");
        }
        // New queue for new session.
        Connection connection = new Connection();
        m_Client.m_CurrentConnection = connection;

        m_Client.SessionStarted = true;
        try
        {
            using (m_StreamingCall = m_WorldEngineClient.Session(m_Header))
            {
                // https://grpc.github.io/grpc/csharp/api/Grpc.Core.IAsyncStreamReader-1.html
                Task inputTask = Task.Run
                (
                    async () =>
                    {
                        while (m_Client.SessionStarted)
                        {
                            bool next;
                            try
                            {
                                // Waiting response for some time before checking if done.
                                next = await m_StreamingCall.ResponseStream.MoveNext();
                            }
                            catch (RpcException rpcException)
                            {
                                if (rpcException.StatusCode == Grpc.Core.StatusCode.Cancelled)
                                {
                                    next = false;
                                }
                                else
                                {
                                    // rethrowing other errors.
                                    throw;
                                }
                            }
                            if (next)
                            {
                                ResolvePackets(m_StreamingCall.ResponseStream.Current);
                            }
                            else
                            {
                                InworldAI.Log("Session is closed.");
                                break;
                            }
                        }
                    }
                );
                Task outputTask = Task.Run
                (
                    async () =>
                    {
                        while (m_Client.SessionStarted)
                        {
                            Task.Delay(100).Wait();
                            // Sending all outgoing events.
                            GrpcPacket e;
                            while (connection.outgoingEventsQueue.TryDequeue(out e))
                            {
                                if (m_Client.SessionStarted)
                                {
                                    await m_StreamingCall.RequestStream.WriteAsync(e);
                                }
                            }
                        }
                    }
                );
                await Task.WhenAll(inputTask, outputTask);
            }
        }
        catch (Exception e)
        {
            m_Client.SessionStarted = false;
            m_Client.Errors.Enqueue(e);
        }
        finally
        {
            m_Client.SessionStarted = false;
        }
    }

    public void OnAuthComplete()
    {
        m_Header = new Metadata
            {
                {"authorization", $"Bearer {m_InworldAuth.Token}"},
                {"session-id", m_InworldAuth.SessionID}
            };
    }

    public void OnAuthFailed(string message)
    {
    }

    public async Task<LoadSceneResponse> LoadScene(string sceneName)
    {
        LoadSceneRequest lsRequest = new LoadSceneRequest
        {
            Name = sceneName,
            Capabilities = InworldAI.Settings.Capabilities,
            User = InworldAI.User.Request,
            Client = InworldAI.User.Client,
            UserSettings = InworldAI.User.Settings
        };
        if (!string.IsNullOrEmpty(m_Client.LastState))
        {
            lsRequest.SessionContinuation = new SessionContinuation
            {
                PreviousState = ByteString.FromBase64(m_Client.LastState)
            };
        }
        try
        {
            LoadSceneResponse response = await m_WorldEngineClient.LoadSceneAsync(lsRequest, m_Header);
            // Yan: They somehow use {WorkSpace}:{sessionKey} as "sessionKey" now. Need to remove the first part.
            m_SessionKey = response.Key.Split(':')[1];
            if (response.PreviousState != null)
            {
                foreach (PreviousState.Types.StateHolder stateHolder in response.PreviousState.StateHolders)
                {
                    InworldAI.Log($"Received Previous Packets: {stateHolder.Packets.Count}");
                }
            }
            m_Header.Add("Authorization", $"Bearer {m_SessionKey}");
            m_Client.InvokeRuntimeEvent(RuntimeStatus.LoadSceneComplete, m_SessionKey);
            return response;
        }
        catch (RpcException e)
        {
            m_Client.InvokeRuntimeEvent(RuntimeStatus.LoadSceneFailed, e.ToString());
            return null;
        }
    }

    public void Destroy()
    {
#pragma warning restore CS4014
        m_Channel.ShutdownAsync();
    }

    public void Update()
    {
    }
}
#endif


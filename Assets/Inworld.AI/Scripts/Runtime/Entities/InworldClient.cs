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
using System.Threading.Tasks;
using UnityEngine;
using AudioChunk = Inworld.Packets.AudioChunk;
using ActionEvent = Inworld.Packets.ActionEvent;
using ControlEvent = Inworld.Grpc.ControlEvent;
using CustomEvent = Inworld.Packets.CustomEvent;
using EmotionEvent = Inworld.Packets.EmotionEvent;
using GrpcPacket = Inworld.Grpc.InworldPacket;
using InworldPacket = Inworld.Packets.InworldPacket;
using Routing = Inworld.Packets.Routing;
using TextEvent = Inworld.Packets.TextEvent;


namespace Inworld
{
    /// <summary>
    ///     This class used to save the communication data in runtime.
    /// </summary>
    class Connection
    {
        // Audio chunks ready to play.
        internal readonly ConcurrentQueue<AudioChunk> incomingAudioQueue = new ConcurrentQueue<AudioChunk>();
        // Events that need to be processed by NPC.
        internal readonly ConcurrentQueue<InworldPacket> incomingInteractionsQueue = new ConcurrentQueue<InworldPacket>();
        // Events ready to send to server.
        internal readonly ConcurrentQueue<GrpcPacket> outgoingEventsQueue = new ConcurrentQueue<GrpcPacket>();
    }
    /// <summary>
    ///     This is the logic class for Server communication.
    /// </summary>
    class InworldClient
    {
        internal InworldClient()
        {
            m_Channel = new Channel(InworldAI.Game.RuntimeServer, new SslCredentials());
            m_WorldEngineClient = new WorldEngine.WorldEngineClient(m_Channel);
        }

        #region Private Variables
        readonly WorldEngine.WorldEngineClient m_WorldEngineClient;
        readonly Channel m_Channel;
        AsyncDuplexStreamingCall<GrpcPacket, GrpcPacket> m_StreamingCall;
        Connection m_CurrentConnection;
        InworldAuth m_InworldAuth;
        string m_SessionKey = "";
        Metadata m_Header;
        internal event Action<RuntimeStatus, string> RuntimeEvent;
        #endregion

        #region Properties
        internal ConcurrentQueue<Exception> Errors { get; } = new ConcurrentQueue<Exception>();
        internal bool SessionStarted { get; private set; }
        internal bool HasInit => !m_InworldAuth.IsExpired;
        internal string SessionID => m_InworldAuth?.Token.SessionId ?? "";
        internal string LastState { get; set; }
        bool IsSessionInitialized => m_SessionKey.Length != 0;
        Timestamp Now => Timestamp.FromDateTime(DateTime.UtcNow);
        #endregion

        #region Private Functions
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
                RuntimeEvent?.Invoke(RuntimeStatus.InitSuccess, "");
            }
            else
                RuntimeEvent?.Invoke(RuntimeStatus.InitFailed, "Token Invalid");
        }
        internal void GetAppAuth(string key, string secret, string sessionToken = "")
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(InworldAI.User.Account))
                VSAttribution.VSAttribution.SendAttributionEvent("Login Runtime", InworldAI.k_CompanyName, InworldAI.User.Account);
#endif
            m_InworldAuth = new InworldAuth();
            if (string.IsNullOrEmpty(sessionToken))
            {
                GenerateTokenRequest gtRequest = new GenerateTokenRequest
                {
                    Key = key,
                    Resources =
                    {
                        InworldAI.Game.currentWorkspace.fullName
                    }
                    
                };
                Metadata metadata = new Metadata
                {
                    {
                        "authorization", m_InworldAuth.GetHeader(InworldAI.Game.RuntimeServer, key, secret)
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
                    RuntimeEvent?.Invoke(RuntimeStatus.InitSuccess, "");
                }
                catch (RpcException e)
                {
                    RuntimeEvent?.Invoke(RuntimeStatus.InitFailed, e.ToString());
                }
            }
            else
            {
                _ReceiveCustomToken(sessionToken);
            }
        }

        internal async Task<LoadSceneResponse> LoadScene(string sceneName)
        {

            LoadSceneRequest lsRequest = new LoadSceneRequest
            {
                Name = sceneName,
                Capabilities = InworldAI.Settings.Capabilities,
                User = InworldAI.User.Request,
                Client = InworldAI.User.Client,
                UserSettings = InworldAI.User.Settings
            };
            if (!string.IsNullOrEmpty(LastState))
            {
                lsRequest.SessionContinuation = new SessionContinuation
                {
                    PreviousState = ByteString.FromBase64(LastState)
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
                RuntimeEvent?.Invoke(RuntimeStatus.LoadSceneComplete, m_SessionKey);
                return response;
            }
            catch (RpcException e)
            {
                RuntimeEvent?.Invoke(RuntimeStatus.LoadSceneFailed, e.ToString());
                return null;
            }
        }
        // Marks audio session start.
        internal void StartAudio(Routing routing)
        {
            InworldAI.Log("Start Audio Event");
            if (SessionStarted)
                m_CurrentConnection?.outgoingEventsQueue.Enqueue
                (
                    new GrpcPacket
                    {
                        Timestamp = Now,
                        Routing = routing.ToGrpc(),
                        Control = new ControlEvent
                        {
                            Action = ControlEvent.Types.Action.AudioSessionStart
                        }
                    }
                );
        }

        // Marks session end.
        internal void EndAudio(Routing routing)
        {
            if (SessionStarted)
                m_CurrentConnection?.outgoingEventsQueue.Enqueue
                (
                    new GrpcPacket
                    {
                        Timestamp = Now,
                        Routing = routing.ToGrpc(),
                        Control = new ControlEvent
                        {
                            Action = ControlEvent.Types.Action.AudioSessionEnd
                        }
                    }
                );
        }

        // Sends audio chunk to server.
        internal void SendAudio(AudioChunk audioEvent)
        {
            if (SessionStarted)
                m_CurrentConnection?.outgoingEventsQueue.Enqueue(audioEvent.ToGrpc());
        }
        internal bool GetAudioChunk(out AudioChunk chunk)
        {
            if (m_CurrentConnection != null)
            {
                return m_CurrentConnection.incomingAudioQueue.TryDequeue(out chunk);
            }
            chunk = null;
            return false;
        }
        internal void SendEvent(InworldPacket e)
        {
            if (SessionStarted)
                m_CurrentConnection?.outgoingEventsQueue.Enqueue(e.ToGrpc());
        }
        internal bool GetIncomingEvent(out InworldPacket incomingEvent)
        {
            if (m_CurrentConnection != null)
            {
                return m_CurrentConnection.incomingInteractionsQueue.TryDequeue(out incomingEvent);
            }
            incomingEvent = null;
            return false;
        }
        internal async Task StartSession()
        {
            if (!IsSessionInitialized)
            {
                throw new ArgumentException("No sessionKey to start Inworld session, use CreateWorld first.");
            }
            // New queue for new session.
            Connection connection = new Connection();
            m_CurrentConnection = connection;

            SessionStarted = true;
            try
            {
                using (m_StreamingCall = m_WorldEngineClient.Session(m_Header))
                {
                    // https://grpc.github.io/grpc/csharp/api/Grpc.Core.IAsyncStreamReader-1.html
                    Task inputTask = Task.Run
                    (
                        async () =>
                        {
                            while (SessionStarted)
                            {
                                bool next;
                                try
                                {
                                    // Waiting response for some time before checking if done.
                                    next = await m_StreamingCall.ResponseStream.MoveNext();
                                }
                                catch (RpcException rpcException)
                                {
                                    if (rpcException.StatusCode == StatusCode.Cancelled)
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
                                    _ResolveGRPCPackets(m_StreamingCall.ResponseStream.Current);
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
                            while (SessionStarted)
                            {
                                Task.Delay(100).Wait();
                                // Sending all outgoing events.
                                GrpcPacket e;
                                while (connection.outgoingEventsQueue.TryDequeue(out e))
                                {
                                    if (SessionStarted)
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
                SessionStarted = false;
                Errors.Enqueue(e);
            }
            finally
            {
                SessionStarted = false;
            }
        }
        internal TextEvent ResolvePreviousPackets(GrpcPacket response) => response.Text != null ? new TextEvent(response) : null;

        void _ResolveGRPCPackets(GrpcPacket response)
        {
            m_CurrentConnection ??= new Connection();
            if (response.DataChunk != null)
            {
                switch (response.DataChunk.Type)
                {
                    case DataChunk.Types.DataType.Audio:
                        m_CurrentConnection.incomingAudioQueue.Enqueue(new AudioChunk(response));
                        break;
                    case DataChunk.Types.DataType.State:
                        StateChunk stateChunk = new StateChunk(response);
                        LastState = stateChunk.Chunk.ToBase64();
                        break;
                    default:
                        InworldAI.LogError($"Unsupported incoming event: {response}");
                        break;
                }
            }
            else if (response.Text != null)
            {
                m_CurrentConnection.incomingInteractionsQueue.Enqueue(new TextEvent(response));
            }
            else if (response.Control != null)
            {
                m_CurrentConnection.incomingInteractionsQueue.Enqueue(new Packets.ControlEvent(response));
            }
            else if (response.Emotion != null)
            {
                m_CurrentConnection.incomingInteractionsQueue.Enqueue(new EmotionEvent(response));
            }
            else if (response.Action != null)
            {
                m_CurrentConnection.incomingInteractionsQueue.Enqueue(new ActionEvent(response));
            }
            else if (response.Custom != null)
            {
                m_CurrentConnection.incomingInteractionsQueue.Enqueue(new CustomEvent(response));
            }
            else if (response.DebugInfo != null && response.DebugInfo.InfoCase == DebugInfoEvent.InfoOneofCase.Relation)
            {
                m_CurrentConnection.incomingInteractionsQueue.Enqueue(new RelationEvent(response));
            }
            else
            {
                InworldAI.LogError($"Unsupported incoming event: {response}");
            }
        }

        internal async Task EndSession()
        {
            if (SessionStarted)
            {
                m_CurrentConnection = null;
                SessionStarted = false;
                await m_StreamingCall.RequestStream.CompleteAsync();
                m_StreamingCall.Dispose();
            }
        }
        internal void Destroy()
        {
#pragma warning disable CS4014
            EndSession();
#pragma warning restore CS4014
            m_Channel.ShutdownAsync();
        }
        #endregion
    }
}

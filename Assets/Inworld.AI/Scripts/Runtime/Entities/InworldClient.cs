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
using CustomEvent = Inworld.Packets.CustomEvent;
using EmotionEvent = Inworld.Packets.EmotionEvent;
#if INWORLD_NDK
using GrpcPacket = Inworld.ProtoBuf.InworldPacket;
#else
using GrpcPacket = Inworld.Grpc.InworldPacket;
using ControlEvent = Inworld.Grpc.ControlEvent;
#endif
using InworldPacket = Inworld.Packets.InworldPacket;
using Routing = Inworld.Packets.Routing;
using TextEvent = Inworld.Packets.TextEvent;


namespace Inworld
{
    /// <summary>
    ///     This class used to save the communication data in runtime.
    /// </summary>
    public class Connection
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
    public class InworldClient
    {
        internal InworldClient()
        {
#if INWORLD_NDK
            core = new InworldNDKClient();
#else
            core = new GRPCClient();
#endif
            core.Initialize(this);
        }
        
        public Connection m_CurrentConnection;
        public event Action<RuntimeStatus, string> RuntimeEvent;
        internal IInworldClient core;

        #region Properties
        internal ConcurrentQueue<Exception> Errors { get; } = new ConcurrentQueue<Exception>();
        internal bool SessionStarted { get; set; }
        internal bool HasInit => core.IsAuthenticated;
        internal string SessionID => core.SessionID;
        internal string LastState { get; set; }
        bool IsSessionInitialized => core.IsSessionInitialized;
        public Timestamp Now => Timestamp.FromDateTime(DateTime.UtcNow);
        #endregion
        
        public void GetAppAuth(string sessionToken)
        {
            core.Authenticate(sessionToken);
        }
        
        #region Call backs
        public void OnAuthCompleted()
        {
            InworldAI.Log("Init Success!");
            core.OnAuthComplete();
            RuntimeEvent?.Invoke(RuntimeStatus.InitSuccess, "");
        }

        public void OnAuthFailed(string msg)
        {
            core.OnAuthFailed(msg);
            RuntimeEvent?.Invoke(RuntimeStatus.InitFailed, msg);
        }
        #endregion
        
        #region Private Functions
        public void InvokeRuntimeEvent(RuntimeStatus status, string msg)
        {
            RuntimeEvent?.Invoke(status, msg);
        }
        internal async Task<LoadSceneResponse> LoadScene(string sceneName)
        {
            var task = core.LoadScene(sceneName);
            await task;
            return task.Result;
        }
        // Marks audio session start.
       internal void StartAudio(Routing routing)
        {
            core.StartAudio(routing);
        }

        // Marks session end.
        internal void EndAudio(Routing routing)
        {
            InworldAI.Log("should end audio target is " + routing.Target.Id);
            core.EndAudio(routing);
        }

        // Sends audio chunk to server.
        internal void SendAudio(AudioChunk audioEvent)
        {
            core.SendAudio(audioEvent);
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
            core.SendEvent(e);
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
            var task = core.StartSession();
            await task;
        }
        internal TextEvent ResolvePreviousPackets(GrpcPacket response) => response.Text != null ? InworldPacketGenerator.Instance.FromProtobufPacket<TextEvent>(response) : null;

        public void Update()
        {
            core.Update();
        }

        public void ResolvePackets(GrpcPacket packet)
        {
            core.ResolvePackets(packet);
            // m_CurrentConnection ??= new Connection();
            // if (packet.DataChunk != null)
            // {
            //     switch (packet.DataChunk.Type)
            //     {
            //         case DataChunk.Types.DataType.Audio:
            //             m_CurrentConnection.incomingAudioQueue.Enqueue(new AudioChunk(packet));
            //             break;
            //         case DataChunk.Types.DataType.Animation:
            //             m_CurrentConnection.incomingAnimationQueue.Enqueue(new AnimationChunk(packet));
            //             break;
            //         case DataChunk.Types.DataType.State:
            //             StateChunk stateChunk = new StateChunk(packet);
            //             LastState = stateChunk.Chunk.ToBase64();
            //             break;
            //         default:
            //             InworldAI.LogError($"Unsupported incoming event: {packet}");
            //             break;
            //     }
            // }
            // else if (packet.Text != null)
            // {
            //     m_CurrentConnection.incomingInteractionsQueue.Enqueue(new TextEvent(packet));
            // }            
            // else if (packet.Control != null)
            // {
            //     m_CurrentConnection.incomingInteractionsQueue.Enqueue(new Packets.ControlEvent(packet));
            // }
            // else if (packet.Emotion != null)
            // {
            //     m_CurrentConnection.incomingInteractionsQueue.Enqueue(new EmotionEvent(packet));
            // }
            // else if (packet.Custom != null)
            // {
            //     m_CurrentConnection.incomingInteractionsQueue.Enqueue(new CustomEvent(packet));
            // }
            // else
            // {
            //     InworldAI.LogError($"Unsupported incoming event: {packet}");
            // }
        }

        internal async Task EndSession()
        {
            var task = core.EndSession();
            await task;
        }
        internal void Destroy()
        {
#pragma warning disable CS4014
            EndSession();
            core.Destroy();
        }
        #endregion
    }
}

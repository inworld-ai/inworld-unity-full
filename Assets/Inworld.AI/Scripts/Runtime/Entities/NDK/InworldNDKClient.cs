using Inworld.NdkData;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static UnityEditor.Progress;
using Inworld.Grpc;
using Inworld;
using Inworld.Util;
using System.Threading.Tasks;
using UnityEditor.MemoryProfiler;
using Inworld.Packets;
using AudioChunk = Inworld.Packets.AudioChunk;
using ControlEvent = Inworld.ProtoBuf.ControlEvent;
using CustomEvent = Inworld.Packets.CustomEvent;
using EmotionEvent = Inworld.Packets.EmotionEvent;
using GrpcPacket = Inworld.ProtoBuf.InworldPacket;
using InworldPacket = Inworld.Packets.InworldPacket;
using Routing = Inworld.Packets.Routing;
using TextEvent = Inworld.Packets.TextEvent;
using DataChunk = Inworld.ProtoBuf.DataChunk;
using static Google.Rpc.Context.AttributeContext.Types;
using System.Text;

#if INWORLD_NDK
public class InworldNDKClient : IInworldClient
{
    public bool useAEC = false;
    InworldClient m_Client;
    InworldNDKWrapper m_Wrapper;
    ConnectionStateCallbackType m_ConnectionCallback;
    PacketCallbackType m_PacketCallback;
    LoadSceneCallbackType callback;
    private TaskCompletionSource<bool> agentInfosFilled;

    #region Wrapper variables
    ClientOptions m_Options = new ClientOptions();
    AgentInfoArray agentInfoArray = new AgentInfoArray();
    #endregion

    public event Action<InworldPacket> OnPacketReceived;

    public void Initialize(InworldClient _client)
    { 
        m_Client = _client;
        m_ConnectionCallback = new ConnectionStateCallbackType(ConnectionStateCallback);
        m_PacketCallback = new PacketCallbackType(PacketCallback);
        m_Wrapper = new InworldNDKWrapper(m_ConnectionCallback, m_PacketCallback);// (ConnectionStateCallback, PacketCallback);//
    }

    public void Authenticate(string sessionToken)
    {
#if UNITY_EDITOR && VSP
if (!string.IsNullOrEmpty(InworldAI.User.Account))
VSAttribution.VSAttribution.SendAttributionEvent("Login Runtime", InworldAI.k_CompanyName, InworldAI.User.Account);
#endif
        m_Options.AuthUrl = InworldAI.Game.currentServer.studio;//"api-studio.inworld.ai";//
        m_Options.LoadSceneUrl = InworldAI.Game.currentServer.RuntimeServer;//"api-engine.inworld.ai:443";//
        m_Options.SceneName = InworldAI.Game.currentScene.fullName;//"workspaces/artem_v_test/scenes/demo";//
        m_Options.PlayerName = InworldAI.User.Name;//"Player"; //
        m_Options.ApiKey = InworldAI.Game.APIKey;//"xgV17Aur4k33Qoox8OxepATdBeS0Bamu";//
        m_Options.ApiSecret = InworldAI.Game.APISecret;// "zSpQ2d0wObaPWCiRS2oioh8nDPVf1zkg2OISrdx8Zs47bm8rpdki3s6nejza0aki";//
        Debug.Log("GetAppAuth with wrapper options");
        callback = new LoadSceneCallbackType(LoadSceneCallback);
        byte[] serializedData = m_Options.ToByteArray();

        InworldNDKWrapper.ClientWrapper_StartClientWithCallback(m_Wrapper.instance, serializedData,
            serializedData.Length, callback);
    }

    //Callback will be invoked through the wrapper NDK side 
    //this callback will likely have to replace the OnPacketEvents functionality
    public void PacketCallback(IntPtr packetwrapper, int packetSize)
    {
        // Create a byte array to store the data
        byte[] data = new byte[packetSize];
        // Copy the data from the IntPtr to the byte array
        Marshal.Copy(packetwrapper, data, 0, packetSize);

        // Deserialize the byte array to an InworldPacket instance using protobuf
        GrpcPacket response = GrpcPacket.Parser.ParseFrom(data);
        ResolvePackets(response);
    }

public void ResolvePackets(GrpcPacket packet)
{
    Debug.Log("RESOLVING A NDK PACKET IN UNITY");
    m_Client.m_CurrentConnection ??= new Inworld.Connection();
    InworldPacketGenerator factory = InworldPacketGenerator.Instance;
    object protobufObject;

    if (packet.DataChunk != null)
    {
        Debug.Log("packet chunk isn't null type is " + packet.DataChunk.Type);
        switch (packet.DataChunk.Type)
        {
            case DataChunk.Types.DataType.Audio:
                protobufObject = factory.Create(packet, "AudioChunk");
                // Perform operations on protobufObject here...
                m_Client.m_CurrentConnection.incomingAudioQueue.Enqueue((AudioChunk)protobufObject);
                break;
            case DataChunk.Types.DataType.Animation:
                protobufObject = factory.Create(packet, "AnimationChunk");
                // Perform operations on protobufObject here...
                m_Client.m_CurrentConnection.incomingAnimationQueue.Enqueue((AnimationChunk)protobufObject);
                break;
            case DataChunk.Types.DataType.State:
                protobufObject = factory.Create(packet, "StateChunk");
                // Perform operations on protobufObject here...
                m_Client.LastState = ((StateChunk)protobufObject).Chunk.ToBase64();
                break;
            default:
                InworldAI.LogError($"Unsupported incoming CHUNK event: {packet}");
                break;
        }
    }
    else if (packet.Text != null)
    {
        Debug.Log("received a text packet from the NDK " + packet.Text);
        protobufObject = factory.Create(packet, "TextEvent");
        // Perform operations on protobufObject here...
        m_Client.m_CurrentConnection.incomingInteractionsQueue.Enqueue((TextEvent)protobufObject);
    }
    else if (packet.AudioChunk != null)
    {
        Debug.Log("received a AUDIO packet from the NDK " + packet.AudioChunk);
        // Rest of the debug logs and processing

        bool phonemesValid = false;
        if (packet.DataChunk == null)
        {
            Debug.Log("packet.DataChunk is null");
        }
        else
        {
            Debug.Log("packet.DataChunk.Chunk null status is " + packet.DataChunk.Chunk == null);
            phonemesValid = packet.DataChunk.AdditionalPhonemeInfo != null;
        }

        if (phonemesValid)
        {
            protobufObject = factory.Create(packet, "AudioChunk");
        }
        else
        {
            protobufObject = new AudioChunk(packet.AudioChunk.Chunk, Inworld.Packets.Routing.FromAgentToPlayer(packet.Routing.Source.Name));
        }
        // Perform operations on protobufObject here...
        m_Client.m_CurrentConnection.incomingAudioQueue.Enqueue((AudioChunk)protobufObject);
    }
    else if (packet.Control != null)
    {
        protobufObject = factory.Create(packet, "ControlEvent");
        // Perform operations on protobufObject here...
        m_Client.m_CurrentConnection.incomingInteractionsQueue.Enqueue((Inworld.Packets.ControlEvent)protobufObject);
    }
    else if (packet.Emotion != null)
    {
        protobufObject = factory.Create(packet, "EmotionEvent");
        // Perform operations on protobufObject here...
        m_Client.m_CurrentConnection.incomingInteractionsQueue.Enqueue((EmotionEvent)protobufObject);
    }
    else if (packet.Custom != null)
    {
        protobufObject = factory.Create(packet, "CustomEvent");
        // Perform operations on protobufObject here...
        m_Client.m_CurrentConnection.incomingInteractionsQueue.Enqueue((CustomEvent)protobufObject);
    }
    else
    {
        InworldAI.LogError($"Unsupported incoming other kind of event: {packet}");
    }
}

    void ConnectionStateCallback(ConnectionState state)
    {
        Debug.Log("CONNECTION STATE IS " + state);
        switch (state)
        {
            case ConnectionState.Connected:
                m_Client.OnAuthCompleted();
                break;
            case ConnectionState.Disconnected:
                break;
            case ConnectionState.Failed:
                m_Client.OnAuthFailed("Connection state returned failed from NDK");
                break;
        }

    }

    //Callback should be invoked through the wrapper in the NDK
    public void LoadSceneCallback(IntPtr serializedAgentInfoArray, int serializedAgentInfoArraySize)
    {
        byte[] serializedData = new byte[serializedAgentInfoArraySize];
        Marshal.Copy(serializedAgentInfoArray, serializedData, 0, serializedAgentInfoArraySize);
        agentInfoArray = AgentInfoArray.Parser.ParseFrom(serializedData);

        Debug.Log("LOADSCENE CALLBACK INVOKED FROM DLL ARRAY COUNT IS " + agentInfoArray.AgentInfoList.Count);

        foreach (AgentInfo agentInfo in agentInfoArray.AgentInfoList)
        {
            Debug.Log("first agent info name is " + agentInfo.GivenName + " brain is " + agentInfo.BrainName + " " + agentInfo.AgentId);
        }
        // Now you can use agentInfoArray in your C# code

        // Free the allocated buffer
        //Marshal.FreeHGlobal(serializedAgentInfoArray);
        if(agentInfosFilled != null)
            agentInfosFilled.TrySetResult(true);

        Marshal.FreeCoTaskMem(serializedAgentInfoArray);
    }

    #region Call backs
    public void OnAuthComplete()
    {
        Debug.Log("Init Success!");
       
    }
    public void OnAuthFailed(string msg)
    {
        Debug.Log("Auth failed with meassage: ! " + msg);

    }
    #endregion

    float lastPacketSendTime = 0f;
    public void Update()
    {
        if (m_Client.SessionStarted)
        {
            if (Time.time - lastPacketSendTime > 0.1f)
            {
                lastPacketSendTime = Time.time;

                GrpcPacket e;
                while (m_Client.m_CurrentConnection.outgoingEventsQueue.TryDequeue(out e))
                {
                    SendRawEvent(e);
                }

            }
        }
        InworldNDKWrapper.ClientWrapper_Update(m_Wrapper.instance);
    }


    public bool IsAuthenticated { get; set; }

    public string SessionID { get; set; }

    public bool IsSessionInitialized { get; set; }

    ///// <summary>
    ///// Section for testing
    ///// </summary>
    //AgentInfo currentAgent;
    //private string inputMessage = "";

    //private void OnGUI()
    //{
    //    if (agentInfoArray != null)
    //    {
    //        float buttonWidth = 200;
    //        float buttonHeight = 40;
    //        float buttonSpacing = 10;
    //        float currentYPosition = 10;

    //        foreach (AgentInfo agentInfo in agentInfoArray.AgentInfoList)
    //        {
    //            string buttonText = $"{agentInfo.GivenName}";
    //            if (GUI.Button(new Rect(10, currentYPosition, buttonWidth, buttonHeight), buttonText))
    //            {
    //                Debug.Log("Button clicked: " + buttonText);
    //                // Add any additional code you want to execute when the button is clicked
    //                currentAgent = agentInfo;
    //            }

    //            currentYPosition += buttonHeight + buttonSpacing;
    //        }

    //        if (currentAgent != null)
    //        {
    //            float screenWidth = Screen.width;
    //            float screenHeight = Screen.height;
    //            float centerX = (screenWidth - buttonWidth) / 2;
    //            float centerY = (screenHeight - buttonHeight) / 2;

    //            GUI.Label(new Rect(centerX, centerY - buttonHeight - buttonSpacing, buttonWidth, buttonHeight), "Current agent: " + currentAgent.GivenName);

    //            inputMessage = GUI.TextField(new Rect(centerX, centerY, buttonWidth, buttonHeight), inputMessage);

    //            centerY += buttonHeight + buttonSpacing;

    //            if (GUI.Button(new Rect(centerX, centerY, buttonWidth, buttonHeight), "Send Message"))
    //            {
    //                Debug.Log("Sending message to agent: " + currentAgent.GivenName);
    //                InworldNDKWrapper.ClientWrapper_SendTextMessage(m_Wrapper.instance, currentAgent.AgentId, inputMessage);
    //            }
    //        }
    //    }
    //}

    public async Task<LoadSceneResponse> LoadScene(string sceneName)
    {
        if(agentInfoArray.AgentInfoList.Count == 0)
        {
            agentInfosFilled = new TaskCompletionSource<bool>();
            await agentInfosFilled.Task;
        }

        LoadSceneResponse response = new LoadSceneResponse();
        foreach(AgentInfo agentInfo in agentInfoArray.AgentInfoList)
        {
            LoadSceneResponse.Types.Agent agent = new LoadSceneResponse.Types.Agent();
            agent.GivenName = agentInfo.GivenName;
            agent.BrainName = agentInfo.BrainName;
            agent.AgentId = agentInfo.AgentId;
            response.Agents.Add(agent);
        }

        m_Client.InvokeRuntimeEvent(RuntimeStatus.LoadSceneComplete, "");
        return response;
    }

    public Task StartSession()
    {
        Inworld.Connection connection = new Inworld.Connection();
        m_Client.m_CurrentConnection = connection;

        m_Client.SessionStarted = true;
        return Task.CompletedTask;
    }

    public Task EndSession()
    {
        if (m_Client.SessionStarted)
        {
            m_Client.m_CurrentConnection = null;
            m_Client.SessionStarted = false;
            InworldNDKWrapper.ClientWrapper_StopClient(m_Wrapper.instance);
        }
        return Task.CompletedTask;
    }

    public void SendEvent(Inworld.Packets.InworldPacket packet)
    {
        //byte[] packetBytes = packet.ToGrpc().ToByteArray();
        //IntPtr packetPtr = Marshal.AllocHGlobal(packetBytes.Length);
        //try
        //{
        //    Marshal.Copy(packetBytes, 0, packetPtr, packetBytes.Length);
        if (packet.Routing.Target.IsAgent() && packet is Inworld.Packets.TextEvent)
        {
            Inworld.Packets.TextEvent textEvent = (packet as Inworld.Packets.TextEvent);
            Debug.Log("SENDING AN EVENT " + packet.GetType().Name + " text is " + textEvent.Text);
            InworldNDKWrapper.ClientWrapper_SendTextMessage(m_Wrapper.instance, textEvent.Routing.Target.Id, textEvent.Text);
        }
        else if (packet.Routing.Source.IsPlayer() && packet is Inworld.Packets.AudioChunk)
        {
            SendAudio(new Inworld.Packets.AudioChunk(packet.ToGrpc()));
        }
        else
            SendRawEvent(packet.ToGrpc());//InworldNDKWrapper.ClientWrapper_SendPacket(m_Wrapper.instance, packetPtr);
        //}
        //finally
        //{
        //    Marshal.FreeHGlobal(packetPtr);  // Make sure to free the allocated memory
        //}
    }

    void SendRawEvent(GrpcPacket packet)
    {
        //if(packet.Text != null) 
        //    Debug.Log("Sending raw event " + packet.Text);

        byte[] packetBytes = packet.ToByteArray();
        IntPtr packetPtr = Marshal.AllocHGlobal(packetBytes.Length);
        try
        {
            Marshal.Copy(packetBytes, 0, packetPtr, packetBytes.Length);
            InworldNDKWrapper.ClientWrapper_SendPacket(m_Wrapper.instance, packetPtr);
        }
        finally
        {
            Marshal.FreeHGlobal(packetPtr);  // Make sure to free the allocated memory
        }
    }

    public void StartAudio(Inworld.Packets.Routing routing)
    {
        Debug.Log("STARTING AUDIO");
        if (routing.Source.IsPlayer())
            InworldNDKWrapper.ClientWrapper_StartAudioSession(m_Wrapper.instance, routing.Target.Id);
    }

    public void SendAudio(Inworld.Packets.AudioChunk audioChunk)
    {
        //Debug.Log("SENDING AUDIO CHUNK");
        SendRawEvent(audioChunk.ToGrpc()); 
        //InworldNDKWrapper.ClientWrapper_SendSoundMessage(m_Wrapper.instance, audioChunk.Routing.Target.Id, audioChunk.Chunk.ToString());     
    }

    public void EndAudio(Inworld.Packets.Routing routing)
    {
        if (routing.Target.IsAgent())
            InworldNDKWrapper.ClientWrapper_StopAudioSession(m_Wrapper.instance, routing.Target.Id);
    }

    public void Destroy()
    {
    }
}
#endif
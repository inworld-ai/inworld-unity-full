using Inworld.Packets;
using Inworld.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if INWORLD_NDK
using GrpcPacket = Inworld.ProtoBuf.InworldPacket;
#else
using GrpcPacket = Inworld.Grpc.InworldPacket;
#endif

public class InworldPacketGenerator 
{
    private static InworldPacketGenerator _instance;

    public static InworldPacketGenerator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new InworldPacketGenerator();
            }

            return _instance;
        }
    }

    public InworldPacket Create(GrpcPacket packet, string messageType)
    {
        switch (messageType)
        {
            case "TextEvent":
                Inworld.Packets.TextEvent textEvent = new Inworld.Packets.TextEvent();
                textEvent.Text = packet.Text.Text;
                return textEvent;
            // case "ControlEvent":
            //     return new Inworld.ProtoBuf.ControlEvent();
            // case "AudioChunk":
            //     return new Inworld.ProtoBuf.AudioChunk();
            // case "CustomEvent":
            //     return new Inworld.ProtoBuf.CustomEvent();
            // case "CancelResponsesEvent":
            //     return new Inworld.ProtoBuf.CancelResponsesEvent();
            // case "EmotionEvent":
            //     return new Inworld.ProtoBuf.EmotionEvent();
            // case "DataChunk":
            //     return new Inworld.ProtoBuf.DataChunk();
            // case "ActionEvent":
            //     return new Inworld.ProtoBuf.ActionEvent();
            // case "MutationEvent":
            //     return new Inworld.ProtoBuf.MutationEvent();
            // case "LoadSceneOutputEvent":
            //     return new Inworld.ProtoBuf.LoadSceneOutputEvent();
            // case "DebugInfoEvent":
            //     return new Inworld.ProtoBuf.DebugInfoEvent();
            default:
                InworldAI.Log("Unknown message type: " + messageType + " in InworldPacketGenerator.Create()");
                return null; 
        }
    }
}
/*
 * /*************************************************************************************************
 * * Copyright 2022 Theai, Inc. (DBA Inworld)
 * *
 * * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 * *************************************************************************************************/
 */

using Google.Protobuf;
using Inworld.Packets;
using Inworld.ProtoBuf;
using Inworld.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActionEvent = Inworld.Packets.ActionEvent;
using Actor = Inworld.Packets.Actor;
using ControlEvent = Inworld.Packets.ControlEvent;
using CustomEvent = Inworld.Packets.CustomEvent;
using EmotionEvent = Inworld.Packets.EmotionEvent;
using InworldPacket = Inworld.Packets.InworldPacket;
using PacketId = Inworld.Packets.PacketId;
using Routing = Inworld.Packets.Routing;
using TextEvent = Inworld.Packets.TextEvent;
#if INWORLD_NDK
using ProtoPacket = Inworld.ProtoBuf.InworldPacket;
using ProtoPacketID = Inworld.ProtoBuf.PacketId;
using ProtoRouting = Inworld.ProtoBuf.Routing;
using ProtoActor = Inworld.ProtoBuf.Actor;
using ActorTypes = Inworld.ProtoBuf.Actor.Types; 
using ProtoControlEvent = Inworld.ProtoBuf.ControlEvent;
using ProtoEmotionEvent = Inworld.ProtoBuf.EmotionEvent;
using ProtoTextEvent = Inworld.ProtoBuf.TextEvent;
using ProtoDataChunk = Inworld.ProtoBuf.DataChunk;
using ProtoActionEvent = Inworld.ProtoBuf.ActionEvent;
using ProtoCustomEvent = Inworld.ProtoBuf.CustomEvent;
#else
using ProtoPacket = Inworld.Grpc.InworldPacket;
using ProtoPacketID = Inworld.Grpc.PacketId;
using ProtoRouting = Inworld.Grpc.Routing;
using ProtoActor = Inworld.Grpc.Actor;
using ActorTypes = Inworld.Grpc.Actor.Types;
using ProtoControlEvent = Inworld.Grpc.ControlEvent;
using ProtoEmotionEvent = Inworld.Grpc.EmotionEvent;
using ProtoTextEvent = Inworld.Grpc.TextEvent;
using ProtoDataChunk = Inworld.Grpc.DataChunk;
using ProtoActionEvent = Inworld.Grpc.ActionEvent;
using ProtoCustomEvent = Inworld.Grpc.CustomEvent;
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

    public ProtoPacket ToProtobufPacket(InworldPacket packet)
    {
        ProtoPacket protoPacket = new ProtoPacket();
        if (packet.PacketBytes == null)
        {
            switch (packet)
            {
                case TextEvent textEvent:
                    ProtoTextEvent te = new ProtoTextEvent();
                    te.Text = textEvent.Text;
                    te.SourceType = textEvent.SourceType;
                    protoPacket.Text = te;
                    packet.PacketBytes = protoPacket.ToByteArray();
                    // Handle TextEvent case
                    // You can access specific properties and perform actions on the textEvent instance
                    break;
                case AudioChunk audioChunk:
                    ProtoDataChunk dc = new ProtoDataChunk();
                    dc.Chunk = audioChunk.Chunk;
                    dc.AdditionalPhonemeInfo.AddRange(audioChunk.PhonemeInfo);
                    dc.DurationMs = audioChunk.duration;
                    dc.Type = ProtoDataChunk.Types.DataType.Audio;
                    protoPacket.DataChunk = dc;
                    packet.PacketBytes = protoPacket.ToByteArray();
                    // Handle AudioChunk case
                    // You can access specific properties and perform actions on the audioChunk instance
                    break;
                case ControlEvent controlEvent:
                    ProtoControlEvent ce = new ProtoControlEvent();
                    ce.Action = controlEvent.Action;
                    protoPacket.Control = ce;
                    packet.PacketBytes = protoPacket.ToByteArray();
                    Debug.Log("should have set control event packetbytes");
                    // Handle ControlEvent case
                    // You can access specific properties and perform actions on the controlEvent instance
                    break;
                case ActionEvent actionEvent:
                    ProtoActionEvent pae = new ProtoActionEvent();
                    NarratedAction narratedAction = new NarratedAction();
                    narratedAction.Content = actionEvent.Content;
                    pae.Playback = Playback.Utterance;
                    pae.NarratedAction = narratedAction;
                    protoPacket.Action = pae;
                    packet.PacketBytes = protoPacket.ToByteArray();
                    // Handle ActionEvent case
                    // You can access specific properties and perform actions on the actionEvent instance
                    break;
                case CustomEvent customEvent:
                    ProtoCustomEvent pce = new ProtoCustomEvent
                    {
                        Name = customEvent.TriggerName
                    };
                    foreach (KeyValuePair<string, string> kvp in customEvent.Parameters)
                    {
                        pce.Parameters.Add(new ProtoCustomEvent.Types.Parameter()
                        {
                            Name = kvp.Key,
                            Value = kvp.Value
                        });
                    }
                    protoPacket.Custom = pce;
                    packet.PacketBytes = protoPacket.ToByteArray();
                    // Handle CustomEvent case
                    // You can access specific properties and perform actions on the customEvent instance
                    break;
                default:
                    // Handle other cases or throw an exception for unsupported types
                    throw new($"Unsupported packet type: {packet.GetType().Name}");
            }
        }
        return ProtoPacket.Parser.ParseFrom(packet.PacketBytes);
    }

    public T FromProtobufPacket<T>(ProtoPacket packet) where T : InworldPacket
    {
        if (typeof(T) == typeof(TextEvent))
        {
            Inworld.Packets.TextEvent textEvent = new Inworld.Packets.TextEvent(packet.ToByteArray());
            return (T)(object)textEvent;
        }
        else if (typeof(T) == typeof(AudioChunk))
        {
            Inworld.Packets.AudioChunk audioChunk = new Inworld.Packets.AudioChunk(packet.ToByteArray());
            return (T)(object)audioChunk;
        }
        else if (typeof(T) == typeof(ControlEvent))
        {
            Inworld.Packets.ControlEvent controlEvent = new Inworld.Packets.ControlEvent(packet.ToByteArray());
            return (T)(object)controlEvent;
        }
        else if (typeof(T) == typeof(ActionEvent))
        {
            Inworld.Packets.ActionEvent actionEvent = new Inworld.Packets.ActionEvent(packet.ToByteArray());
            return (T)(object)actionEvent;
        }
        else if (typeof(T) == typeof(CustomEvent))
        {
            Inworld.Packets.CustomEvent customEvent = new Inworld.Packets.CustomEvent(packet.ToByteArray());
            return (T)(object)customEvent;
        }
        else if (typeof(T) == typeof(EmotionEvent))
        {
            Inworld.Packets.EmotionEvent emotionEvent = new EmotionEvent(packet.ToByteArray());
            return (T)(object)emotionEvent;
        }

        throw new ("Unsupported packet type " + typeof(T));
    }

    public ProtoRouting ToProtoRouting(Routing routing)
    {
        ProtoRouting rp = new ProtoRouting();
        rp.Source = ToProtoActor(routing.Source);
        rp.Target = ToProtoActor(routing.Target);
        return rp;
    }
    
    public Routing FromProtoRouting(ProtoRouting routing)
    {
        return new Routing(FromProtoActor(routing.Source), FromProtoActor(routing.Target));
    }
    
    public ProtoActor ToProtoActor(Actor actor)
    {
        ProtoActor a = new ProtoActor();
        a.Name = actor.Id;
        a.Type = ToProtoActorType(actor.Type);
        return a;
    }
    
    public Actor FromProtoActor(ProtoActor actor)
    {
        return new Actor(FromProtoActorType(actor.Type), actor.Name);
    }
    
    public ProtoActor.Types.Type ToProtoActorType(ActorType type)
    {
        return (ProtoActor.Types.Type)type;
    }
    
    public ActorType FromProtoActorType(ProtoActor.Types.Type type)
    {
        return (ActorType)type;

    }

    public ProtoPacketID ToProtoPacketId(string packetId, string utteranceId, string interactionId, string correlatedId)
    {
        return new ProtoPacketID
        {
            PacketId_ = packetId, UtteranceId = utteranceId, InteractionId = interactionId, CorrelationId = correlatedId
        };
    }
    
    public ProtoPacketID ToProtoPacketId(PacketId packetId)
    {
        return new ProtoPacketID
        {
            PacketId_ = packetId.PacketId_, UtteranceId = packetId.UtteranceId, InteractionId = packetId.InteractionId, CorrelationId = packetId.CorrelatedId
        };
    }
    
    public PacketId FromProtoPacketId(ProtoPacketID packetId)
    {
        return new PacketId
        {
            PacketId_ = packetId.PacketId_, UtteranceId = packetId.UtteranceId, InteractionId = packetId.InteractionId, CorrelatedId = packetId.CorrelationId
        };
    }
}
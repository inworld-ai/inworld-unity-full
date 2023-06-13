using Google.Protobuf;
using Inworld.Packets;
using Inworld.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InworldPacket = Inworld.Packets.InworldPacket;
#if INWORLD_NDK
using ProtoPacket = Inworld.ProtoBuf.InworldPacket;
using ProtoPacketID = Inworld.ProtoBuf.PacketId;
using ProtoRouting = Inworld.ProtoBuf.Routing;
using ProtoActor = Inworld.ProtoBuf.Actor;
using ActorTypes = Inworld.ProtoBuf.Actor.Types;  
#else
using ProtoPacket = Inworld.Grpc.InworldPacket;
using ProtoPacketID = Inworld.Grpc.PacketId;
using ProtoRouting = Inworld.Grpc.Routing;
using ProtoActor = Inworld.Grpc.Actor;
using ActorTypes = Inworld.Grpc.Actor.Types;
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

        throw new ("Unsupported packet type");
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
using System;
using System.Collections.Generic;
#if INWORLD_NDK
using GrpcPacket = Inworld.ProtoBuf.InworldPacket;
using GrpcPacketID = Inworld.ProtoBuf.PacketId;
using GrpcRouting = Inworld.ProtoBuf.Routing;
using GrpcActor = Inworld.ProtoBuf.Actor;
using ActorTypes = Inworld.ProtoBuf.Actor.Types;  
#else
using GrpcPacket = Inworld.Grpc.InworldPacket;
using GrpcPacketID = Inworld.Grpc.PacketId;
using GrpcRouting = Inworld.Grpc.Routing;
using GrpcActor = Inworld.Grpc.Actor;
using ActorTypes = Inworld.Grpc.Actor.Types;
#endif

namespace Inworld.Packets
{
    public interface InworldPacket
    {
        DateTime Timestamp { get; set; }
        Routing Routing { get; set; }
        PacketId PacketId { get; set; }
        GrpcPacket ToGrpc();
        
    }

    public class PacketId
    {
        public string PacketId_ = Guid.NewGuid().ToString();
        public string UtteranceId = Guid.NewGuid().ToString();
        public string InteractionId = Guid.NewGuid().ToString();
        public string CorrelatedId = Guid.NewGuid().ToString();

        public PacketId() { }

        public PacketId(GrpcPacketID packetId)
        {
            PacketId_ = packetId.PacketId_;
            UtteranceId = packetId.UtteranceId;
            InteractionId = packetId.InteractionId;
            CorrelatedId = packetId.CorrelationId;
        }

        public GrpcPacketID ToGrpc()
        {
            return new GrpcPacketID
            {
                PacketId_ = PacketId_, UtteranceId = this.UtteranceId, InteractionId = this.InteractionId, CorrelationId = CorrelatedId
            };
        }

        public override string ToString()
        {
            return $"{nameof(PacketId_)}: {PacketId_}, {nameof(UtteranceId)}: {UtteranceId}, {nameof(InteractionId)}: {InteractionId} {nameof(CorrelatedId)}: {CorrelatedId}";
        }

        protected bool Equals(PacketId other)
        {
            return PacketId_ == other.PacketId_ && UtteranceId == other.UtteranceId && InteractionId == other.InteractionId && CorrelatedId == other.CorrelatedId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PacketId) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (PacketId_ != null ? PacketId_.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (UtteranceId != null ? UtteranceId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (InteractionId != null ? InteractionId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CorrelatedId != null ? CorrelatedId.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
    
    public class Actor
    {
        public ActorTypes.Type Type;
        // agentId for AGENT type.
        public string Id;

        static public Actor Player() => 
            new Actor() { Type = ActorTypes.Type.Player };

        static public Actor Agent(string agentId) =>
            new Actor() { Type = ActorTypes.Type.Agent, Id = agentId };

        public Actor(ActorTypes.Type type, string id) 
        {
            Type = type;
            Id = id;
        }

        public Actor() : this(ActorTypes.Type.Unknown, null) { }

        public Actor(GrpcActor grpc)
        {
            Type = grpc.Type;
            if (!string.IsNullOrEmpty(grpc.Name))
                Id = grpc.Name;
            else
                Id = null;
        }

        public GrpcActor ToGrpc()
        {
            var result = new GrpcActor { Type = Type };
            if (Id != null)
                result.Name = Id;
            return result;
        }

        public override string ToString() => $"(Type={Type}, Id={Id})";

        public bool IsAgent() => Type == ActorTypes.Type.Agent;

        public bool IsPlayer() => Type == ActorTypes.Type.Player;

        protected bool Equals(Actor other)
        {
            return Type == other.Type && Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Actor) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Type * 397) ^ (Id != null ? Id.GetHashCode() : 0);
            }
        }
    }

    public class Routing
    {
        public Actor Source;
        public Actor Target;

        static public Routing FromPlayerToAgent(string agentId) =>
            new Routing(Actor.Player(), Actor.Agent(agentId));

        static public Routing FromAgentToPlayer(string agentId) =>
            new Routing(Actor.Agent(agentId), Actor.Player());

        public Routing() : this(new Actor(), new Actor()) { }

        public Routing(Actor Source, Actor Target)
        {
            this.Source = Source;
            this.Target = Target;
        }

        public Routing(GrpcRouting grpc)
        {
            if (grpc != null)
            {
                Source = new Actor(grpc.Source);
                Target = new Actor(grpc.Target);
            }
            else
            {
                Source = new Actor();
                Target = new Actor();
            }
        }

        public GrpcRouting ToGrpc() => new GrpcRouting
        {
            Source = Source?.ToGrpc(),
            Target = Target?.ToGrpc()
        };

        public override string ToString() => $"(Source={Source}, Target={Target})";

        protected bool Equals(Routing other)
        {
            return Equals(Source, other.Source) && Equals(Target, other.Target);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Routing) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Source != null ? Source.GetHashCode() : 0) * 397) ^ (Target != null ? Target.GetHashCode() : 0);
            }
        }
    }
}
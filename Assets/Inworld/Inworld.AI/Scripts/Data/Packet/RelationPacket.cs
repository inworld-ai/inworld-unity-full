using System;

namespace Inworld.Packet
{
    [Serializable]
    public class RelationState
    {
        public int trust;
        public int respect;
        public int familiar;
        public int flirtatious;
        public int attraction;
        
        public string GetUpdate(RelationState rhs) => $"{_GetDiff("Trust", trust, rhs.trust)} {_GetDiff("Respect", respect, rhs.respect)} {_GetDiff("Familiar", familiar, rhs.familiar)} {_GetDiff("Flirtatious", flirtatious, rhs.flirtatious)} {_GetDiff("Attraction", attraction, rhs.attraction)}";
        
        string _GetDiff(string name, int nCurrent, int nUpdate)
        {
            int nDiff = nUpdate - nCurrent;
            if (nDiff > 0)
                return $"{name}: +{nDiff}";
            if (nDiff < 0)
                return $"{name}: {nDiff}";
            return "";
        }
    }
    [Serializable]
    public class RelationData
    {
        public RelationState relationState;
        public RelationState relationUpdate;
    }
    [Serializable]
    public class RelationEvent
    {
        public RelationData relation;
    }
    [Serializable]
    public class RelationPacket : InworldPacket
    {
        public RelationEvent debugInfo;
        
        public RelationPacket()
        {
            debugInfo = new RelationEvent();
        }
        public RelationPacket(InworldPacket rhs, RelationEvent evt) : base(rhs)
        {
            debugInfo = evt;
        }
    }
}
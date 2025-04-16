/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Newtonsoft.Json;
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

        public bool IsEqualTo(RelationState rhs)
        {
            return rhs.trust == trust && rhs.respect == respect && rhs.familiar == familiar && rhs.flirtatious == flirtatious && rhs.attraction == attraction;
        }
        public override string ToString()
        {
            return $"Trust: {trust}, Respect: {respect}, Familiar: {familiar}, Flirtatious: {flirtatious}, Attraction {attraction}";
        }
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
        public void UpdateByTrigger(TriggerParameter input)
        {
            switch (input.name)
            {
                case "trust":
                    int.TryParse(input.value, out trust);
                    break;
                case "respect": 
                    int.TryParse(input.value, out respect);
                    break;
                case "familiar":
                    int.TryParse(input.value, out familiar);
                    break;
                case "flirtatious":
                    int.TryParse(input.value, out flirtatious);
                    break;
                case "attraction":
                    int.TryParse(input.value, out attraction);
                    break;
            }
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
        [JsonIgnore]
        public string Relation => debugInfo.relation.relationState.GetUpdate(debugInfo.relation.relationUpdate);
    }
}

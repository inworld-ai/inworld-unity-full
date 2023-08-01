using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;

namespace Inworld.Packet
{
    [Serializable]
    public class TriggerParamer
    {
        public string name;
        public string value;
    }
    [Serializable]
    public class ClientTrigger
    {
        public string name;
        public List<TriggerParamer> parameters;
        
        public ClientTrigger()
        {
            name = "";
            parameters = null;
        }

        public ClientTrigger(string eventName, Dictionary<string, string> eventParameters)
        {
            name = eventName;
            if (eventParameters != null)
                parameters = eventParameters.Select(parameter =>
                                                                                 new TriggerParamer
                                                                                 {
                                                                                     name = parameter.Key,
                                                                                     value = parameter.Value
                                                                                 }).ToList();
        }
    }
    [Serializable]
    public class CustomPacket : InworldPacket
    {
        const string k_Pattern = @"^inworld\.goal\.complete\.(.+)$";
        [FormerlySerializedAs("custom")] public ClientTrigger clientTrigger;
        public string TriggerName
        {
            get
            {
                Match match = new Regex(k_Pattern).Match(clientTrigger.name);
                return match.Success && match.Groups.Count > 1 ? match.Groups[1].Value : clientTrigger.name;
            }
        }
        public string Trigger => clientTrigger.parameters.Aggregate(TriggerName, (current, param) => current + $" {param.name}: {param.value}");

        public CustomPacket()
        {
            clientTrigger = new ClientTrigger();
        }
        public CustomPacket(InworldPacket rhs, ClientTrigger evt) : base(rhs)
        {
            clientTrigger = evt;
        }
    }
}

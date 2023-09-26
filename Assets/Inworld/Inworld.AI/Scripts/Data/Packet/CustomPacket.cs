using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    public class CustomEvent
    {
        public string name;
        public List<TriggerParamer> parameters;
        
        public CustomEvent()
        {
            name = "";
            parameters = new List<TriggerParamer>();
        }

        public CustomEvent(string eventName, Dictionary<string, string> eventParameters)
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
        public CustomEvent custom;
        public string TriggerName
        {
            get
            {
                Match match = new Regex(k_Pattern).Match(custom.name);
                return match.Success && match.Groups.Count > 1 ? match.Groups[1].Value : custom.name;
            }
        }
        public string Trigger
        {
            get
            {
                string result = TriggerName;
                if (custom.parameters == null || custom.parameters.Count == 0)
                    return result;
                foreach (var param in custom.parameters)
                {
                    result += $"{param.name}: {param.value} ";
                }
                return result;
            }
        }

        public CustomPacket()
        {
            custom = new CustomEvent();
        }
        public CustomPacket(InworldPacket rhs, CustomEvent evt) : base(rhs)
        {
            custom = evt;
        }
    }
}

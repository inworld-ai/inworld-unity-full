using System;
using System.Collections.Generic;
using System.Linq;

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
            parameters = null;
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
        public CustomEvent custom;
        
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

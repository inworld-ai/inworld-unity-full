/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Inworld.Packet
{
    [Serializable]
    public class TriggerParameter
    {
        public string name;
        public string value;
    }
    [Serializable]
    public class CustomEvent
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public CustomType type;
        public string name;
        public List<TriggerParameter> parameters;

        public CustomEvent()
        {
            name = "";
            parameters = new List<TriggerParameter>();
        }

        public CustomEvent(string eventName, Dictionary<string, string> eventParameters)
        {
            name = eventName;
            type = CustomType.TRIGGER;
            if (eventParameters != null)
                parameters = eventParameters.Select
                (
                    parameter =>
                        new TriggerParameter
                        {
                            name = parameter.Key,
                            value = parameter.Value
                        }
                ).ToList();
        }
    }
    [Serializable]
    public sealed class CustomPacket : InworldPacket
    {
        public CustomEvent custom;

        public CustomPacket()
        {
            custom = new CustomEvent();
        }
        public CustomPacket(string triggerName, Dictionary<string, string> parameters = null)
        {
            custom = new CustomEvent(triggerName, parameters);
            PreProcess();
        }
        public CustomPacket(InworldPacket rhs, CustomEvent evt) : base(rhs)
        {
            custom = evt;
        }
        public string TriggerName
        {
            get
            {
                switch (Message)
                {
                    case InworldMessage.GoalComplete:
                        return custom.name.Substring(InworldMessenger.GoalCompleteHead);
                    case InworldMessage.None:
                        return custom.name;
                }
                return "";
            }
        }

        public string Trigger
        {
            get
            {
                string result = TriggerName;
                if (custom.parameters == null || custom.parameters.Count == 0)
                    return result;
                return custom.parameters.Aggregate(result, (current, param) => current + $" {param.name}: {param.value} ");
            }
        }
        public InworldMessage Message => InworldMessenger.ProcessPacket(this);
    }
}

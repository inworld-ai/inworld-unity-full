/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inworld.Packet
{
    public class ControlEventDeserializer : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Not used. 
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            if (jo["audioSessionStart"] != null)
                return jo.ToObject<AudioControlEvent>(serializer);
            if (jo["conversationUpdate"] != null)
                return jo.ToObject<ConversationControlEvent>(serializer);
            if (jo["sessionControl"] != null)
                return jo.ToObject<SessionControlEvent>(serializer);
            if (jo["currentSceneStatus"] != null)
                return jo.ToObject<CurrentSceneStatusEvent>(serializer);
            return jo.ToObject<ControlEvent>(serializer);
        }
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ControlEvent);
        }
        public override bool CanWrite => false; // YAN: Use default serializer.
    }
    public class ControlEvent
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ControlType action;
        public string description;
    }
    public class AudioControlEvent : ControlEvent
    {
        public AudioSessionPayload audioSessionStart;

        public AudioControlEvent()
        {
            action = ControlType.AUDIO_SESSION_START;
        }
    }
    public class AudioSessionPayload
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public MicrophoneMode mode;
        [JsonConverter(typeof(StringEnumConverter))]
        public UnderstandingMode understandingMode ;
    }
    public class ConversationControlEvent : ControlEvent
    {
        public ConversationUpdatePayload conversationUpdate;

        public ConversationControlEvent()
        {
            action = ControlType.CONVERSATION_UPDATE;
        }
    }
    public class ConversationUpdatePayload
    {
        public List<Source> participants;
    }
    public class SessionControlEvent : ControlEvent
    {
        public SessionConfigurationPayload sessionConfiguration;

        public SessionControlEvent()
        {
            action = ControlType.SESSION_CONFIGURATION;
        }
    }
    public class SessionConfigurationPayload
    {
        public SessionConfiguration sessionConfiguration;
        public UserRequest userConfiguration;
        public Client clientConfiguration;
        public Capabilities capabilitiesConfiguration;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public Continuation continuation;
    }
    public class CurrentSceneStatusEvent : ControlEvent
    {
        public CurrentSceneStatusPayload currentSceneStatus;
        public CurrentSceneStatusEvent()
        {
            action = ControlType.CURRENT_SCENE_STATUS;
        }
    }
    public class CurrentSceneStatusPayload
    {
        public List<InworldCharacterData> agents;
        public string sceneName;
        public string sceneDescription;
        public string sceneDisplayName;
    }
    public sealed class ControlPacket : InworldPacket
    {
        [JsonConverter(typeof(ControlEventDeserializer))]
        public ControlEvent control;
        public ControlPacket()
        {
            control = new ControlEvent();
        }
        public ControlPacket(ControlEvent evt)
        {
            control = evt;
            PreProcess();
        }
        public ControlPacket(ControlEvent evt, Dictionary<string, string> targets)
        {
            control = evt;
            PreProcess(targets);
        }
        public ControlPacket(InworldPacket rhs, ControlEvent evt) : base(rhs)
        {
            control = evt;
        }
        protected override void UpdateRouting()
        {
            base.UpdateRouting();
            if (!(control is ConversationControlEvent convoEvt))
                return;
            routing = new Routing();
            convoEvt.conversationUpdate.participants = OutgoingTargets.Values.Where(agentID => !string.IsNullOrEmpty(agentID)).Select(agentID => new Source(agentID)).ToList();
        }
        [JsonIgnore]
        public ControlType Action => control?.action ?? ControlType.UNKNOWN;
    }
}

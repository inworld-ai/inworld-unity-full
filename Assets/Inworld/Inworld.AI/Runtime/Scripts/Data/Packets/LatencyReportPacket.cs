/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;

namespace Inworld.Packet
{
	public class LatencyEventDeserializer : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			// Not used. 
		}
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jo = JObject.Load(reader);
			if (jo["pingPong"] != null)
				return jo.ToObject<PingPongEvent>(serializer);
			if (jo["perceivedLatency"] != null)
				return jo.ToObject<PerceivedLatencyEvent>(serializer);
			return jo.ToObject<LatencyReportEvent>(serializer);
		}
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(LatencyReportEvent);
		}
		public override bool CanWrite => false; // YAN: Use default serializer.
	}
	
	[Serializable]
	public class LatencyReportEvent
	{
	
	}

	[Serializable]
	public class PingPong
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public PingPongType type;

		// The pong response will have a copy of the ping packet ID.
		public PacketId pingPacketId;

		// The pong response will have a copy of the ping timestamp.
		public string pingTimestamp;
	}
	[Serializable]
	public class PingPongEvent : LatencyReportEvent
	{
		public PingPong pingPong;
	}

	[Serializable]
	public class PerceivedLatency
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public Precision precision;
		[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
		public string latency; 
	}
	[Serializable]
	public class PerceivedLatencyEvent : LatencyReportEvent
	{
		public PerceivedLatency perceivedLatency;
	}
	[Serializable]
	public class LatencyReportPacket : InworldPacket
	{
		[JsonConverter(typeof(LatencyEventDeserializer))]
		public LatencyReportEvent latencyReport;
		
		public LatencyReportPacket()
		{
			latencyReport = new LatencyReportEvent();
		}
		public LatencyReportPacket(LatencyReportEvent evt)
		{
			latencyReport = evt;
			PreProcess();
		}
		public LatencyReportPacket(InworldPacket rhs, LatencyReportEvent evt) : base(rhs)
		{
			latencyReport = evt;
		}
	}
}

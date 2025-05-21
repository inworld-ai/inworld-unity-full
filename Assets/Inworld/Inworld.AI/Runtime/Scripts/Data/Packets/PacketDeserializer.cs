/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Inworld.Packet
{
	public class PacketDeserializer : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			// Not used.
		}
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jo = JObject.Load(reader);
			if (jo["text"] != null)
				return jo.ToObject<TextPacket>(serializer);
			if (jo["control"] != null)
				return jo.ToObject<ControlPacket>(serializer);
			if (jo["dataChunk"] != null)
			{
				if (jo["dataChunk"]["type"]?.ToString() == "AUDIO") 
				{
					return jo.ToObject<AudioPacket>(serializer);
				}
			}
			if (jo["custom"] != null)
				return jo.ToObject<CustomPacket>(serializer);
			if (jo["emotion"] != null)
				return jo.ToObject<EmotionPacket>(serializer);
			if (jo["action"] != null)
				return jo.ToObject<ActionPacket>(serializer);
			if (jo["sessionControlResponse"] != null)
				return jo.ToObject<SessionResponsePacket>(serializer);
			if (jo["operationStatus"] != null)
				return jo.ToObject<OperationStatusPacket>(serializer);
			if (jo["latencyReport"] != null)
				return jo.ToObject<LatencyReportPacket>(serializer);
			if (jo["log"] != null)
				return jo.ToObject<LogPacket>(serializer);
			InworldAI.LogWarning($"Unsupported type {jo}");
			return jo.ToObject<InworldPacket>(serializer);
		}
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(InworldPacket);
		}
		public override bool CanWrite => false; // YAN: Use default serializer.
	}

	public class NetworkPacketResponse
    {
        [JsonConverter(typeof(PacketDeserializer))]
        public InworldPacket result;
        public InworldError error;
    }
}

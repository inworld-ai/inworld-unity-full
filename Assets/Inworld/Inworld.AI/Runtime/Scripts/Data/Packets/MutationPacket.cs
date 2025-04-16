/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;


namespace Inworld.Packet
{
	public class MutationEventDeserializer : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			// Not used. 
		}
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jo = JObject.Load(reader);
			if (jo["regenerateResponse"] != null)
				return jo.ToObject<RegenerateResponseEvent>(serializer);
			if (jo["applyResponse"] != null)
				return jo.ToObject<ApplyResponseEvent>(serializer);
			if (jo["cancelResponses"] != null)
				return jo.ToObject<CancelResponseEvent>(serializer);
			if (jo["loadScene"] != null)
				return jo.ToObject<LoadSceneEvent>(serializer);
			if (jo["loadCharacters"] != null)
				return jo.ToObject<LoadCharactersEvent>(serializer);
			if (jo["unloadCharacters"] != null)
				return jo.ToObject<UnloadCharactersRequest>(serializer);
			return jo.ToObject<MutationEvent>(serializer);
		}
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(MutationEvent);
		}
		public override bool CanWrite => false; // YAN: Use default serializer.
	}
	public class MutationEvent
	{
		
	}
	public class LoadCharactersRequest
	{
		public List<CharacterName> name;

		public LoadCharactersRequest(List<string> charFullNames)
		{
			name = new List<CharacterName>();
			foreach (string charName in charFullNames)
			{
				name.Add(new CharacterName(charName));
			}
		}
	}
	public class UnloadCharactersRequest
	{
		public List<InworldCharacterData> agents;

		public UnloadCharactersRequest(List<InworldCharacterData> agentsToUnload)
		{
			agents = new List<InworldCharacterData>(agentsToUnload);
		}
	}
	public class UnloadCharactersEvent : MutationEvent
	{
		public UnloadCharactersRequest unloadCharacters;
	}
	public class LoadCharactersEvent : MutationEvent
	{
		public LoadCharactersRequest loadCharacters;
	}
	public class LoadSceneRequest
	{
		public string name;
	}
	public class LoadSceneEvent : MutationEvent
	{
		public LoadSceneRequest loadScene;
	}
	public class CancelResponse
	{
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string interactionId;
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<string> utteranceId;
	}
	public class CancelResponseEvent : MutationEvent
	{
		public CancelResponse cancelResponses;
	}
	public class ApplyResponse
	{
		public PacketId packetId;
	}
	public class ApplyResponseEvent : MutationEvent
	{
		public ApplyResponse applyResponse;
	}
	public class RegenerateResponse
	{
		public string interactionId;
	}
	public class RegenerateResponseEvent : MutationEvent
	{
		public RegenerateResponse regenerateResponse;
	}
	public sealed class MutationPacket : InworldPacket
	{
		[JsonConverter(typeof(MutationEventDeserializer))]
		public MutationEvent mutation;

		public MutationPacket()
		{
			mutation = new MutationEvent();
		}
		public MutationPacket(MutationEvent evt)
		{
			mutation = evt;
			PreProcess();
		}
		public MutationPacket(InworldPacket rhs, MutationEvent evt) : base(rhs)
		{
			mutation = evt;
		}
		public static string LoadScene(string sceneFullName) => new MutationPacket
		{
			timestamp = InworldDateTime.UtcNow,
			packetId = new PacketId(),
			routing = new Routing("WORLD"),
			mutation = new LoadSceneEvent
			{
				loadScene = new LoadSceneRequest
				{
					name = sceneFullName
				}
			}
		}.ToJson;

		public static string UnloadCharacters(List<InworldCharacterData> characters) => new MutationPacket
		{
			timestamp = InworldDateTime.UtcNow,
			packetId = new PacketId(),
			routing = new Routing(),
			mutation = new UnloadCharactersEvent
			{
				unloadCharacters = new UnloadCharactersRequest(characters)
			}
		}.ToJson;
		public static string LoadCharacters(List<string> charactersFullName) => new MutationPacket
		{
			timestamp = InworldDateTime.UtcNow,
			packetId = new PacketId(),
			routing = new Routing(),
			mutation = new LoadCharactersEvent
			{
				loadCharacters = new LoadCharactersRequest(charactersFullName)
			}
		}.ToJson;
	}
}

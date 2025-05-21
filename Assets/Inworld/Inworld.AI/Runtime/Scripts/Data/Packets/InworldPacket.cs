/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Inworld.Packet
{
	[Serializable]
	public class Source
	{
		public string name;
		[JsonConverter(typeof(StringEnumConverter))]
		public SourceType type;

		public Source(string targetName = "")
		{
			if (targetName == InworldAI.User.Name)
			{
				name = targetName;
				type = SourceType.PLAYER;
			}
			else
			{
				name = targetName;
				type = targetName == "WORLD" ? SourceType.WORLD : SourceType.AGENT;
			}
		}
	}
	public class Routing
	{
		public Source source;
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public Source target;
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<Source> targets;

		public Routing(string character)
		{
			source = new Source(InworldAI.User.Name);
			target = new Source(character);
		}
		public Routing(List<string> characters = null)
		{
			source = new Source(InworldAI.User.Name);

			if (characters == null || characters.Count == 0)
				return;
			targets = new List<Source>();
			foreach (string characterID in characters)
			{
				targets.Add(new Source(characterID));
			}
		}
	}

	public class PacketId
	{
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string conversationId; // Used in the conversations.
		
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string correlationId; // Used in callback for server packets.
		
		public string interactionId = InworldAuth.Guid(); // Lot of sentences included in one interaction.
		public string packetId = InworldAuth.Guid(); // Unique.
		public string utteranceId = InworldAuth.Guid(); // Each sentence is an utterance. But can be interpreted as multiple behavior (Text, EmotionChange, Audio, etc)

		public override string ToString()
		{
			return $"I: {interactionId} U: {utteranceId} P: {packetId}";
		}
	}

	public class InworldPacket
	{
		public PacketId packetId;
		public Routing routing;
		public string timestamp;

		public InworldPacket()
		{
			timestamp = InworldDateTime.UtcNow;
			packetId = new PacketId();
			routing = new Routing();
		}
		public InworldPacket(InworldPacket rhs)
		{
			timestamp = rhs.timestamp;
			packetId = rhs.packetId;
			routing = rhs.routing;
		}

#region NonSerialized Properties

        /// <summary>
        ///     Key/Value Pair for targets to send.
        ///     Key is character's full name.
        ///     Value is its agent ID [Nullable] (We'll fetch it in the UpdateSessionInfo)
        /// </summary>
        [JsonIgnore]
		public Dictionary<string, string> OutgoingTargets { get; set; } = new Dictionary<string, string>();
		[JsonIgnore]
		public virtual string ToJson => JsonConvert.SerializeObject(this);
		[JsonIgnore]
		public SourceType Source => routing?.source?.type ?? SourceType.NONE;
		[JsonIgnore]
		// YAN: For now we can also differentiate if the conversationID is null.
		//		But for the future integration, setting target would be allowed with the conversation.
		public bool IsBroadCast => string.IsNullOrEmpty(routing?.target?.name);
		[JsonIgnore]
		public string SourceName => routing?.source?.name;
		[JsonIgnore]
		public string TargetName => routing?.target?.name;

#endregion
		
		protected virtual void PreProcess()
		{
			LiveInfo liveInfo = InworldController.Client.Current;
			if (liveInfo.Character == null)
				packetId.conversationId = liveInfo.Conversation.ID;
			else
			{
				OutgoingTargets = new Dictionary<string, string>
				{
					[liveInfo.Character.brainName] = liveInfo.Character.agentId
				};
				routing = new Routing(liveInfo.Character.agentId);
			}
		}
		// YAN: Only for packets that needs to explicitly set multiple targets (Like start conversation).
		//		Usually for conversation packets, do not need to call this.
		protected virtual void PreProcess(Dictionary<string, string> targets)
		{
			LiveInfo liveInfo = InworldController.Client.Current;
			packetId.conversationId = liveInfo.Conversation.ID;
			OutgoingTargets = targets;
			routing = new Routing(targets.Values.ToList());
		}
        /// <summary>
        ///     Originally OnDequeue in OutgoingPacket.
        ///     Always call it before send to fetch the agent ID.
        /// </summary>
        public virtual bool PrepareToSend()
        {
	        if (!UpdateSessionInfo())
		        return false;
	        UpdateRouting();
	        return true;
        }
        /// <summary>
        ///     Update the characters in this conversation with updated ID.
        /// </summary>
        /// <returns>The brain name of the characters not found in the current session.</returns>
        protected virtual bool UpdateSessionInfo()
		{
			if (OutgoingTargets == null || OutgoingTargets.Count == 0)
			{
				if (!InworldController.Client.Current.IsConversation)
					return false;
				routing = new Routing();
				return true;
			}
			foreach (string key in OutgoingTargets.Keys.ToList().Where(key => !string.IsNullOrEmpty(key)))
			{
				if (InworldController.Client.LiveSessionData.TryGetValue(key, out InworldCharacterData value))
					OutgoingTargets[key] = value.agentId;
				else
				{
					if (InworldController.CharacterHandler 
					    && InworldController.CharacterHandler[key] 
					    && InworldController.CharacterHandler[key].EnableVerboseLog)
						InworldAI.LogWarning($"{key} is not in the current session.");
				}
			}
			return OutgoingTargets.Count > 0 && OutgoingTargets.Values.Any(id => !string.IsNullOrEmpty(id));
		}

		protected virtual void UpdateRouting()
		{
			List<string> agentIDs = OutgoingTargets.Values.Where(c => !string.IsNullOrEmpty(c)).ToList();
			routing = new Routing(agentIDs);
		}
		public bool IsSource(string agentID)
		{
			return !string.IsNullOrEmpty(agentID) && SourceName == agentID;
		}
		

		public bool IsTarget(string agentID)
		{
			return !string.IsNullOrEmpty(agentID) && (routing?.target?.name == agentID || (routing?.targets?.Any(agent => agent.name == agentID) ?? false));
		}

		public bool IsRelated(string agentID) => Source == SourceType.PLAYER ? IsTarget(agentID) : IsSource(agentID);

	}
}

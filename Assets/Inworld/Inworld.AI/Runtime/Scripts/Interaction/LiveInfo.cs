/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace Inworld.Entities
{
	public class Conversation
	{
		string m_ConversationID;
		public string ID
		{
			get
			{
				if (string.IsNullOrEmpty(m_ConversationID))
					m_ConversationID = InworldController.CharacterHandler.ConversationID;
				return m_ConversationID;
			}
			set => m_ConversationID = value;
		}
		public ConversationEventType Status { get; set; } = ConversationEventType.EVICTED;
		public List<string> BrainNames { get; set; } = new List<string>();
	}
	public class AudioSession
	{
		public string ID { get; set; }
		public bool IsConversation { get; set; }
		public string Target { get; set; } // Either character's brain Name or ConversationID.
		public bool IsSameSession(string brainName)
		{
			if (string.IsNullOrEmpty(brainName))
			{
				if (InworldController.CharacterHandler)
					return InworldController.CharacterHandler.ConversationID == Target;
			}
			return brainName == Target;
		}

		public bool HasStarted => !string.IsNullOrEmpty(ID); 
	}
	/// <summary>
	/// This class is used for caching the current conversation and audio session.
	/// </summary>
	public class LiveInfo
	{
		bool m_IsConversation;
		public InworldCharacterData Character { get; set; } = new InworldCharacterData();
		public Conversation Conversation { get; } = new Conversation();
		public AudioSession AudioSession { get; } = new AudioSession();
		public bool UpdateLiveInfo(string brainName)
		{
			return string.IsNullOrEmpty(brainName) ? UpdateMultiTargets() : UpdateSingleTarget(brainName);
		}
		// ReSharper disable Unity.PerformanceAnalysis
		// As InworldController.CharacterHandler would return directly in most cases.
		public bool UpdateMultiTargets(string conversationID = "", List<string> brainNames = null)
		{
			if (string.IsNullOrEmpty(conversationID))
			{
				if (brainNames?.Count == 1 && !string.IsNullOrEmpty(brainNames[0]))
					return UpdateSingleTarget(brainNames[0]);
				if (InworldController.CharacterHandler.CurrentCharacters.Count == 1)
					return UpdateSingleTarget(InworldController.CharacterHandler.CurrentCharacters[0].BrainName);
				if (!InworldController.Client.EnableGroupChat)
					return false;
			}
			IsConversation = true;
			if (string.IsNullOrEmpty(conversationID) && InworldController.CharacterHandler)
				conversationID = InworldController.CharacterHandler.ConversationID;
			Conversation.ID = conversationID;
			if ((brainNames == null || brainNames.Count == 0) && InworldController.CharacterHandler)
				brainNames = InworldController.CharacterHandler.CurrentCharacterNames;
			Conversation.BrainNames = brainNames;
			return Conversation.BrainNames?.Count > 0;
		}
		// ReSharper disable Unity.PerformanceAnalysis
		// As InworldController.CharacterHandler would return directly in most cases.
		public bool UpdateSingleTarget(string brainName)
		{
			if (string.IsNullOrEmpty(brainName))
				return false;
			IsConversation = false;
			if (brainName == SourceType.WORLD.ToString())
				return true;
			if (brainName != Character?.brainName && InworldController.CharacterHandler)
				Character = InworldController.CharacterHandler[brainName]?.Data;
			return Character != null;
		}
		public bool IsConversation
		{
			get => m_IsConversation;
			set
			{
				m_IsConversation = value;
				AudioSession.IsConversation = value;
				if (m_IsConversation)
					Character = null;
			}
		}
		public void StartAudioSession(string packetID)
		{
			AudioSession.ID = packetID;
			AudioSession.Target = IsConversation ? InworldController.CharacterHandler ? InworldController.CharacterHandler.ConversationID : "" : Character?.brainName;
		}
		public void StopAudioSession()
		{
			AudioSession.ID = "";
			AudioSession.Target = "";
		}
		public string Name => IsConversation ? "the Chat group" : Character?.givenName;
	}
}

/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Packet;
using System;
using UnityEngine.Events;

namespace Inworld
{
	[Serializable]
	public class ConversationEvents
	{
		public UnityEvent<CharSelectingMethod> onCharacterSelectingModeUpdated;
		public UnityEvent<InworldCharacter> onCharacterListJoined;
		public UnityEvent<InworldCharacter> onCharacterListLeft;
		public UnityEvent onConversationUpdated;
		
		public void RemoveAllEvents()
		{
			onCharacterSelectingModeUpdated.RemoveAllListeners();
			onCharacterListJoined.RemoveAllListeners();
			onCharacterListLeft.RemoveAllListeners();
			onConversationUpdated.RemoveAllListeners();
		}
	}
}

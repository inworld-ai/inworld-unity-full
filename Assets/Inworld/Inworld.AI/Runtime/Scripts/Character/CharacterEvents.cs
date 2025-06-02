/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Packet;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Inworld
{
	[Serializable]
	public class CharacterEvents
	{
		// The first string are the character's Brain Name
		public UnityEvent<string> onCharacterSelected;				
		public UnityEvent<string> onCharacterDeselected;			
		public UnityEvent<string> onCharacterDestroyed;
		public UnityEvent<string> onBeginSpeaking; 
		public UnityEvent<string> onEndSpeaking;
		public UnityEvent<string> onInteractionEnd;
		public UnityEvent<InworldPacket> onPacketReceived; 
		public UnityEvent<string, string> onCharacterSpeaks;
		public UnityEvent<string, string> onEmotionChanged;
		public UnityEvent<string, string> onGoalCompleted;
		public UnityEvent<string, string, List<TriggerParameter>> onTaskReceived;
		public UnityEvent<string> onRelationUpdated;

		public void RemoveAllEvents()
		{
			onCharacterSelected.RemoveAllListeners();
			onCharacterDeselected.RemoveAllListeners();
			onCharacterDestroyed.RemoveAllListeners();
			onBeginSpeaking.RemoveAllListeners();
			onEndSpeaking.RemoveAllListeners();
			onPacketReceived.RemoveAllListeners();
			onCharacterSpeaks.RemoveAllListeners();
			onEmotionChanged.RemoveAllListeners();
			onGoalCompleted.RemoveAllListeners();
			onTaskReceived.RemoveAllListeners();
			onRelationUpdated.RemoveAllListeners();
		}
	}
}

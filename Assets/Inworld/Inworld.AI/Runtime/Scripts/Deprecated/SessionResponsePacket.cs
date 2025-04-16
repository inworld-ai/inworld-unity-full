/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Inworld.Packet
{
	//YAN:  To be deprecated.
	//		Currently they are still sending these data which will throw warning if marked as Obsolete.
	[Serializable]
	public class LoadSceneResponse
	{
		public List<InworldCharacterData> agents = new List<InworldCharacterData>();

		public List<string> UpdateRegisteredCharacter(ref List<InworldCharacterData> outData)
		{
			List<string> result = new List<string>();
			foreach (InworldCharacterData charData in outData)
			{
				string registeredID = agents.FirstOrDefault(a => a.brainName == charData.brainName)?.agentId;
				if (string.IsNullOrEmpty(registeredID))
					result.Add(charData.givenName);
				charData.agentId = registeredID;
			}
			return result;
		}
		[JsonIgnore]
		public bool IsValid => agents?.Count > 0;
	}
	[Serializable]
	public class SessionResponseEvent
	{
		public LoadSceneResponse loadedScene;
		public LoadSceneResponse loadedCharacters;
		
		[JsonIgnore]
		public bool IsValid => (loadedScene?.IsValid ?? false) || (loadedCharacters?.IsValid ?? false);
	}

	[Serializable]
	public class SessionResponsePacket : InworldPacket
	{
		public SessionResponseEvent sessionControlResponse;
		
		public SessionResponsePacket()
		{
			sessionControlResponse = new SessionResponseEvent();
		}
		public SessionResponsePacket(InworldPacket rhs, SessionResponseEvent evt) : base(rhs)
		{
			sessionControlResponse = evt;
		}
	}
}



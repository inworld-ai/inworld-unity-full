/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Inworld.LLM.ModelServing
{
	// Unique identifier of the model being requested.
	[Serializable]
	public class ModelID 
	{
		// The name of the model being served.
		public string model;
		
		// Service provider hosting llm and handling completion requests.
		[JsonConverter(typeof(StringEnumConverter))]
		public ServiceProvider service_provider;
		
		public ModelID(ModelName modelName = ModelName.Inworld_Dragon)
		{
			switch (modelName)
			{
				case ModelName.Llama3_1_Druid_70B:
					model = "llama-3.1-druid-70b";
					service_provider = ServiceProvider.SERVICE_PROVIDER_INWORLD;
					break;
				case ModelName.Inworld_Dragon2:
					model = "inworld-dragon-2";
					service_provider = ServiceProvider.SERVICE_PROVIDER_INWORLD;
					break;
				case ModelName.Inworld_Mage:
					model = "inworld-mage";
					service_provider = ServiceProvider.SERVICE_PROVIDER_INWORLD;
					break;
				case ModelName.GPT_4o:
					model = "gpt-4o";
					service_provider = ServiceProvider.SERVICE_PROVIDER_OPENAI;
					break;
				case ModelName.Undefined:
				case ModelName.Inworld_Dragon:
				default:
					model = "inworld-dragon";
					service_provider = ServiceProvider.SERVICE_PROVIDER_INWORLD;
					break;
			}
		}
	}
		
	[Serializable]
	public class ServingID
	{
		// ID of the model to use.
		public ModelID model_id;
		
		// Unique identifier representing end-user.
		public string user_id;
		
		// Unique identifier of the session with multiple completion requests.
		public string session_id;

		public ServingID(ModelName modelName = ModelName.Inworld_Dragon)
		{
			user_id = InworldAI.User.ID;
			model_id = new ModelID(modelName);
			session_id = InworldController.SessionID;
		}
	}
}

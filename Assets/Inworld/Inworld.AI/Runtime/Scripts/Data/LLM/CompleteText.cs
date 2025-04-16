/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.LLM.ModelConfig;
using Inworld.LLM.ModelServing;
using System.Collections.Generic;

namespace Inworld.LLM.Service
{
	public class CompleteTextRequest
	{
		// The serving ID of the request.
		public ServingID serving_id;
		
		// The prompt to generate text from.
		public Prompt prompt;
		
		// The generation configuration to use instead of the model's default one.
		public TextGenerationConfig text_generation_config;
	}
	
	// Text completion response.
	public class CompleteTextResponse 
	{
		// A unique identifier for the completion.
		public string id;

		// The list of completion choices the model generated for the input prompt.
		public List<Choice> choices;

		// The time when completion was created.
		public string create_time;

		// The model used for the completion.
		public string model;

		// Usage statistics for the completion.
		public Usage usage;
	}
}

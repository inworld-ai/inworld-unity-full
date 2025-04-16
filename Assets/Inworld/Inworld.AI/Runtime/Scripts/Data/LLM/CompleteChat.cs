/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.LLM.ModelServing;
using Inworld.LLM.ModelConfig;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Inworld.LLM.Service
{
	public class CompleteChatRequest
	{
		// The serving ID of the request.
		public ServingID serving_id;
		
		// A list of messages comprising the conversation so far.
		public List<Message> messages;
		
		// A list of tools the model may call. Currently, only functions are supported as a tool. Use this to
		// provide a list of functions the model may generate JSON inputs for.
		[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
		public List<Tool> tools;
		
		// Controls which (if any) function is called by the model.
		[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
		public ToolChoice tool_choice;

		// The generation configuration to use instead of the model's default one.
		[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
		public TextGenerationConfig text_generation_config;
		
		public CompleteChatRequest(ServingID serving, List<Message> messages, TextGenerationConfig config = null)
		{
			serving_id = serving;
			this.messages = messages;
			text_generation_config = config;
		}
		[JsonIgnore]
		public string ToJson => JsonConvert.SerializeObject(this);
	}

	public class CompleteChatResponse
	{
		// A unique identifier for the chat completion. Each chunk has the same ID.
		public string id;
		
		// A list of chat completion choices. Can be more than one if n is greater than 1.
		public List<Choice> choices;
		
		// The time when the chat completion was created.
		public string create_time;
		
		// The model used for the chat completion.
		public string model;
		
		// Usage statistics for the chat completion.
		public Usage usage;
	}
	public class NetworkCompleteChatResponse
	{
		public CompleteChatResponse result;
	}
}

/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
namespace Inworld.LLM
{
	public enum ModelName
	{
		Undefined,
		Llama3_1_Druid_70B,
		Inworld_Dragon,
		Inworld_Dragon2,
		Inworld_Mage,
		GPT_4o
	}
	public enum Kind
	{
		none, // "none" means the model will not call a function and instead generates a message.
		auto  // "auto" means the model can pick between generating a message or calling a function.
	}
	public enum ImageFidelity
	{
		// By default, the model will use the auto setting which will look at the image input size and decide if it should use the low or high setting
		auto, 
		// low will enable the "low res" mode. The model will receive a low-res 512px x 512px version of the image, and represent the image with a budget of 85 tokens. 
		// This allows the API to return faster responses and consume fewer input tokens for use cases that do not require high detail.
		low, 
		// high will enable "high res" mode, which first allows the model to first see the low res image (using 85 tokens)
		// and then creates detailed crops using 170 tokens for each 512px x 512px tile.
		high
	}
	
	// Format that the model must output..
	public enum ResponseFormat 
	{
		// Response format is not specified. Defaults to "text".
		RESPONSE_FORMAT_UNSPECIFIED = 0,
		// Text response format.
		RESPONSE_FORMAT_TEXT = 1,
		// Json response format. This guarantees that the message the model generates is
		// valid JSON. Note that your system prompt must still instruct the model to produce JSON, and to help ensure you
		// don't forget, the API will throw an error if the string JSON does not appear in your system message. Also note
		// that the message content may be partial (i.e. cut off) if finish_reason="length", which indicates the generation
		// exceeded max_tokens or the conversation exceeded the max context length.
		RESPONSE_FORMAT_JSON = 2
	}
	
	// Service provider hosting llm and handling completion requests.
	public enum ServiceProvider 
	{
		// No service provider specified.
		SERVICE_PROVIDER_UNSPECIFIED = 0,
		// Inworld service provider.
		SERVICE_PROVIDER_INWORLD = 1,
		// Microsoft Azure service provider.
		SERVICE_PROVIDER_AZURE = 2,
		// Openai service provider.
		SERVICE_PROVIDER_OPENAI = 3
	}
	// The reason the model stopped generating tokens.
	public enum FinishReason 
	{
		// Finish reason is not specified.
		FINISH_REASON_UNSPECIFIED = 0,
		// The model hit a natural stop point or a provided stop sequence.
		FINISH_REASON_STOP = 2,
		// The maximum number of tokens specified in the request was reached.
		FINISH_REASON_LENGTH = 3,
		// Content was omitted due to a flag from content safety filters.
		FINISH_REASON_CONTENT_FILTER = 4,
		// The model called a tool. Returned in CompleteChat method only.
		FINISH_REASON_TOOL_CALL = 5
	}

	// The role of the messages author.
	public enum MessageRole 
	{
		// Message role is not specified.
		MESSAGE_ROLE_UNSPECIFIED = 0,
		// Instructs the behavior of the assistant.
		MESSAGE_ROLE_SYSTEM = 1,
		// Provides request or comment for the assistant to respond to.
		MESSAGE_ROLE_USER = 2,
		// Stores previous assistant response, but can also be used to give an example of desired behavior.
		MESSAGE_ROLE_ASSISTANT = 3,
		// Provides available tools for the assistant to call.
		MESSAGE_ROLE_TOOL = 4
	}
}

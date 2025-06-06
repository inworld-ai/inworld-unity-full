﻿/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Inworld.LLM.ModelConfig
{
	// Modify the likelihood of specified tokens appearing in the completion.
	[Serializable]
	public class LogitBias 
	{
		// Id of the token.
		public string token_id;
		// Bias value from -100 to 100.
		[Range(-100, 100)] public int bias_value;
	}
	
	// Function to call.
	[Serializable]
	public class FunctionCall 
	{
		// The name of the function to be called. Must be a-z, A-Z, 0-9, or contain underscores and dashes,
		// with a maximum length of 64.
		public string name;
		// A description of what the function does, used by the model to choose when and how to call the
		// function.
		[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
		public string description;
		// The parameters the functions accepts, described as a JSON Schema object.
		
		[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
		public string properties;
	}
	
	// A tool the model may call.
	[Serializable]
	public class Tool 
	{
		// Function to call.
		public FunctionCall function_call;
	}
	
	// Controls which (if any) function is called by the model.
	[Serializable]
	public class ToolChoice
	{

	}

	[Serializable]
	public class TextToolChoice : ToolChoice
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public Kind text;
	}
	
	[Serializable]
	public class FunctionToolChoice : ToolChoice
	{
		// YAN: Noticed that in this class, the description has to be null.
		public Tool @object;
	}
	
	// Configuration on how to perform text generation e2e.
	[Serializable]
	public class TextGenerationConfig 
	{
	  [Tooltip("Positive values penalize new tokens based on their existing frequency in the text so far, decreasing.\n\n" +
	           "The model's likelihood to repeat the same line verbatim. Defaults to 0.")]
	  [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	  public float frequency_penalty;
	  
	  [Tooltip("Modify the likelihood of specified tokens appearing in the completion.\n\n" +
	           "Mathematically, the bias is added to the logits generated by the model prior to sampling. The exact effect will" +
	           "vary per model, but values between -1 and 1 should decrease or increase likelihood of selection; values like -100" + 
	           "or 100 should result in a ban or exclusive selection of the relevant token.")]
	  [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
	  public List<LogitBias> logit_bias;
	  
	  [Tooltip("Maximum number of output tokens allowed to generate. The total length of input tokens and generated" +
	           "tokens is limited by the model's context length. Defaults to inf.\n\n" +
	           "However, the Gpt4o's max token is 2500. Let's not exceed unless you know your models capacity.")]
	  public int max_tokens = 2500;
	  
	  [Tooltip("How many choices to generate for each input message. \n\nDefaults to 1.")]
	  [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
	  public int n = 1;

	  [Tooltip("Positive values penalize new tokens based on whether they appear in the text so far, increasing the " +
	           "model's likelihood to talk about new topics. \n\nDefaults to 0.")]
	  [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
	  public float presence_penalty;
	  
	  [Tooltip("Up to 4 sequences where the API will stop generating further tokens.\n\n" +
	           "For example, If you've set the related tokens, such as \"Apple\", whenever the character generates till \"Apple\" it'll be stopped.")]
	  [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
	  public List<string> stop;
	  
	  [Tooltip("If set, partial message deltas will be sent. \n\nDefaults to false.")]
	  [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
	  public bool stream;

	  [Tooltip("What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output more random, " +
	           "while lower values like 0.2 will make it more focused and deterministic. \n\nDefaults to 1.")]
	  [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
	  [Range(0,2)] public float temperature = 1;
	  
	  [Tooltip("An alternative to sampling with temperature, called nucleus sampling, where the model considers the " +
	           "results of the tokens with top_p probability mass. \n\nSo 0.1 means only the tokens comprising the top 10% probability " +
	           "mass are considered. \n\nDefaults to 1.")]
	  [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
	  [Range(0,1)] public float top_p = 1;

	  [Tooltip("Float that penalizes new tokens based on whether they appear in the prompt and the " +
	           "generated text so far. Values > 1 encourage the model to use new tokens, while " +
	           "values < 1 encourage the model to repeat tokens. \n\nThe value must be strictly " +
	           "positive. Defaults to 1 (no penalty).")]
	  [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
	  public float repetition_penalty = 1;
	}
}

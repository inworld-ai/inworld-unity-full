/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.LLM.Service;
using Inworld.UI;

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace Inworld.LLM
{
	public class LLMChatPanel : BubblePanel
	{
		[SerializeField] protected ChatBubble m_BubbleLeft;
		[SerializeField] protected ChatBubble m_BubbleRight;
		[SerializeField] protected TMP_InputField m_InputField;
		[SerializeField] protected Texture2D m_PlayerThumbnail;
		[SerializeField] protected Button m_SendButton;
		[SerializeField] protected string m_SubmitName;

		protected InputAction m_SubmitAction;
		Dictionary<string, ChatBubble> m_ChatBubbles = new Dictionary<string, ChatBubble>();
		
		protected void Awake()
		{
			if (string.IsNullOrEmpty(m_SubmitName))
				return;
			m_SubmitAction = InworldAI.InputActions[m_SubmitName];
			if (!m_PlayerThumbnail)
				m_PlayerThumbnail = InworldAI.DefaultThumbnail;
		}
		protected void OnEnable()
		{
			if (!InworldController.Instance)
				return;
			InworldController.LLM.onChatHistoryUpdated.AddListener(OnChatUpdated);
			InworldController.Instance.OnControllerStatusChanged += OnControllerStatusChanged;
		}
		protected void OnDisable()
		{
			if (!InworldController.Instance)
				return;
			InworldController.LLM.onChatHistoryUpdated.RemoveListener(OnChatUpdated);
			InworldController.Instance.OnControllerStatusChanged -= OnControllerStatusChanged;
		}
		protected void Update()
		{
			HandleInput();
		}
		protected void HandleInput()
		{
			if (m_SubmitAction != null && m_SubmitAction.WasReleasedThisFrame())
				Submit();
		}
		public void Submit()
		{
			if (!m_InputField || string.IsNullOrEmpty(m_InputField.text))
				return;
			string text = m_InputField.text;
			InworldController.LLM.SendText(text);
			m_InputField.text = "";
		}
		public void OnChatUpdated()
		{
			List<Message> chatHistories = InworldController.LLM.ChatHistory;
			foreach (Message history in chatHistories)
			{
				string hash = history.ToHash;
				ChatBubble bubble = history.role == MessageRole.MESSAGE_ROLE_USER ? m_BubbleRight : m_BubbleLeft;
				Texture2D thumbnail = history.role == MessageRole.MESSAGE_ROLE_USER ? m_PlayerThumbnail : InworldAI.Logo;
				InsertBubble(hash, bubble, history.Role, false, history.ToMessage, thumbnail);
			}
		}
		void OnControllerStatusChanged(InworldConnectionStatus status, string detail)
		{
			if (m_SendButton)
				m_SendButton.interactable = status == InworldConnectionStatus.Initialized;
		}
	}
}

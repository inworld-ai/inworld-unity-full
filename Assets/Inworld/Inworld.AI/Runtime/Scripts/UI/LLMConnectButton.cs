/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

namespace Inworld.UI
{
	public class LLMConnectButton : ConnectButton
	{
		public override void ConnectInworld()
		{
			InworldController.Instance.GetAccessToken();
		}
		void OnEnable()
		{
			if (!InworldController.Instance)
				return;
			InworldController.Instance.OnControllerStatusChanged += OnControllerStatusChanged;
		}

		void OnDisable()
		{
			if (!InworldController.Instance)
				return;
			InworldController.Instance.OnControllerStatusChanged -= OnControllerStatusChanged;
		}
		void OnControllerStatusChanged(InworldConnectionStatus newStatus, string detail)
		{
			if(!m_ConnectButton)
				return;
			switch (newStatus)
			{
				case InworldConnectionStatus.Idle:
				case InworldConnectionStatus.Initialized:
					_SetButtonStatus(true, "CONNECT");
					break;
				default:
					_SetButtonStatus(false);
					break;
			}
			if (!m_Status)
				return;
			m_Status.text = newStatus.ToString();
		}
	}
}

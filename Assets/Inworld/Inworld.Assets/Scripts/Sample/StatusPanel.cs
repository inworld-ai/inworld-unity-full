/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Packet;
using TMPro;
using UnityEngine;

namespace Inworld.Sample
{
	public class StatusPanel: MonoBehaviour
	{
		[SerializeField] protected GameObject m_Board;
		[SerializeField] protected PlayerCanvas m_ErrorBoard;
		[SerializeField] protected TMP_Text m_Indicator;
		[SerializeField] protected TMP_Text m_Error;
		[SerializeField] protected GameObject m_NoMic;
		
		protected virtual void OnEnable()
		{
			InworldController.Audio.Event.onStartCalibrating.AddListener(() => SwitchMic(true));
			InworldController.Audio.Event.onStopCalibrating.AddListener(() => SwitchMic(false));
			InworldController.Client.OnErrorReceived += OnErrorReceived;
			InworldController.Client.OnStatusChanged += OnStatusChanged;
		}

		protected virtual void OnDisable()
		{
			if (!InworldController.Instance)
				return;
			InworldController.Client.OnErrorReceived -= OnErrorReceived;
			InworldController.Client.OnStatusChanged -= OnStatusChanged;
		}
		protected virtual void OnErrorReceived(InworldError error)
		{
			if (m_ErrorBoard)
				m_ErrorBoard.Open();
			if (!m_Error)
				return;
			m_Error.gameObject.SetActive(true);
			m_Error.text = error.message;
		}
		protected virtual void SwitchMic(bool isOn)
		{
			if (m_NoMic)
				m_NoMic.SetActive(isOn);
		}
		protected virtual void OnStatusChanged(InworldConnectionStatus incomingStatus)
		{
			bool hidePanel = incomingStatus == InworldConnectionStatus.Idle && !InworldController.HasError || incomingStatus == InworldConnectionStatus.Connected;
			if (m_Board)
				m_Board.SetActive(!hidePanel);
			if (m_Indicator)
				m_Indicator.text = incomingStatus.ToString();
			if (m_Error && incomingStatus == InworldConnectionStatus.Error)
				m_Error.text = InworldController.Client.ErrorMessage;
		}
	}
}

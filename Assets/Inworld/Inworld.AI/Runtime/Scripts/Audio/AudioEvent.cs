/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using UnityEngine.Events;


namespace Inworld.Audio
{
	[Serializable]
	public class AudioEvent
	{
		public UnityEvent onStartCalibrating;
		public UnityEvent onStopCalibrating;
		public UnityEvent onRecordingStart;
		public UnityEvent onRecordingEnd;
		public UnityEvent onPlayerStartSpeaking;
		public UnityEvent onPlayerStopSpeaking;
		
		public void RemoveAllEvents()
		{
			onStartCalibrating.RemoveAllListeners();
			onStopCalibrating.RemoveAllListeners();
			onRecordingStart.RemoveAllListeners();
			onRecordingEnd.RemoveAllListeners();
			onPlayerStartSpeaking.RemoveAllListeners();
			onPlayerStopSpeaking.RemoveAllListeners();
		}
	}
}

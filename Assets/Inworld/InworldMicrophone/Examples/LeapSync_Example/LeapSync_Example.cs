/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace Inworld.Audio
{
	public class LeapSync_Example : MonoBehaviour
	{
#if !UNITY_WEBGL || UNITY_EDITOR
		public AudioSource _audioSource;
#endif
		public Button startRecord,
					  stopRecord;

		private int _sampleRate = 44100;

		private int _recordingTime = 1;


		private void Start()
		{
			startRecord.onClick.AddListener(StartRecordHandler);
			stopRecord.onClick.AddListener(StopRecordHandler);
			startRecord.interactable = true;
			stopRecord.interactable = false;

            InworldMicrophone.PermissionChangedEvent += PermissionChangedEvent;
        }

        private void OnDestroy()
        {
            InworldMicrophone.PermissionChangedEvent -= PermissionChangedEvent;
        }

        private void PermissionChangedEvent(bool granted)
        {
            Debug.Log($"Permission state changed on: {granted}");
        }

        private void StartRecordHandler()
        {
            if (InworldMicrophone.devices.Length == 0)
                return;
#if UNITY_WEBGL && !UNITY_EDITOR
			// for webgl we use native audio speaker instead unity audio source due to limitation of unity audio engine
            InworldMicrophone.Start(InworldMicrophone.devices[0], true, _recordingTime, _sampleRate, true);
#else
			_audioSource.clip = InworldMicrophone.Start(InworldMicrophone.devices[0], true, _recordingTime, _sampleRate);
            _audioSource.loop = true;
            _audioSource.Play();
#endif
            startRecord.interactable = false;
			stopRecord.interactable = true;
		}

		private void StopRecordHandler()
		{
			if (InworldMicrophone.devices.Length == 0)
				return;

            InworldMicrophone.End(InworldMicrophone.devices[0]);
#if !UNITY_WEBGL || UNITY_EDITOR
            _audioSource.Stop();
#endif
			startRecord.interactable = true;
			stopRecord.interactable = false;
		}
	}
}
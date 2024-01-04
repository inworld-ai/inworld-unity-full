using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

using Microphone = FrostweepGames.MicrophonePro.Microphone;

namespace FrostweepGames.MicrophonePro.Examples
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

            Microphone.PermissionChangedEvent += PermissionChangedEvent;
        }

        private void OnDestroy()
        {
            Microphone.PermissionChangedEvent -= PermissionChangedEvent;
        }

        private void PermissionChangedEvent(bool granted)
        {
            Debug.Log($"Permission state changed on: {granted}");
        }

        private void StartRecordHandler()
        {
            if (Microphone.devices.Length == 0)
                return;
#if UNITY_WEBGL && !UNITY_EDITOR
			// for webgl we use native audio speaker instead unity audio source due to limitation of unity audio engine
            Microphone.Start(Microphone.devices[0], true, _recordingTime, _sampleRate, true);
#else
			_audioSource.clip = Microphone.Start(Microphone.devices[0], true, _recordingTime, _sampleRate);
            _audioSource.loop = true;
            _audioSource.Play();
#endif
            startRecord.interactable = false;
			stopRecord.interactable = true;
		}

		private void StopRecordHandler()
		{
			if (Microphone.devices.Length == 0)
				return;

            Microphone.End(Microphone.devices[0]);
#if !UNITY_WEBGL || UNITY_EDITOR
            _audioSource.Stop();
#endif
			startRecord.interactable = true;
			stopRecord.interactable = false;
		}
	}
}
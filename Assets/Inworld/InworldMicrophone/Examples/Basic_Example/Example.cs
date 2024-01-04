using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace FrostweepGames.MicrophonePro.Examples
{
	[RequireComponent(typeof(AudioSource))]
	public class Example : MonoBehaviour
	{

		public Text permissionStatusText;

		public Text recordingStatusText;

		public Dropdown devicesDropdown;

		public AudioSource audioSource;

		public Button startRecordButton,
		              stopRecordButton,
		              playRecordedAudioButton,
		              requestPermissionButton,
		              refreshDevicesButton;

		public List<AudioClip> recordedClips;

		public int frequency = 44100;

		public int recordingTime = 120;

		public string selectedDevice;

		public bool permissionGranted;
		AudioClip _workingClip;

		void Start()
		{
			audioSource = GetComponent<AudioSource>();
			Debug.Log("YAN Start!!");
			startRecordButton.onClick.AddListener(StartRecord);
			stopRecordButton.onClick.AddListener(StopRecord);
			playRecordedAudioButton.onClick.AddListener(PlayRecordedAudio);
			requestPermissionButton.onClick.AddListener(RequestPermission);
			refreshDevicesButton.onClick.AddListener(RefreshMicrophoneDevicesButtonOnclickHandler);

			devicesDropdown.onValueChanged.AddListener(DevicesDropdownValueChangedHandler);

			selectedDevice = string.Empty;

			Microphone.RecordStreamDataEvent += RecordStreamDataEventHandler;
			Microphone.PermissionChangedEvent += PermissionChangedEvent;

			// no need to request permission in webgl. it does automatically
			requestPermissionButton.interactable = Application.platform != RuntimePlatform.WebGLPlayer;
		}

		void Update()
		{
			permissionStatusText.text = $"Microphone permission for device: '{selectedDevice}' is '{(permissionGranted ? "<color=green>granted</color>" : "<color=red>denined</color>")}'";
			recordingStatusText.text = $"Recording status is '{(Microphone.IsRecording(selectedDevice) ? "<color=green>recording</color>" : "<color=yellow>idle</color>")}'";
		}

		void OnDestroy()
		{
			Microphone.RecordStreamDataEvent -= RecordStreamDataEventHandler;
			Microphone.PermissionChangedEvent -= PermissionChangedEvent;
		}

        /// <summary>
        ///     Works only in WebGL
        /// </summary>
        /// <param name="samples"></param>
        void RecordStreamDataEventHandler(Microphone.StreamData streamData)
		{
			// handle streaming recording data
		}

		void PermissionChangedEvent(bool granted)
		{
			// handle current permission status

			if (permissionGranted != granted)
				RefreshMicrophoneDevicesButtonOnclickHandler();

			permissionGranted = granted;

			Debug.Log($"Permission state changed on: {granted}");
		}

		void RefreshMicrophoneDevicesButtonOnclickHandler()
		{
			devicesDropdown.ClearOptions();
			devicesDropdown.AddOptions(Microphone.devices.ToList());
			DevicesDropdownValueChangedHandler(0);
		}

		void RequestPermission()
		{
			Microphone.RequestPermission();
		}

		void StartRecord()
		{
			_workingClip = Microphone.Start(selectedDevice, false, recordingTime, frequency);
		}

		void StopRecord()
		{
			Microphone.End(selectedDevice);

			PlayRecordedAudio();
		}

		void PlayRecordedAudio()
		{
			if (_workingClip == null)
				return;

			audioSource.clip = _workingClip;
			audioSource.Play();

			Debug.Log("start playing");
		}

		void DevicesDropdownValueChangedHandler(int index)
		{
			if (index < Microphone.devices.Length)
			{
				selectedDevice = Microphone.devices[index];
			}
		}
	}
}

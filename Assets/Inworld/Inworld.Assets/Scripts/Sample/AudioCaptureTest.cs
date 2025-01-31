/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Inworld.Sample
{
    public class AudioCaptureTest : MonoBehaviour
    {
        [SerializeField] InworldAudioManager m_Audio;
        [SerializeField] PlayerVoiceDetector m_VolumeDetector;
        [SerializeField] TMP_Dropdown m_Dropdown;
        [SerializeField] TMP_Text m_Text;
        [SerializeField] Image m_Volume;
        [SerializeField] Button m_MicButton;
        [SerializeField] Button m_CalibButton;
        [SerializeField] Sprite m_MicOn;
        [SerializeField] Sprite m_MicOff;
        
        IMicrophoneHandler m_AudioCapturer;
        List<string> m_Devices = new List<string>();
        
        /// <summary>
        /// Change the current input device from the selection of drop down field.
        /// </summary>
        /// <param name="nIndex">the index of the audio input devices.</param>
        public void UpdateAudioInput(int nIndex)
        {
            int nDeviceIndex = nIndex - 1;
            if (nDeviceIndex < 0)
            {
                if (m_Text)
                    m_Text.text = "Please Choose Input Device!";
                return;
            }
            m_AudioCapturer?.ChangeInputDevice(m_Devices[nIndex]);
            if (m_CalibButton)
                m_CalibButton.interactable = true;
            if (!m_MicButton)
                return;
            m_MicButton.interactable = true;
            m_MicButton.image.sprite = m_MicOff;
        }
        /// <summary>
        /// Mute/Unmute microphone.
        /// </summary>
        public void SwitchMicrophone()
        {
            if (!m_MicButton || !m_MicButton.interactable)
                return;
            if (m_MicButton.image.sprite == m_MicOff)
            {
                m_MicButton.image.sprite = m_MicOn;
                m_AudioCapturer?.StopMicrophone();
            }
            else
            {
                m_MicButton.image.sprite = m_MicOff;
                m_AudioCapturer?.StartMicrophone();
            }
        }

        public void Calibrate()
        {
            if (m_Audio)
                m_Audio.StartCalibrate();
        }

        protected void Awake()
        {
            if (m_Audio)
                m_AudioCapturer = m_Audio.GetModule<IMicrophoneHandler>();
            _InitUI();
        }
        
        void OnEnable()
        {
            if (!m_Audio)
                return;
            m_Audio.Event.onStartCalibrating.AddListener(()=>Title("Calibrating"));
            m_Audio.Event.onStopCalibrating.AddListener(()=>Title("Calibrated"));
            m_Audio.Event.onPlayerStartSpeaking.AddListener(()=>Title("PlayerSpeaking"));
            m_Audio.Event.onPlayerStopSpeaking.AddListener(()=>Title(""));
        }

        void Title(string newText)
        {
            if (m_Text)
                m_Text.text = newText;
        }

        void Update()
        {
            if (m_Volume && m_VolumeDetector)
                m_Volume.fillAmount = m_VolumeDetector.CalculateSNR() * 0.05f;
        }

        void _InitUI()
        {
            if (m_AudioCapturer != null)
                m_Devices = m_AudioCapturer.ListMicDevices();
            if (!m_Dropdown)
                return;
            if (m_Dropdown.options == null)
                m_Dropdown.options = new List<TMP_Dropdown.OptionData>();
            m_Dropdown.options.Clear();
            m_Dropdown.options.Add(new TMP_Dropdown.OptionData("--- CHOOSE YOUR DEVICE ---"));
            foreach (string device in m_Devices)
            {
                m_Dropdown.options.Add(new TMP_Dropdown.OptionData(device));
            }
        }
    }
}


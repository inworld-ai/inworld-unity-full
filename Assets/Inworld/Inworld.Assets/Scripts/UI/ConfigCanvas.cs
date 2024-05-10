/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Inworld.Assets
{
    public class ConfigCanvas : MonoBehaviour
    {
        [SerializeField] Slider m_VolumeSlider;
        [SerializeField] TMP_Text m_VolumeValue;
        [SerializeField] TMP_InputField m_PlayerNameField;
        [SerializeField] TMP_InputField m_SceneField;
        [SerializeField] Toggle m_Emotion;
        [SerializeField] Toggle m_Relation;
        [SerializeField] Toggle m_Narrative;
        [SerializeField] Toggle m_Lipsync;
        [SerializeField] GameObject m_NotConnectedPanel;

        string m_CurrentSceneName;
        string m_CurrentPlayerName;

        Capabilities m_Capabilities;
        void OnEnable()
        {
            if (m_VolumeSlider)
                m_VolumeSlider.value = InworldController.Audio.Volume * 100;
            if (m_VolumeValue)
                m_VolumeValue.text = m_VolumeSlider.value.ToString(CultureInfo.InvariantCulture);
            if (string.IsNullOrEmpty(m_CurrentPlayerName))
                m_CurrentPlayerName = InworldAI.User.Name;
            if (m_PlayerNameField)
                m_PlayerNameField.text = m_CurrentPlayerName;
            if (string.IsNullOrEmpty(m_CurrentSceneName))
                m_CurrentSceneName = InworldController.Client.CurrentScene;
            if (m_SceneField)
                m_SceneField.text = m_CurrentSceneName;
            m_Capabilities = new Capabilities(InworldAI.Capabilities);
            m_NotConnectedPanel.SetActive(InworldController.Client.Status != InworldConnectionStatus.Connected); 
        }
        public void SetAudioVolume(Single volume)
        {
            if (m_VolumeValue)
                m_VolumeValue.text = m_VolumeSlider.value.ToString(CultureInfo.InvariantCulture);
            InworldController.Audio.Volume = volume * 0.01f;
        }
        public void ApplyChanges()
        {
            if (InworldController.Client.Status != InworldConnectionStatus.Connected)
                return;
            _SendPlayerChangeRequest();
            _SendSceneChangeRequest();
            _SendCapabilityChangeRequest();
        }

        void _SendPlayerChangeRequest()
        {
            if (m_CurrentPlayerName == m_PlayerNameField.text)
                return;
            InworldAI.User.Name = m_PlayerNameField.text;
            InworldController.Client.SendUserConfig();
            m_CurrentPlayerName = InworldAI.User.Name;
        }
        void _SendSceneChangeRequest()
        {
            if (m_CurrentSceneName == m_SceneField.text)
                return;
            InworldController.Instance.StopAudio();
            InworldController.Client.LoadScene(m_SceneField.text);
            m_CurrentSceneName = m_SceneField.text;
        }
        void _SendCapabilityChangeRequest()
        {
            if (m_Emotion)
                m_Capabilities.emotions = m_Emotion.isOn;
            if (m_Relation)
                m_Capabilities.relations = m_Relation.isOn;
            if (m_Narrative)
                m_Capabilities.narratedActions = m_Narrative.isOn;
            if (m_Lipsync)
                m_Capabilities.phonemeInfo = m_Lipsync.isOn;
            InworldAI.Capabilities = m_Capabilities;
            InworldController.Client.SendCapabilities();
        }
    }
}

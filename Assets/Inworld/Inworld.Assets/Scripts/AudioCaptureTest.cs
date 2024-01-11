/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inworld.Sample
{
    public class AudioCaptureTest : AudioCapture
{
    [SerializeField] TMP_Dropdown m_Dropdown;
    [SerializeField] TMP_Text m_Text;
    [SerializeField] Image m_Volume;
    [SerializeField] Button m_Button;
    [SerializeField] Sprite m_MicOn;
    [SerializeField] Sprite m_MicOff;
    
    /// <summary>
    /// Change the current input device from the selection of drop down field.
    /// </summary>
    /// <param name="nIndex">the index of the audio input devices.</param>
    public void UpdateAudioInput(int nIndex)
    {
#if !UNITY_WEBGL
        int nDeviceIndex = nIndex - 1;
        if (nDeviceIndex < 0)
        {
            m_Text.text = "Please Choose Input Device!";
            return;
        }
        ChangeInputDevice(Microphone.devices[nDeviceIndex]);
        StartRecording();
        m_Button.interactable = true;
        m_Button.image.sprite = m_MicOff;
#endif
    }
    /// <summary>
    /// Mute/Unmute microphone.
    /// </summary>
    public void SwitchMicrophone()
    {
        if (!m_Button.interactable)
            return;
        if (m_Button.image.sprite == m_MicOff)
        {
            StopRecording();
            m_Button.image.sprite = m_MicOn;
        }
        else
        {
            StartRecording();
            m_Button.image.sprite = m_MicOff;
        }
    }
    protected override void Awake()
    {
        base.Awake();
        _InitUI();
    }
    protected override void OnEnable()
    {
        m_AudioCoroutine = AudioCoroutine();
        StartCoroutine(m_AudioCoroutine);
    }
    void _InitUI()
    {
#if !UNITY_WEBGL
        string[] devices = Microphone.devices;
        if (m_Dropdown.options == null)
            m_Dropdown.options = new List<TMP_Dropdown.OptionData>();
        m_Dropdown.options.Clear();
        m_Dropdown.options.Add(new TMP_Dropdown.OptionData("--- CHOOSE YOUR DEVICE ---"));
        foreach (string device in devices)
        {
            m_Dropdown.options.Add(new TMP_Dropdown.OptionData(device));
        }
#endif
    }
    

    protected override IEnumerator Collect()
    {
#if !UNITY_WEBGL
        int nSize = GetAudioData();
        m_Volume.fillAmount = m_InputBuffer.Max() * 5f;
#endif
        yield return new WaitForSeconds(0.1f);
    }
}
}


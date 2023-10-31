using Inworld;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AudioCaptureTest : AudioCapture
{
    [SerializeField] TMP_Dropdown m_Dropdown;
    [SerializeField] TMP_Text m_Text;
    [SerializeField] Image m_Volume;
    [SerializeField] Button m_Button;
    [SerializeField] Sprite m_MicOn;
    [SerializeField] Sprite m_MicOff;
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        _InitUI();
    }

    void _InitUI()
    {
        string[] devices = Microphone.devices;
        m_Dropdown.options ??= new List<TMP_Dropdown.OptionData>();
        m_Dropdown.options.Clear();
        m_Dropdown.options.Add(new TMP_Dropdown.OptionData("--- CHOOSE YOUR DEVICE ---"));
        foreach (string device in devices)
        {
            m_Dropdown.options.Add(new TMP_Dropdown.OptionData(device));
        }
    }
    protected override void Update()
    {
        if (!IsCapturing)
            return;
        if (!Microphone.IsRecording(m_CurrentDevice))
            StartRecording();
        Collect();
    }
    public void UpdateAudioInput(int nIndex)
    {
        int nDeviceIndex = nIndex - 1;
        if (nDeviceIndex < 0)
        {
            m_Text.text = "Please Choose Input Device!";
            return;
        }
        string newDevice = Microphone.devices[nDeviceIndex];
        m_Text.text = newDevice;
        SwitchAudioInputDevice(newDevice);
        StartRecording();
        m_Button.interactable = true;
        m_Button.image.sprite = m_MicOn;
    }
    protected override void Collect()
    {
        int nSize = GetAudioData();
        m_Volume.fillAmount = m_FloatBuffer.Max() * 5f;
    }

    public void SwitchMicrophone()
    {
        if (!m_Button.interactable)
            return;
        if (m_Button.image.sprite == m_MicOff)
        {
	        StartRecording();
            m_Button.image.sprite = m_MicOn;
        }
        else
        {
	        StopRecording();
            m_Button.image.sprite = m_MicOff;
        }
    }
}

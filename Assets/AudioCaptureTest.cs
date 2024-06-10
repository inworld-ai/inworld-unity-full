using Inworld;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCaptureTest : MonoBehaviour
{
    float[] m_OutputBuffer = new float[512];
    protected const int k_SampleRate = 16000;
    protected const int k_Channel = 1;
    int m_OutputChannels = k_Channel;
    int m_OutputSampleRate = k_SampleRate;
    List<short> m_DebugOutput = new List<short>();
    // Start is called before the first frame update
    void Start()
    {
        AudioConfiguration audioSetting = AudioSettings.GetConfiguration();
        m_OutputSampleRate = audioSetting.sampleRate;
        m_OutputChannels = audioSetting.speakerMode == AudioSpeakerMode.Stereo ? 2 : 1;
    }
    float[] Resample(float[] inputSamples) 
    {
        int nResampleRatio = m_OutputSampleRate / k_SampleRate;
        if (nResampleRatio == 1)
            return inputSamples;
        int nTargetLength = inputSamples.Length / nResampleRatio;

        float[] resamples = new float[nTargetLength];

        for (int i = 0; i < nTargetLength; i++)
        {
            int index = i * nResampleRatio;
            resamples[i] = inputSamples[index];
        }
        return resamples;
    }
    // Update is called once per frame
    void Update()
    {
        AudioListener.GetOutputData(m_OutputBuffer, 0);
        float[] resampledBuffer = Resample(m_OutputBuffer);
        short[] outputBuffer = WavUtility.ConvertAudioClipDataToInt16Array(resampledBuffer, resampledBuffer.Length); 
        m_DebugOutput.AddRange(outputBuffer);
        if (Input.GetKeyUp(KeyCode.P))
        {
            WavUtility.ShortArrayToWavFile(m_DebugOutput.ToArray(), "DebugOutput.wav");
            m_DebugOutput.Clear();
        }
    }
}

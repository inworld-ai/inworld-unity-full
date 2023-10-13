using Inworld;
using Inworld.AEC;
using System;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;


public class InworldAECAudioCapture : AudioCapture
{
    const int k_NumSamples = 160;
    IntPtr m_AECHandle;

    float[] m_CharacterBuffer;
    short[] m_CurrentPlayingWavData;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (m_AECHandle == IntPtr.Zero)
            return;
        AECInterop.WebRtcAec3_Free(m_AECHandle);
        m_AECHandle = IntPtr.Zero;
    }
    protected override void Init()
    {
        m_AECHandle = AECInterop.WebRtcAec3_Create(m_AudioRate);
        m_CurrentPlayingWavData = new short[m_AudioRate];
        base.Init();
    }

    public override void SamplePlayingWavData(float[] data, int channels)
    {
        m_CurrentPlayingWavData = WavUtility.ConvertAudioClipDataToInt16Array(data, data.Length);
    }
    protected override byte[] Output(int nSize)
    {
        short[] shortBuffer = WavUtility.ConvertAudioClipDataToInt16Array(m_InputBuffer, nSize * m_Recording.channels);
        return FilterAudio(shortBuffer, m_CurrentPlayingWavData, m_AECHandle);
    }
    public byte[] FilterAudio(short[] inputData, short[] outputData, IntPtr aecHandle)
    {
        short[] filteredAudio = new short[inputData.Length]; // Create a new array for filtered audio
        if (outputData == null || outputData.Length == 0 || outputData.Average(x => Mathf.Abs(x)) == 0)
        {
            filteredAudio = inputData;
        }
        else
        {
            int maxSamples = Math.Min(inputData.Length, outputData.Length) / k_NumSamples * k_NumSamples;
            for (int i = 0; i < maxSamples; i += k_NumSamples)
            {
                AECInterop.WebRtcAec3_BufferFarend(aecHandle, outputData.Skip(i).Take(k_NumSamples).ToArray());
                AECInterop.WebRtcAec3_Process(aecHandle, inputData.Skip(i).Take(k_NumSamples).ToArray(), filteredAudio.Skip(i).ToArray());
            }
        }
        byte[] byteArray = new byte[filteredAudio.Length * 2]; // Each short is 2 bytes
        Buffer.BlockCopy(filteredAudio, 0, byteArray, 0, filteredAudio.Length * 2);
        return byteArray;
    }
}

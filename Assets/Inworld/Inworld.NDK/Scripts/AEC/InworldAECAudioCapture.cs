using Inworld;
using Inworld.AEC;
using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class InworldAECAudioCapture : AudioCapture
{
    const int k_NumSamples = 160;
    SharedAudioData m_SharedAudioData;
    IntPtr m_AECHandle;
    Stopwatch m_Stopwatch;

    float[] m_CharacterBuffer;
    short[] m_CurrentPlayingWavData;

    void OnDestroy()
    {
        if (m_AECHandle == IntPtr.Zero)
            return;
        AECInterop.WebRtcAec3_Free(m_AECHandle);
        m_AECHandle = IntPtr.Zero;
    }
    protected override void Init()
    {
        m_AECHandle = AECInterop.WebRtcAec3_Create(m_AudioRate);
        m_CurrentPlayingWavData = new short[m_AudioRate];
        m_SharedAudioData = new SharedAudioData();
        m_Stopwatch = Stopwatch.StartNew();
        base.Init();
        m_ByteBuffer = new byte[m_BufferSize * 1 * k_SizeofInt32];
    }

    public override void SamplePlayingWavData(float[] data, int channels)
    {
        m_SharedAudioData.Add(data, m_Stopwatch.ElapsedMilliseconds * 0.001f);
        m_CurrentPlayingWavData = m_SharedAudioData.GetDataAsMixedShortArray(); 
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
            filteredAudio = inputData;
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

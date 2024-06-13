using UnityEngine;
using CSCore;
using CSCore.SoundIn;
using CSCore.Codecs.WAV;
using CSCore.CoreAudioAPI;
using CSCore.DSP;
using CSCore.Streams;
using Inworld;
using Inworld.AEC;
using System;
using System.Collections.Generic;
using System.Linq;


public class SoundCapture : MonoBehaviour
{
    WasapiCapture inputCapture;
    WasapiCapture outputCapture;
    WaveWriter inputWriter;
    WaveWriter outputWriter;
    WaveWriter filterWriter;
    byte[] inputBuffer;
    byte[] outputBuffer;
    byte[] filterBuffer;
    DmoResampler inputDmoResampler;
    DmoResampler outputDmoResampler;
    IntPtr m_AECHandle;
    WaveFormat targetFormat = new WaveFormat(16000, 16, 1);
    public bool isDebug = false;

    List<short> inputData = new List<short>();
    List<short> outputData = new List<short>();
    List<short> filterData = new List<short>();
    void Start()
    {
        inputCapture = new WasapiCapture();
        MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
        MMDevice device = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
        m_AECHandle = AECInterop.WebRtcAec3_Create(16000);
        Debug.Log(device.FriendlyName);
        //inputCapture.Device = device; 
        inputCapture.Initialize();
        // This uses the wasapi api to get any sound data played by the computer
        outputCapture = new WasapiLoopbackCapture();
        outputCapture.Initialize();

        if (isDebug)
        {
            inputWriter = new WaveWriter("savedInput.wav", targetFormat);
            outputWriter = new WaveWriter("savedOutput.wav", targetFormat);
            filterWriter = new WaveWriter("savedFilter.wav", targetFormat);
        }
        inputCapture.DataAvailable += (s, e) =>
        {
            int nRatio = inputCapture.WaveFormat.BytesPerSecond / inputDmoResampler.WaveFormat.BytesPerSecond;

            inputBuffer = new byte[e.ByteCount / nRatio];
            inputDmoResampler.Read(inputBuffer, 0, inputBuffer.Length);
            inputData.AddRange(ConvertByteArrayToShortArray(inputBuffer));
            if (inputWriter != null)
            {
                inputWriter.Write(inputBuffer, 0, inputBuffer.Length);
            }
        };
        inputCapture.Stopped += (s, e) =>
        {
            inputWriter?.Dispose();
            inputWriter = null;
            inputDmoResampler?.Dispose();
        };
        outputCapture.DataAvailable += (s, e) =>
        {
            int nRatio = outputCapture.WaveFormat.BytesPerSecond / outputDmoResampler.WaveFormat.BytesPerSecond;
        
            outputBuffer = new byte[e.ByteCount / nRatio];
            outputDmoResampler.Read(outputBuffer, 0, outputBuffer.Length);
            outputData.AddRange(ConvertByteArrayToShortArray(outputBuffer));
            if (outputWriter != null)
            {
                outputWriter.Write(outputBuffer, 0, outputBuffer.Length);
            }
        };
        // 定义录音停止后的处理
        outputCapture.Stopped += (sender, e) =>
        {
            outputWriter?.Dispose();
            outputWriter = null;
            outputDmoResampler?.Dispose();
        };
        IWaveSource inputSource = new SoundInSource(inputCapture);
        IWaveSource outputSource = new SoundInSource(outputCapture);
        inputDmoResampler = new DmoResampler(inputSource, targetFormat);
        outputDmoResampler = new DmoResampler(outputSource, targetFormat);
        inputCapture.Start();
        outputCapture.Start();
    }

    void Update()
    {
        while (inputData.Count >= 160 && outputData.Count >= 160)
        {
            filterBuffer = FilterAudio(inputData.Take(160).ToArray(), outputData.Take(160).ToArray(), m_AECHandle);
            if (filterBuffer != null)
                filterWriter.Write(filterBuffer, 0, filterBuffer.Length);
            inputData.RemoveRange(0, 160);
            outputData.RemoveRange(0, 160);
        }
    }
    private void OutputCaptureDataAvailable(object sender, DataAvailableEventArgs e)
    {
        int nRatio = outputCapture.WaveFormat.BytesPerSecond / outputDmoResampler.WaveFormat.BytesPerSecond;
       
        byte[] buffer = new byte[e.ByteCount / nRatio];
        outputDmoResampler.Read(buffer, 0, buffer.Length);

        if (outputWriter != null)
        {
            outputWriter.Write(buffer, 0, buffer.Length);
        }
    }

    public short[] ConvertByteArrayToShortArray(byte[] byteArray)
    {
        short[] shortArray = new short[byteArray.Length / 2];
        for (int i = 0; i < shortArray.Length; i++)
        {
            shortArray[i] = BitConverter.ToInt16(byteArray, i * 2);
        }
        return shortArray;
    }
    const int k_NumSamples = 160;
    
    protected byte[] FilterAudio(short[] inputData, short[] outputData, IntPtr aecHandle)
    {
        short[] filterTmp = new short[k_NumSamples];
        AECInterop.WebRtcAec3_BufferFarend(m_AECHandle, outputData);
        AECInterop.WebRtcAec3_Process(m_AECHandle, inputData, filterTmp);
        filterData.AddRange(filterTmp);
        byte[] byteArray = new byte[filterTmp.Length * 2]; // Each short is 2 bytes
        Buffer.BlockCopy(filterTmp, 0, byteArray, 0, filterTmp.Length * 2);
        return byteArray;
    }
    protected byte[] FilterAudioBak(short[] inputData, short[] outputData, IntPtr aecHandle)
    {
        List<short> filterBuffer = new List<short>();
        for (int i = 0; i <= inputData.Length - k_NumSamples && i < outputData.Length - k_NumSamples; i += k_NumSamples)
        {
            short[] inputTmp = new short[k_NumSamples];
            short[] outputTmp = new short[k_NumSamples];
            short[] filterTmp = new short[k_NumSamples];
            Array.Copy(inputData, i, inputTmp, 0, k_NumSamples);
            Array.Copy(outputData, i, outputTmp, 0, k_NumSamples);
            AECInterop.WebRtcAec3_BufferFarend(m_AECHandle, outputTmp);
            AECInterop.WebRtcAec3_Process(m_AECHandle, inputTmp, filterTmp);
            filterBuffer.AddRange(filterTmp);
            filterData.AddRange(filterTmp);
        }

        byte[] byteArray = new byte[filterBuffer.Count * 2]; // Each short is 2 bytes
        Buffer.BlockCopy(filterBuffer.ToArray(), 0, byteArray, 0, filterBuffer.Count * 2);
        return byteArray;
    }
    void OnApplicationQuit()
    {
        if (m_AECHandle == IntPtr.Zero)
            return;
        AECInterop.WebRtcAec3_Free(m_AECHandle);
        m_AECHandle = IntPtr.Zero;
        if (enabled)
        {
            outputCapture.Stop();
            outputCapture.Dispose();
            inputCapture.Stop();
            inputCapture.Dispose();
        }
        WavUtility.ShortArrayToWavFile(inputData.ToArray(),"inputShort.wav");
        WavUtility.ShortArrayToWavFile(outputData.ToArray(),"outputShort.wav");
        WavUtility.ShortArrayToWavFile(filterData.ToArray(),"filterShort.wav");
    }
}

using Inworld.VAD;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocalVAD
{
#region Const Variables
    const int k_SampleRate = 16000;
    const int k_MinSpeechSamples = 1024; // 64 * (16000 / 1000)
    const int k_WindowSizeSamples = 1024; // 64 * (16000 / 1000)
    const int k_MinSilenceSamplesAtMaxSpeech = 16 * 98;
    const int k_SizeHC = 128; 
    readonly long[] k_SrNodeDims = {1};
    readonly long[] k_HCNodeDims = {2,1,64};
    readonly long[] k_InputNodeDims = {1, 1024};
    readonly string[] k_InputNodeNames = {"input", "sr", "h", "c"};
    readonly string[] k_OutputNodeNames = {"output", "hn", "cn"};
#endregion
    float m_Threshold = 0.5f;
    SessionOptions m_SessionOptions;
    InferenceSession m_InferenceSession;

    int m_AudioLengthSamples;
    int m_CurrentSample;
    int m_TempEnd;
    int m_PrevEnd;
    int m_NextStart;
    // model states
    bool m_IsTriggered;
    
    List<float> m_Input = new List<float>();
    float[] m_H = new float[k_SizeHC];
    float[]  m_C = new float[k_SizeHC];
    long[] m_Sr = {k_SampleRate};
    List<OrtValue> m_OrtInputs = new List<OrtValue>();
    TimeStamp m_CurrentSpeech = new TimeStamp();
    float m_Probability;
    void InitEngineThreads(int interThreads, int intraThreads)
    {
        m_SessionOptions = new SessionOptions();
        // The method should be called in each thread/proc in multi-thread/proc work
        m_SessionOptions.InterOpNumThreads = interThreads;
        m_SessionOptions.IntraOpNumThreads = intraThreads;
        m_SessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL; 
    }
    void Predict(List<float> data)
    {
        
        float[] input = data.ToArray();
        OrtValue inputTensor = OrtValue.CreateTensorValueFromMemory(input, k_InputNodeDims);
        long[] sr = m_Sr.ToArray();
        OrtValue srTensor = OrtValue.CreateTensorValueFromMemory(sr, k_SrNodeDims);
        float[] h = m_H.ToArray();
        OrtValue hTensor = OrtValue.CreateTensorValueFromMemory(h, k_HCNodeDims);
        float[] c = m_C.ToArray();
        OrtValue cTensor = OrtValue.CreateTensorValueFromMemory(c, k_HCNodeDims);
        List<OrtValue> inputTensors = new List<OrtValue>() { inputTensor, srTensor, hTensor, cTensor };
        List<OrtValue> outputTensors = new List<OrtValue>()
        {
            OrtValue.CreateTensorValueFromMemory(new float[1], new long[] { 1, 1 }), 
            OrtValue.CreateTensorValueFromMemory(new float[k_SizeHC], k_HCNodeDims),
            OrtValue.CreateTensorValueFromMemory(new float[k_SizeHC], k_HCNodeDims)
        };
        try
        {
            m_InferenceSession.Run(null, k_InputNodeNames, inputTensors, k_OutputNodeNames, outputTensors);
            float speechProb = outputTensors[0].GetTensorDataAsSpan<float>()[0];
            float[] hn = outputTensors[1].GetTensorDataAsSpan<float>().ToArray();
            Array.Copy(hn, m_H, m_H.Length);
            float[] cn = outputTensors[2].GetTensorDataAsSpan<float>().ToArray();
            Array.Copy(cn, m_C, m_C.Length);
            m_Probability = speechProb;
            // Push forward sample index
            m_CurrentSample += k_WindowSizeSamples;
            // Reset temp_end when > threshold 
            if (speechProb >= m_Threshold)
            {
                if (m_TempEnd != 0)
                {
                    m_TempEnd = 0;
                    if (m_NextStart < m_PrevEnd)
                        m_NextStart = m_CurrentSample - k_WindowSizeSamples;
                }
                if (m_IsTriggered)
                    return;
                m_IsTriggered = true;
                m_CurrentSpeech.start = m_CurrentSample - k_WindowSizeSamples;
                return;
            }
            // 4) End 
            if (!(speechProb < m_Threshold - 0.15))
                return;
            if (m_IsTriggered != true)
                return;
            if (m_TempEnd == 0)
            {
                m_TempEnd = m_CurrentSample;
            }
            if (m_CurrentSample - m_TempEnd > k_MinSilenceSamplesAtMaxSpeech)
                m_PrevEnd = m_TempEnd;
            // a. silence < min_slience_samples, continue speaking 
            if (m_CurrentSample - m_TempEnd < 0)
                return;
            m_CurrentSpeech.end = m_TempEnd;
            if (m_CurrentSpeech.end - m_CurrentSpeech.start <= k_MinSpeechSamples)
                return;
            m_CurrentSpeech.Reset();
            m_PrevEnd = 0;
            m_NextStart = 0;
            m_TempEnd = 0;
            m_IsTriggered = false;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
        finally
        {
            foreach (var tensor in inputTensors)
            {
                tensor.Dispose();
            }
            foreach (var tensor in outputTensors)
            {
                tensor.Dispose();
            }
        }
    }
    void InitOnnxModel(OrtAsset model, float threshold)
    {
        m_Threshold = threshold;
        // Init threads = 1 for 
        InitEngineThreads(1, 1);
        
        // Load model
        m_InferenceSession = new InferenceSession(model.bytes, m_SessionOptions);
    }
    public LocalVAD(OrtAsset model, float threshold)
    {
        InitOnnxModel(model, threshold);
        m_Input.Clear();
        m_Input.AddRange(Enumerable.Repeat<float>(0, k_WindowSizeSamples));
    }
    // Call reset before each audio start
    public void ResetStates()
    {
        m_H = new float[k_SizeHC];
        m_C = new float[k_SizeHC];
        m_IsTriggered = false;
        m_TempEnd = 0;
        m_CurrentSample = 0;

        m_PrevEnd = m_NextStart = 0;
        m_CurrentSpeech.Reset();
    }
    public float Process(List<float> inputWav)
    {
        m_AudioLengthSamples = inputWav.Count;
        for (int j = 0; j < m_AudioLengthSamples; j += k_WindowSizeSamples)
        {
            if (j + k_WindowSizeSamples > m_AudioLengthSamples)
                break;
            Predict(inputWav.GetRange(j, k_WindowSizeSamples));
        }
        if (m_CurrentSpeech.start >= 0) 
        {
            m_CurrentSpeech.end = m_AudioLengthSamples;
            m_CurrentSpeech.Reset();
            m_PrevEnd = 0;
            m_NextStart = 0;
            m_TempEnd = 0;
            m_IsTriggered = false;
        }
        return m_Probability;
    }
    public void Dispose()
    {
        m_InferenceSession.Dispose();
    }
}

/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Interactions;
using Inworld.Sample;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Inworld.AEC
{
    public class InworldAECAudioCapture : AudioCapture
    {
        const int k_NumSamples = 80;//160;
        IntPtr m_AECHandle;

        protected float[] m_OutputBuffer;
        
        float[] m_CharacterBuffer;
        List<short> m_CurrentPlayingWavData = new List<short>();
        Dictionary<string, InworldAudioInteraction> m_SoundEnv = new Dictionary<string, InworldAudioInteraction>();
        
        /// <summary>
        /// A flag for this component is using AEC (in this class always True)
        /// </summary>
        public override bool EnableAEC => true;
        /// <summary>
        /// When scene loaded, add the AudioInteraction for each character to get the mixed audio environment.
        /// </summary>
        /// <param name="dataAgentId">the live session id of the character</param>
        /// <param name="interaction">the interaction to attach.</param>
        public override void RegisterLiveSession(string dataAgentId, InworldInteraction interaction)
        {
            if(interaction is InworldAudioInteraction audioInteraction)
                m_SoundEnv[dataAgentId] = audioInteraction;
        }
        List<short> inputArray = new List<short>();
        List<short> outputArray = new List<short>();
        List<short> filterArray = new List<short>();
        
        public static void ConvertShortArrayToWav(short[] audioData, int sampleRate, string filePath)
        {
            using (var memoryStream = new MemoryStream())
                using (var writer = new BinaryWriter(memoryStream))
                {
                    // 写入WAV文件头部信息
                    writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                    writer.Write(44 + audioData.Length * 2); // 文件总大小
                    writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                    writer.Write(Encoding.ASCII.GetBytes("fmt "));
                    writer.Write(16); // fmt子块大小（PCM）
                    writer.Write((short)1); // 音频格式（PCM）
                    writer.Write((short)1); // 声道数量
                    writer.Write(sampleRate); // 采样率
                    writer.Write(sampleRate * 2); // 字节速率=采样率*块对齐单位
                    writer.Write((short)2); // 块对齐单位=声道数量*每个样本的位数/8
                    writer.Write((short)16); // 每个样本的位数
                    writer.Write(Encoding.ASCII.GetBytes("data"));
                    writer.Write(audioData.Length * 2); // data子块大小（字节数）

                    // 写入音频数据
                    foreach (var sample in audioData)
                    {
                        writer.Write(sample);
                    }

                    // 将数据写入文件
                    File.WriteAllBytes(filePath, memoryStream.ToArray());
                }
        }
        public static void WriteShortArraysToCSV(short[] array1, short[] array2, short[] array3, string filePath)
        {
            // 确保三个数组的长度相等
            if (array1.Length != array2.Length || array2.Length != array3.Length)
            {
                Debug.LogError("Arrays are not the same length!");
                return;
            }

            using (var writer = new StreamWriter(filePath))
            {
                // 使用循环遍历数组的索引并写入文件流
                for (int i = 0; i < array1.Length; i++)
                {
                    // 在每行的末尾添加换行符，完成写入操作
                    writer.WriteLine($"{array1[i]},{array2[i]},{array3[i]}");
                }
            }

            Debug.Log("CSV file written to " + filePath);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            // Begin
            ConvertShortArrayToWav(inputArray.ToArray(), 16000, "input.wav");
            ConvertShortArrayToWav(filterArray.ToArray(), 16000, "filter.wav");
            ConvertShortArrayToWav(outputArray.ToArray(), 16000, "output.wav");
            WriteShortArraysToCSV(inputArray.ToArray(), outputArray.ToArray(), filterArray.ToArray(), "report.csv");
            // End
            if (m_AECHandle == IntPtr.Zero)
                return;
            AECInterop.WebRtcAec3_Free(m_AECHandle);
            m_AECHandle = IntPtr.Zero;
        }
        protected override void Init()
        {
            m_AECHandle = AECInterop.WebRtcAec3_Create(k_SampleRate);
            m_CurrentPlayingWavData = new List<short>();
            base.Init();
            m_OutputBuffer = new float[m_BufferSize * 1];
            AudioSettings.outputSampleRate = 16000;
            AudioSettings.speakerMode = AudioSpeakerMode.Mono;
        }
        protected override void Collect()
        {
#if !UNITY_WEBGL
            int nPosition = Microphone.GetPosition(null);
            if (nPosition < m_LastPosition)
                nPosition = m_BufferSize;
            if (nPosition <= m_LastPosition)
                return;
            int nSize = nPosition - m_LastPosition;
            if (!m_Recording.GetData(m_InputBuffer, m_LastPosition))
                return;
            m_OutputBuffer = new float[nSize];
            AudioListener.GetOutputData(m_OutputBuffer, 0);

            m_LastPosition = nPosition % m_BufferSize;
            short[] inputBuffer = WavUtility.ConvertAudioClipDataToInt16Array(m_InputBuffer, nSize * m_Recording.channels);
            short[] outputBuffer = WavUtility.ConvertAudioClipDataToInt16Array(m_OutputBuffer, nSize * m_Recording.channels);
            InworldController.Instance.SendAudio(Convert.ToBase64String(FilterAudio(inputBuffer, outputBuffer, m_AECHandle)));
#endif            
        }
        protected override byte[] Output(int nSize)
        {
            short[] inputBuffer = WavUtility.ConvertAudioClipDataToInt16Array(m_InputBuffer, nSize * m_Recording.channels);
            m_CurrentPlayingWavData.Clear();
            // foreach (InworldAudioInteraction audioInteraction in m_SoundEnv.Values)
            // {
            //     _Mix(audioInteraction.GetCurrentAudioFragment());
            // }
            m_OutputBuffer = new float[nSize * m_Recording.channels];
            AudioListener.GetOutputData(m_OutputBuffer, m_Recording.channels);
            short[] outputBuffer = WavUtility.ConvertAudioClipDataToInt16Array(m_OutputBuffer, m_LastPosition * m_Recording.channels);
            return FilterAudio(inputBuffer, outputBuffer, m_AECHandle); /*m_CurrentPlayingWavData.ToArray()*/
        }
        void _Mix(short[] currAudio)
        {
            if (currAudio == null || currAudio.Length == 0)
                return;
            for (int i = 0; i < currAudio.Length; i++)
            {
                if (i < m_CurrentPlayingWavData.Count)
                    m_CurrentPlayingWavData[i] += currAudio[i];
                else
                    m_CurrentPlayingWavData.Add(currAudio[i]);
            }
        }

        protected byte[] FilterAudio(short[] inputData, short[] outputData, IntPtr aecHandle)
        {
            short[] filteredAudio = new short[inputData.Length]; // Create a new array for filtered audio
            if (outputData == null || outputData.Length == 0)// || outputData.Average(x => Mathf.Abs(x)) == 0)
            {
                filteredAudio = inputData;
                byte[] byteArray = new byte[filteredAudio.Length * 2]; // Each short is 2 bytes
                Buffer.BlockCopy(filteredAudio, 0, byteArray, 0, filteredAudio.Length * 2);
                return byteArray;
            }
            else
            {
                filteredAudio = inputData;
                inputArray.AddRange(inputData);
                outputArray.AddRange(outputData);
                int maxSamples = Math.Min(inputData.Length, outputData.Length) / 160 * 160;
                short[] tmp = new short[160];
                for (int i = 0; i < maxSamples; i += 160)
                {
                    AECInterop.WebRtcAec3_BufferFarend(aecHandle, outputData.Skip(i).Take(160).ToArray());
                    AECInterop.WebRtcAec3_Process(aecHandle, inputData.Skip(i).Take(160).ToArray(), tmp);
                    Buffer.BlockCopy(tmp, 0, filteredAudio, i, 160);
                }
                filterArray.AddRange(filteredAudio);
                byte[] byteArray = new byte[filteredAudio.Length * 2]; // Each short is 2 bytes
                Buffer.BlockCopy(filteredAudio, 0, byteArray, 0, filteredAudio.Length * 2);
                return byteArray;
            }
        }
    }
}


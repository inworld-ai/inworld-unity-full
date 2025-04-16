/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Inworld.Packet
{
    [Serializable]
    public class PhonemeList
    {
        public List<PhonemeInfo> phonemes;
    }
    [Serializable]
    public class PhonemeInfo
    {
        public string phoneme;
        public string startOffset;

        [JsonIgnore]
        public float StartOffset 
        {
             get
             {
                 if (float.TryParse(startOffset.TrimEnd('s', 'S'), out float result))
                     return result;
                 return 0;
             }
        }
    }

    [Serializable]
    public sealed class AudioPacket : InworldPacket
    {
        public DataChunk dataChunk;
        
        public AudioPacket()
        {
            dataChunk = new DataChunk
            {
                type = DataType.AUDIO
            };
        }
        public AudioPacket(DataChunk chunk)
        {
            dataChunk = chunk;
            PreProcess();
        }
        public AudioPacket(InworldPacket rhs, DataChunk chunk) : base(rhs)
        {
            dataChunk = chunk;
        }
        /// <summary>
        /// Dump received audio packet and phoneme Info to local files.
        /// </summary>
        /// <param name="fileName">the filename to be saved.</param>
        public void DumpWaveFile(string fileName)
        {
            byte[] bytes = Convert.FromBase64String(dataChunk.chunk);
            File.WriteAllBytes($"{fileName}.wav", bytes);
            PhonemeList phonemes = new PhonemeList
            {
                phonemes = dataChunk.additionalPhonemeInfo
            };
            string phoneme = JsonUtility.ToJson(phonemes);
            File.WriteAllText($"{fileName}.json", phoneme);
        }
        [JsonIgnore]
        public AudioClip Clip
        {
            get
            {
                if (dataChunk == null || string.IsNullOrEmpty(dataChunk.chunk))
                    return null;
                try
                {
                    byte[] bytes = Convert.FromBase64String(dataChunk.chunk);
                    return WavUtility.ToAudioClip(bytes);
                }
                catch (Exception)
                {
                    InworldAI.LogError($"Data converting failed. {dataChunk.chunk.Length}");
                    return null;
                }
            }
        }
    }
}
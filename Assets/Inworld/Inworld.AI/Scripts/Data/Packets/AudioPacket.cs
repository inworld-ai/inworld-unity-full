/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;
using UnityEngine;

namespace Inworld.Packet
{
    [Serializable]
    public class PhonemeInfo
    {
        public string phoneme;
        public float startOffset;
    }
    [Serializable]
    public class AudioPacket : InworldPacket
    {
        public DataChunk dataChunk;
        
        public AudioPacket()
        {
            dataChunk = new DataChunk
            {
                type = "AUDIO"
            };
        }
        public AudioPacket(InworldPacket rhs, DataChunk chunk) : base(rhs)
        {
            dataChunk = chunk;
        }

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
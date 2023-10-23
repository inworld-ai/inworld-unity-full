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
            type = "AUDIO";
            dataChunk = new DataChunk
            {
                type = "AUDIO"
            };
        }
        public AudioPacket(InworldPacket rhs, DataChunk chunk) : base(rhs)
        {
            type = "AUDIO";
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

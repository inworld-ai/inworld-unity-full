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
                byte[] bytes = Convert.FromBase64String(dataChunk.chunk);
                AudioClip clip = WavUtility.ToAudioClip(bytes);
                dataChunk.chunk = "";
                return clip;
            }
        }
    }
}

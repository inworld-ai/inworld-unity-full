using System;
using System.Collections.Generic;

namespace Inworld.Packet
{
    [Serializable]
    public class DataChunk
    {
        public string chunk;
        public string type;
        public List<PhonemeInfo> additionalPhonemeInfo;
    }
}

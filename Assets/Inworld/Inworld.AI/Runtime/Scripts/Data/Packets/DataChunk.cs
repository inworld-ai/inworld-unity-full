/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Inworld.Packet
{
    [Serializable]
    public class DataChunk
    {
        public string chunk;
        [JsonConverter(typeof(StringEnumConverter))]
        public DataType type;
        public List<PhonemeInfo> additionalPhonemeInfo;
    }
}

/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
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

/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Inworld.Entities
{
    [Serializable]
    public class AudioDevice
    {
        public string deviceId;
        public string kind;
        public string label;
        public string groupId;
        [Preserve] public AudioDevice() {}
        [Preserve] public AudioDevice(string deviceId, string kind, string label, string groupId)
        {
            this.deviceId = deviceId;
            this.kind = kind;
            this.label = label;
            this.groupId = groupId;
        }
    }
    [Serializable]
    public class WebGLAudioDevicesData
    {
        public List<AudioDevice> devices;
    }
    
    [Serializable]
    public class WebGLAudioDeviceCapsData
    {
        public int[] caps;
    }
}

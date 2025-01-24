/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/


using UnityEngine;

namespace Inworld.Audio.AEC
{
    public enum SignalEnd
    {
        FarEnd,
        NearEnd
    }
    public class AECProbe : MonoBehaviour
    {
        AcousticEchoCanceler m_AECModule;
        public SignalEnd end;
        protected virtual void Awake()
        {
            m_AECModule = InworldController.Audio.GetModule<AcousticEchoCanceler>();
        }

        protected void OnAudioFilterRead(float[] data, int channels)
        {
            m_AECModule?.GetOutputData(end, data, channels);
        }
    }
}
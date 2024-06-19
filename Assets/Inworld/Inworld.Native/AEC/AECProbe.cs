/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/


using UnityEngine;

namespace Inworld.AEC
{
    public class AECProbe : MonoBehaviour
    {
        AudioCapture m_AudioCapture;
        
        public virtual void Init(AudioCapture audioCapture)
        {
            m_AudioCapture = audioCapture;
        }
        protected void OnAudioFilterRead(float[] data, int channels)
        {
            m_AudioCapture.GetOutputData(data, channels);
        }
    }
}
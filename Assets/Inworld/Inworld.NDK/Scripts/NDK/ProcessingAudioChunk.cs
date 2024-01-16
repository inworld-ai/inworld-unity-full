/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
namespace Inworld.NDK
{
    /// <summary>
    /// In NDK, it's difficult to get a whole bunch of array without knowing the data size.
    /// So instead, we need to send API request to fetch the phoneme and triggers one by one.
    ///
    /// This class helped us to compose the phonemes into the audio packet.
    /// </summary>
    public class ProcessingAudioChunk
    {
        readonly Inworld.Packet.AudioPacket m_CurrentAudioPacket;
        readonly int m_PhonemeCount;
        readonly bool m_Initialized;
        bool m_IsPacketPushed;

        public ProcessingAudioChunk()
        {
            
        }
        /// <summary>
        /// Generate the process class via the NDK audio packet.
        /// </summary>
        /// <param name="rhs">the input audio packet of NDK.</param>
        public ProcessingAudioChunk(NDKPacket rhs)
        {
            m_CurrentAudioPacket = InworldNDK.From.NDKAudioChunk(rhs);
            m_PhonemeCount = rhs.audioPacket.phonemeCount;
            m_Initialized = true;
        }
        /// <summary>
        /// Receive the phonemeInfo from NDK.
        /// This function will store the phoneme into the current audio packet's phoneme list. 
        /// </summary>
        /// <param name="phonemeInfo">the phonemeInfo received from NDK.</param>
        public void ReceivePhoneme(PhonemeInfo phonemeInfo)
        {
            if (phonemeInfo.packetID != m_CurrentAudioPacket.packetId.packetId)
                return;
            m_CurrentAudioPacket.dataChunk.additionalPhonemeInfo.Add(new Inworld.Packet.PhonemeInfo
            {
                phoneme = phonemeInfo.code,
                startOffset = phonemeInfo.timeStamp
            });
            if (m_CurrentAudioPacket.dataChunk.additionalPhonemeInfo.Count != m_PhonemeCount)
                return;
            ToInworldPacket();
        }
        /// <summary>
        /// Generate the Unity SDK compatible audio packet by the current given data.
        /// </summary>
        public void ToInworldPacket()
        {
            if (m_IsPacketPushed)
                return;
            if (m_Initialized && InworldController.Client is InworldNDKClient ndkClient)
            {
                ndkClient.Enqueue(m_CurrentAudioPacket);
                m_IsPacketPushed = true;
            }
        }
    }
}

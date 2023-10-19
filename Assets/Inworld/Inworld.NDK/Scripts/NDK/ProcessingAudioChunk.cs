namespace Inworld.NDK
{
    public class ProcessingAudioChunk
    {
        readonly Inworld.Packet.AudioPacket m_CurrentAudioPacket;
        readonly int m_PhonemeCount;
        readonly bool m_Initialized;
        bool m_IsPacketPushed;

        public ProcessingAudioChunk()
        {
            
        }
        public ProcessingAudioChunk(NDKPacket rhs)
        {
            m_CurrentAudioPacket = InworldNDK.From.NDKAudioChunk(rhs);
            m_PhonemeCount = rhs.audioPacket.phonemeCount;
            m_Initialized = true;
        }
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

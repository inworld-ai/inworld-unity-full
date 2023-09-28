using UnityEngine;
using Inworld.Packet;

namespace Inworld
{
    public class PlayerController3D : PlayerController
    {
        [SerializeField] protected GameObject m_ChatCanvas;
        protected override void HandleInput()
        {
            if (Input.GetKeyUp(KeyCode.BackQuote))
            {
                m_ChatCanvas.SetActive(!m_ChatCanvas.activeSelf);
                m_BlockAudioHandling = m_ChatCanvas.activeSelf;
                if (m_PushToTalk)
                   m_CharacterHandler.StopAudio();
                m_AudioCapture.AutoPush = !m_ChatCanvas.activeSelf && !m_PushToTalk;
                m_CharacterHandler.ManualAudioHandling = m_ChatCanvas.activeSelf || m_PushToTalk;
            }
            if (m_ChatCanvas.activeSelf)
                base.HandleInput();
        }

        protected override void HandleTrigger(CustomPacket customPacket)
        {
            InworldAI.Log($"(Received {customPacket.Trigger})");
        }
        protected override void HandleEmotion(EmotionPacket packet) => m_CurrentEmotion = packet.emotion.ToString();
    }
}

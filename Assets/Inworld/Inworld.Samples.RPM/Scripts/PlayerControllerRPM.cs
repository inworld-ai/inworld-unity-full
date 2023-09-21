using Inworld.Packet;
using UnityEngine;


namespace Inworld.Sample.RPM
{
    public class PlayerControllerRPM : PlayerController
    {
        [SerializeField] GameObject m_ChatCanvas;

        protected override void HandleInput()
        {
            if (Input.GetKeyUp(KeyCode.BackQuote))
            {
                m_ChatCanvas.SetActive(!m_ChatCanvas.activeSelf);
                InworldController.Instance.StopAudio();
                AudioCapture.Instance.AutoPush = !m_ChatCanvas.activeSelf;
                m_BlockAudioHandling = m_ChatCanvas.activeSelf;

                if (!m_ChatCanvas.activeSelf)
                    return;
            }
            base.HandleInput();
        }
        
        protected override void OnCharacterRegistered(InworldCharacterData charData)
        {
            
        }

        protected override void HandleTrigger(CustomPacket customPacket)
        {
            InworldAI.Log($"(Received {customPacket.Trigger})");
        }
        protected override void HandleEmotion(EmotionPacket packet) => m_CurrentEmotion = packet.emotion.ToString();
    }
}

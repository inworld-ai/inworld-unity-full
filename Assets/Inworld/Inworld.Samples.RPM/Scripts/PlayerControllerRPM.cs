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
                m_BlockAudioHandling = m_ChatCanvas.activeSelf;
                if (m_PushToTalk)
                {
                    if (InworldController.Instance.CurrentCharacter)
                        InworldController.Instance.StopAudio(InworldController.Instance.CurrentCharacter.ID);
                }
                else
                {
                    if (m_ChatCanvas.activeSelf)
                    {
                        if (InworldController.Instance.CurrentCharacter)
                            InworldController.Instance.StopAudio(InworldController.Instance.CurrentCharacter.ID);
                    }
                    else
                    {
                        if (InworldController.Instance.CurrentCharacter)
                            InworldController.Instance.StartAudio(InworldController.Instance.CurrentCharacter.ID);
                    }
                }

            }
            if (m_ChatCanvas.activeSelf)
            {
                if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
                    SendText();
            }
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

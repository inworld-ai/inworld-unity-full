using Inworld.Packet;
using UnityEngine;


namespace Inworld.Sample.RPM
{
    public class PlayerControllerRPM : PlayerController
    {
        [SerializeField] GameObject m_ChatCanvas;

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.BackQuote))
                m_ChatCanvas.SetActive(!m_ChatCanvas.activeSelf);
            if (!m_ChatCanvas.activeSelf)
                return;
            if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
                SendText();
        }
        protected override void OnStatusChanged(InworldConnectionStatus newStatus)
        {
            if (newStatus == InworldConnectionStatus.Connected && InworldController.Instance.CurrentCharacter)
            {
                if (m_SendButton)
                    m_SendButton.interactable = true;
                if (!InworldController.IsRecording)
                    InworldController.Instance.StartAudio(InworldController.Instance.CurrentCharacter.ID);
            }
            else
            {
                if (m_SendButton)
                    m_SendButton.interactable = false;
                if (InworldController.IsRecording)
                    InworldController.Instance.StopAudio(InworldController.Instance.CurrentCharacter.ID);
            }
            if (m_StatusText)
                m_StatusText.text = newStatus.ToString();
            if (newStatus == InworldConnectionStatus.Error)
                m_StatusText.text = InworldController.Client.Error;
        }
        protected override void OnCharacterRegistered(InworldCharacterData charData)
        {

        }

        protected override void OnCharacterChanged(InworldCharacter oldChar, InworldCharacter newChar)
        {
            m_SendButton.interactable = InworldController.Status == InworldConnectionStatus.Connected && InworldController.Instance.CurrentCharacter;
            if (newChar != null)
                InworldAI.Log($"Now Talking to: {newChar.Name}");
            base.OnCharacterChanged(oldChar, newChar);
        }

        protected override void HandleTrigger(CustomPacket customPacket)
        {
            InworldAI.Log($"(Received {customPacket.Trigger})");
        }
        protected override void HandleEmotion(EmotionPacket packet) => m_CurrentEmotion = packet.emotion.ToString();
    }
}

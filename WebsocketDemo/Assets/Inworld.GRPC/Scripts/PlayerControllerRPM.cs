using Inworld.Packet;
using UnityEngine;


namespace Inworld.Sample
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
            UpdateInteractions();
            
            if (m_StatusText)
                m_StatusText.text = newStatus.ToString();
            if (newStatus == InworldConnectionStatus.Error)
                m_StatusText.text = InworldController.Client.Error;
        }

        void UpdateInteractions()
        {
            if (InworldController.Status == InworldConnectionStatus.Connected && InworldController.Instance.CurrentCharacter)
            {
                m_SendButton.interactable = true;
                if (!InworldController.IsRecording)
                    InworldController.Instance.StartAudio(InworldController.Instance.CurrentCharacter.ID);
            }
            else
            {
                m_SendButton.interactable = false;
                if (InworldController.IsRecording)
                    InworldController.Instance.StopAudio(InworldController.Instance.CurrentCharacter.ID);
            }
        }
        protected override void OnCharacterRegistered(InworldCharacterData charData){}

        protected override void OnCharacterChanged(InworldCharacter oldChar, InworldCharacter newChar)
        {
            m_SendButton.interactable = InworldController.Status == InworldConnectionStatus.Connected && InworldController.Instance.CurrentCharacter;
            if (newChar != null)
            {
                InworldAI.Log($"Current: {newChar.Name}");
                UpdateInteractions();
            }
        }

        protected override void HandleTrigger(CustomPacket customPacket)
        {
            InworldAI.Log($"(Received {customPacket.Trigger})");
        }
        protected override void HandleEmotion(EmotionPacket packet) => m_CurrentEmotion = packet.emotion.ToString();
    }
}
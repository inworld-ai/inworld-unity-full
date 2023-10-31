﻿/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using UnityEngine;
using Inworld.Packet;

namespace Inworld.Sample
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
                    InworldController.Instance.StopAudio();
                InworldController.Audio.AutoPush = !m_ChatCanvas.activeSelf && !m_PushToTalk;
                InworldController.CharacterHandler.ManualAudioHandling = m_ChatCanvas.activeSelf || m_PushToTalk;
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

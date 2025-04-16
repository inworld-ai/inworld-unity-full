/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Inworld.Audio
{
    public class PushToTalkModule : PlayerEventModule
    {
        InputAction m_PushToTalkInputAction;

        void Awake()
        {
            m_PushToTalkInputAction = InworldAI.InputActions["PushToTalk"];
        }

        public override IEnumerator OnPlayerUpdate()
        {
            while (isActiveAndEnabled)
            {
                Audio.IsPlayerSpeaking = m_PushToTalkInputAction.IsPressed();
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
    }
}
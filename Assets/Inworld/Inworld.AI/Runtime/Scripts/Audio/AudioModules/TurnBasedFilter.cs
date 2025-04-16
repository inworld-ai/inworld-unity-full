/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System.Collections;
using UnityEngine;

namespace Inworld.Audio
{
    public class TurnBasedFilter : PlayerEventModule
    {
        public override IEnumerator OnPlayerUpdate()
        {
            while (isActiveAndEnabled)
            {
                Audio.IsPlayerSpeaking = !InworldController.CharacterHandler.IsAnyCharacterSpeaking;
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
    }
}
/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Interactions;
using Inworld.Entities;
using UnityEngine;

namespace Inworld.Sample.RPM
{
    [RequireComponent(typeof(InworldAudioInteraction))]
    public class InworldRPMCharacter : InworldCharacter
    {
        protected override void OnCharRegistered(InworldCharacterData charData)
        {
            if (charData.brainName == Data.brainName)
                RegisterLiveSession();
        }
    }
    
}


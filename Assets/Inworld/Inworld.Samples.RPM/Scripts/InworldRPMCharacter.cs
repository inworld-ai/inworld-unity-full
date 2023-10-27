/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Interactions;
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
        /// <summary>
        /// Register the live session once the load scene request completed.
        /// In this overwritten functioon, it'll also send its audio interaction component to the mixer of the AudioCapture.
        /// </summary>
        public override void RegisterLiveSession()
        {
            base.RegisterLiveSession();
            if (InworldController.Audio && InworldController.Audio.EnableAEC)
            {
                InworldController.Audio.RegisterLiveSession(m_Data.agentId, m_Interaction);
            }
        }
    }
    
}


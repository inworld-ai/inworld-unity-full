/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using UnityEngine;
using Inworld.Interactions;


namespace Inworld.Sample.Innequin
{
    [RequireComponent(typeof(InworldInteraction))]
    public class InworldCharacter3D : InworldCharacter
    {
        protected override void OnCharRegistered(InworldCharacterData charData)
        {
            if (charData.brainName == Data.brainName)
                RegisterLiveSession();
        }
        /// <summary>
        /// Register live session once load scene completed.
        ///
        /// This overwritten function will also send its audio interaction to the mixer of AudioCapture
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

/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using UnityEngine;
using Inworld.Interactions;
using System;


namespace Inworld.Sample.Innequin
{
    [RequireComponent(typeof(InworldInteraction))]
    public class InworldCharacter3D : InworldCharacter
    {
        [SerializeField] bool m_AutoStart = true;
        void Start()
        {
            if (m_AutoStart)
                InworldController.CharacterHandler.Register(this);
        }
    }
}

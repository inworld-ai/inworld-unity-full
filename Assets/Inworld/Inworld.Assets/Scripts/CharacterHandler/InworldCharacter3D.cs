﻿/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
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
        [SerializeField] bool m_AutoStart = true;
        void Start()
        {
            if (!string.IsNullOrEmpty(Data.brainName) && Data.brainName.Split('/').Length <= 1 && InworldController.Instance.GameData)
                Data.brainName = InworldAI.GetCharacterFullName(InworldController.Instance.GameData.workspaceName, Data.brainName);
            if (m_AutoStart)
                InworldController.CharacterHandler.Register(this);
        }
    }
}

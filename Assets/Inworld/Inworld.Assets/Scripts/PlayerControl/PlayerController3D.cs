/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Assets;
using UnityEngine;

namespace Inworld.Sample
{
    public class PlayerController3D : PlayerController
    {
        InworldCameraController m_CameraController;
        protected void Awake()
        {
            m_CameraController = GetComponent<InworldCameraController>();
            m_BubbleChat = GetComponent<BubbleChat>();
        }
    }
}

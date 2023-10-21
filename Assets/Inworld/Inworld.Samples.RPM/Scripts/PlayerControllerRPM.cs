/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using UnityEngine;


namespace Inworld.Sample.RPM
{
    public class PlayerControllerRPM : PlayerController3D
    {
        InworldCameraController m_CameraController;

        protected override void Awake()
        {
            base.Awake();
            m_CameraController = GetComponent<InworldCameraController>();
        }
        
        protected override void HandleInput()
        {
            base.HandleInput();
            if (Input.GetKeyUp(KeyCode.BackQuote))
            {
                m_CameraController.enabled = !m_ChatCanvas.activeSelf;
            }
        }
    }
}

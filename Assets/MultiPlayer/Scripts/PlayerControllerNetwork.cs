/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Assets;
using UnityEngine;


namespace Inworld.Sample.RPM
{
    public class PlayerControllerNetwork : PlayerController3D
    {
        FeedbackCanvas m_FeedbackDlg;
        InworldNetworkPlayer m_CameraController;
        protected void Awake()
        {
            m_CameraController = GetComponent<InworldNetworkPlayer>();
            m_FeedbackDlg = m_FeedbackCanvas.GetComponent<FeedbackCanvas>();
        }
        protected override void HandleCanvas()
        {
            m_CameraController.enabled = !IsAnyCanvasOpen;
        }
        public override void OpenFeedback(string interactionID, string correlationID)
        {
            m_FeedbackDlg.Open(interactionID, correlationID);
        }
    }
}

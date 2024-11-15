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
        ContentEditingCanvas m_ContentEditingDlg;
        InworldCameraController m_CameraController;
        protected void Awake()
        {
            m_CameraController = GetComponent<InworldCameraController>();
            m_ContentEditingDlg = GetComponent<ContentEditingCanvas>();
        }
        public override void OpenContextEditing(string interactionID, string correlationID)
        {
            m_ContentEditingDlg.Open();
            m_ContentEditingDlg.Init(interactionID, correlationID);
        }
    }
}

/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Runtime;
using Inworld.Sample.UI;
using UnityEngine;
namespace Inworld.Sample
{
    /// <summary>
    ///     This is the class for global text management, by original, it's added in Player Controller.
    ///     And would be called by Keycode.Backquote.
    /// </summary>
    public class InworldPlayer : InworldPlayer2D
    {
        #region Inspector Variables
        [SerializeField] InworldCameraController m_CameraController;
        [SerializeField] GameObject m_TriggerCanvas;
        [SerializeField] RecordButton m_RecordButton;
        [SerializeField] Vector3 m_InitPosition;
        [SerializeField] Vector3 m_InitRotation;
        #endregion
        

        #region Monobehavior Functions
        protected override void Update()
        {
            if (Input.GetKeyUp(KeyCode.BackQuote))
            {
                m_GlobalChatCanvas.SetActive(!m_GlobalChatCanvas.activeSelf);
                if (m_CameraController)
                    m_CameraController.enabled = !m_GlobalChatCanvas.activeSelf;
                if (m_TriggerCanvas)
                    m_TriggerCanvas.SetActive(!m_TriggerCanvas.activeSelf);
                InworldController.Instance.ManualAudioCapture = m_GlobalChatCanvas.activeSelf;
                InworldController.Audio.AutoPush = !m_GlobalChatCanvas.activeSelf;
                if (m_GlobalChatCanvas.activeSelf)
	                InworldController.Instance.EndAudioCapture();
                else
	                InworldController.Instance.StartAudioCapture();
            }
            UpdateSendText();
        }
        #endregion
    }
}

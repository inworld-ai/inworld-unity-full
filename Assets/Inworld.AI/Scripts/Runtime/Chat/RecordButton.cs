/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Inworld.Sample.UI
{
    /// <summary>
    ///     This class is used for the Record Button in the global chat panel.
    /// </summary>
    public class RecordButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        bool m_IsCapturingBeforeReset = false;
        void OnEnable()
        {
            m_IsCapturingBeforeReset = InworldController.IsCapturing;
            InworldController.IsCapturing = false;
        }
        void OnDisable()
        {
            InworldController.IsCapturing = m_IsCapturingBeforeReset;
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            InworldController.IsCapturing = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            InworldController.IsCapturing = false;
        }
    }
}

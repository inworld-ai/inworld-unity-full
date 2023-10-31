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
        void OnDisable()
        {
	        if (!InworldController.Instance)
		        return;
	        InworldController.Instance.PushAudio();
            InworldController.Instance.EndAudioCapture();
        }
        public void OnPointerDown(PointerEventData eventData)
        {
	        if (!InworldController.Instance)
		        return;
	        InworldController.Instance.StartAudioCapture();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
	        if (!InworldController.Instance)
		        return;
	        InworldController.Instance.PushAudio();
	        InworldController.Instance.EndAudioCapture();
        }
    }
}

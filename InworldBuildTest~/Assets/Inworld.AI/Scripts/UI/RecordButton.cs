/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using UnityEngine;
using UnityEngine.EventSystems;

namespace Inworld.UI
{
    public class RecordButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        
        public void OnPointerDown(PointerEventData eventData)
        {
            
            InworldController.Instance.StartAudio();
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            InworldController.Instance.PushAudio();
        }
    }
}
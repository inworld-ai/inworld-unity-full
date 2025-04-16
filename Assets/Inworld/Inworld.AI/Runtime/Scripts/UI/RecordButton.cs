/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
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
            if (InworldController.Client.Status == InworldConnectionStatus.Connected)
                InworldController.Instance.CancelResponses();
            InworldController.Audio.IsPlayerSpeaking = true;
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            InworldController.Audio.IsPlayerSpeaking = false;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Inworld.UI
{
    public class RecordButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            InworldController.CharacterHandler.StartAudio();
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            InworldController.CharacterHandler.StopAudio(true);
        }
    }
}
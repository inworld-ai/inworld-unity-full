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
            CharacterHandler.Instance.StartAudio();
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            CharacterHandler.Instance.StopAudio(true);
        }
    }
}
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
            if(InworldController.Instance.CurrentCharacter)
                InworldController.Instance.StartAudio();
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (InworldController.Instance.CurrentCharacter)
            {
                InworldController.Instance.PushAudio();
                InworldController.Instance.StopAudio();
            }
                
        }
    }
}
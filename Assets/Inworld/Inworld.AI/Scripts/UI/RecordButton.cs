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
                InworldController.Instance.StartAudio(InworldController.Instance.CurrentCharacter.ID);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (InworldController.Instance.CurrentCharacter)
            {
                InworldController.Instance.PushAudio(InworldController.Instance.CurrentCharacter.ID);
                InworldController.Instance.StopAudio(InworldController.Instance.CurrentCharacter.ID);
            }
                
        }
    }
}
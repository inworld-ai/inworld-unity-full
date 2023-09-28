
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
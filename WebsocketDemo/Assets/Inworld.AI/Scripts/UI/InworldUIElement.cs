using UnityEngine;
using UnityEngine.UI;
namespace Inworld
{
    public class InworldUIElement : MonoBehaviour
    {    
        [SerializeField] protected RawImage m_Icon;
        /// <summary>
        ///     Get the bubble's height.
        /// </summary>
        public float Height => GetComponent<RectTransform>().sizeDelta.y + 20;
        /// <summary>
        ///     Get/Set the Thumbnail of the bubble.
        ///     NOTE: For the worldspace's floating text, Icon will not be displayed.
        /// </summary>
        public RawImage Icon
        {
            get => m_Icon;
            set => m_Icon = value;
        }
    }
}

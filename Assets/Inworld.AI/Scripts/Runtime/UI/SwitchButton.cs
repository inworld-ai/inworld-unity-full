using UnityEngine;
using UnityEngine.UI;

namespace Inworld.Runtime
{
    public class SwitchButton : MonoBehaviour
    {
        [SerializeField] Image m_Image;
        
        public void CheckBackground(bool isOn)
        {
            m_Image.enabled = !isOn;
        }
    }
}

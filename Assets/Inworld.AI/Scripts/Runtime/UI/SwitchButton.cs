/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using UnityEngine;
using UnityEngine.UI;

namespace Inworld.Runtime
{
    public class SwitchButton : MonoBehaviour
    {
	    [SerializeField] Toggle m_Toggle;
        [SerializeField] Image m_Image;
        [SerializeField] Sprite m_OnSprite;
        Sprite m_OffSprite;

        void Awake()
        {
	        m_OffSprite = m_Image.sprite;
	        CheckSprite(m_Toggle.isOn);
        }
        
        public void CheckSprite(bool isOn)
        {
            m_Image.sprite = isOn ? m_OffSprite : m_OnSprite;
        }
    }
}

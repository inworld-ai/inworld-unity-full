/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using UnityEngine;
using UnityEngine.UI;

namespace Inworld.Runtime.RPM
{
    public class SwitchButton : MonoBehaviour
    {
        [SerializeField] Sprite m_OnSprite;
        [SerializeField] Sprite m_OffSprite;
        [SerializeField] Image m_Image;
        
        /// <summary>
        /// Switch the sprite based on this toggle's status. 
        /// </summary>
        /// <param name="isOn">the incoming status of the toggle.</param>
        public void CheckBackground(bool isOn)
        {
            m_Image.sprite = isOn ? m_OnSprite : m_OffSprite;
        }
    }
}

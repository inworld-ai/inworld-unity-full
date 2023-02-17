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
        [SerializeField] Image m_Image;
        
        public void CheckBackground(bool isOn)
        {
            m_Image.enabled = !isOn;
        }
    }
}

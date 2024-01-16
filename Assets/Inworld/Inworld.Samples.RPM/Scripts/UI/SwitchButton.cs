/*************************************************************************************************
* Copyright 2022-2024 Theai, Inc. dba Inworld AI
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/

using UnityEngine.UI;

namespace Inworld.Runtime.RPM
{
    public class SwitchButton : Toggle
    {
        protected override void Start()
        {
            base.Start();
            onValueChanged.AddListener(CheckBackground);
        }
        void CheckBackground(bool on)
        {
            if (targetGraphic && targetGraphic.gameObject)
                targetGraphic.gameObject.GetComponent<Image>().enabled = !on;
        }
    }
}

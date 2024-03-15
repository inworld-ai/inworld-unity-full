/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inworld.Assets
{
    public class FeedbackButton : MonoBehaviour
    {
        [SerializeField] TMP_Text m_Label;
        [SerializeField] Color m_OnColor;
        [SerializeField] Color m_OffColor;

        Image m_Image;
        void Start()
        {
            Toggle toggle = GetComponent<Toggle>();
            m_Image = GetComponent<Image>();
            
            if (toggle)
                toggle.onValueChanged.AddListener(OnToggleChanged);
        }
        void OnToggleChanged(bool isOn)
        {
            if (m_Label)
                m_Label.color = isOn ? m_OnColor : m_OffColor;
            if (m_Image)
                m_Image.color = isOn ? m_OnColor : m_OffColor;
        }
    }
}

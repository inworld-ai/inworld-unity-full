/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using TMPro;
using UnityEngine;


namespace Inworld.Sample
{
    public class TokenCanvas : MonoBehaviour
    {
        [SerializeField] TMP_InputField m_TokenInput;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
            {
                SendToken();
                gameObject.SetActive(false);
            }
        }
        public void SendToken()
        {
            if (string.IsNullOrEmpty(m_TokenInput.text))
            {
                Debug.LogError("Token Incorrect!");
                return;
            }
            InworldController.Instance.Init(m_TokenInput.text);
            gameObject.SetActive(false);
        }
    }
}
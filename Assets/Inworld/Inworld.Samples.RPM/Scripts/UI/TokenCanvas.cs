/*************************************************************************************************
* Copyright 2022-2024 Theai, Inc. dba Inworld AI
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using TMPro;
using UnityEngine;


namespace Inworld.Sample.RPM
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
        /// <summary>
        /// Send the custom token to InworldClient.
        /// </summary>
        public void SendToken()
        {
            if (string.IsNullOrEmpty(m_TokenInput.text))
            {
                Debug.LogError("Token Incorrect!");
                return;
            }
            InworldController.Instance.InitWithCustomToken(m_TokenInput.text);
            gameObject.SetActive(false);
        }
    }
}
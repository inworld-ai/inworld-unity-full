/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Studio;
using Inworld.Util;
using TMPro;
using UnityEngine;
namespace Inworld.Runtime
{
    public class LoginPanel : MonoBehaviour
    {
        [SerializeField] TMP_InputField m_InputField;
        public void OpenStudio()
        {
            Application.OpenURL(InworldAI.Game.currentServer.web);
        }
        public void OpenTutorial()
        {
            Application.OpenURL(InworldAI.Game.currentServer.tutorialPage);
        }

        public void Login()
        {
            if (string.IsNullOrEmpty(m_InputField.text))
            {
                RuntimeCanvas.Instance.Error("Login Error", "Token is empty.\nPlease fill the token first!");
                return;
            }
            string[] tokenForExchange = m_InputField.text.Split(':');
            if (tokenForExchange.Length < 2)
            {
                RuntimeCanvas.Instance.Error("Login Error", "Token is error.\nPlease paste the correct token!");
                return;
            }
            InworldAI.User.IDToken = tokenForExchange[0];
            InworldAI.User.RefreshTokens(tokenForExchange[0], tokenForExchange[1]);
            RuntimeInworldStudio.Instance.Init(InworldAI.User.IDToken);
        }
    }
}

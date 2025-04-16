/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inworld.UI
{
    public class ConnectButton : MonoBehaviour
    {
        [SerializeField] protected TMP_Text m_Status;
        [SerializeField] protected Button m_ConnectButton;
        [SerializeField] TMP_Text m_ButtonText;

        /// <summary>
        /// Control the InworldController to connect inworld server.
        /// </summary>
        public virtual void ConnectInworld()
        {
            switch (InworldController.Status)
            {
                case InworldConnectionStatus.Idle:
                    InworldController.Instance.Reconnect();
                    break;
                case InworldConnectionStatus.Initialized:
                    InworldController.Client.StartSession();
                    break;
                case InworldConnectionStatus.Connected:
                    InworldController.Instance.Disconnect();
                    break;
            }
        }

        void OnEnable()
        {
            InworldController.Client.OnStatusChanged += OnStatusChanged;
        }

        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Client.OnStatusChanged -= OnStatusChanged;
        }
        
        void OnStatusChanged(InworldConnectionStatus newStatus)
        {
            if(!m_ConnectButton)
                return;
            switch (newStatus)
            {
                case InworldConnectionStatus.Idle:
                    _SetButtonStatus(true, "CONNECT");
                    break;
                case InworldConnectionStatus.Connected:
                    _SetButtonStatus(true, "DISCONNECT");
                    break;
                case InworldConnectionStatus.Initialized:
                    InworldController.Client.StartSession();
                    break;
                default:
                    _SetButtonStatus(false);
                    break;
            }
            if (!m_Status)
                return;
            m_Status.text = newStatus.ToString();
        }
        protected void _SetButtonStatus(bool interactable, string buttonText = "")
        {
            if (m_ConnectButton)
                m_ConnectButton.interactable = interactable;
            if (m_ButtonText)
                m_ButtonText.text = buttonText;
        }
    }
}

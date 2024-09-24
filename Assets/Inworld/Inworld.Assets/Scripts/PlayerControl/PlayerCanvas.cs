/*************************************************************************************************
* Copyright 2022-2024 Theai, Inc. dba Inworld AI
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/

using UnityEngine;
using UnityEngine.InputSystem;

namespace Inworld.Sample
{
    public class PlayerCanvas : MonoBehaviour
    {
        [SerializeField] protected string m_ActionName;
        [SerializeField] protected GameObject m_CanvasObj;
        
        protected InputAction m_InputAction;
        public bool KeyReleased => m_InputAction != null && m_InputAction.WasReleasedThisFrame();

        public void Open()
        {
            if (m_CanvasObj.activeSelf)
                return;
            m_CanvasObj.SetActive(true);
            OnCanvasOpen();
            if (PlayerController.Instance)
                PlayerController.Instance.UILayer++;
        }

        public void Close()
        {
            if (!m_CanvasObj.activeSelf)
                return;
            m_CanvasObj.SetActive(false);
            OnCanvasClosed();
            if (PlayerController.Instance)
                PlayerController.Instance.UILayer--;
        }
        protected virtual void OnCanvasOpen()
        {
            
        }
        protected virtual void OnCanvasClosed()
        {
            
        }
        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(m_ActionName))
                return;
            m_InputAction = InworldAI.InputActions[m_ActionName];
        }
        protected virtual void HandleInput()
        {
            if (!KeyReleased)
                return;
            if (m_CanvasObj.activeSelf)
                Close();
            else
                Open();
        }
    
        // Update is called once per frame
        void Update()
        {
            HandleInput();
        }
    }

}

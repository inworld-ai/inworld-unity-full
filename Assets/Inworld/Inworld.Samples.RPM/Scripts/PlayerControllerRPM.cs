using UnityEngine;


namespace Inworld.Sample.RPM
{
    public class PlayerControllerRPM : PlayerController3D
    {
        InworldCameraController m_CameraController;

        protected override void Awake()
        {
            base.Awake();
            m_CameraController = GetComponent<InworldCameraController>();
        }
        
        protected override void HandleInput()
        {
            base.HandleInput();
            if (Input.GetKeyUp(KeyCode.BackQuote))
            {
                m_CameraController.enabled = !m_ChatCanvas.activeSelf;
            }
        }
    }
}

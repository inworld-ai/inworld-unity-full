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
    // YAN: Basic camera controller. Based on Unity's default SimpleCameraController.
    public class InworldCameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Range(0, 5)]
        [Tooltip("Exponential boost factor on translation, controllable by mouse wheel.")]
        public float boost = 3.5f;

        [Tooltip("Time it takes to interpolate camera position 99% of the way to the target.")][Range(0.001f, 1f)]
        public float positionLerpTime = 0.2f;

        [Header("Rotation Settings")]
        [Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
        public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

        [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target.")][Range(0.001f, 1f)]
        public float rotationLerpTime = 0.01f;

        [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
        public bool invertY;
        
        readonly CameraState m_InterpolatingCameraState = new CameraState();

        readonly CameraState m_TargetCameraState = new CameraState();

        InputAction m_LeftClickInputAction;
        InputAction m_MouseDeltaInputAction;
        InputAction m_SpeedUpInputAction;
        InputAction m_SpeedInputAction;
        InputAction m_MoveInputAction;

        void Awake()
        {
            m_LeftClickInputAction = InworldAI.InputActions["LeftClick"];
            m_MouseDeltaInputAction = InworldAI.InputActions["MouseDelta"];
            m_SpeedUpInputAction = InworldAI.InputActions["SpeedUp"];
            m_SpeedInputAction = InworldAI.InputActions["Speed"];
            m_MoveInputAction = InworldAI.InputActions["Move"];
        }
        void OnEnable()
        {
            m_TargetCameraState.SetFromTransform(transform);
            m_InterpolatingCameraState.SetFromTransform(transform);
        }
        void OnDisable()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        void Update()
        {
            // Hide and lock cursor when right mouse button pressed
            if (m_LeftClickInputAction.WasPressedThisFrame())
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            // Unlock and show cursor when right mouse button released
            if (m_LeftClickInputAction.WasReleasedThisFrame())
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            // Rotation
            if (Cursor.lockState != CursorLockMode.None)
            {
                Vector2 mouseMovement = m_MouseDeltaInputAction.ReadValue<Vector2>() * 0.1f;
                mouseMovement.y *= (invertY ? 1 : -1);
                float mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);
                m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
                m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
            }
            // Translation
            Vector3 translation = GetInputTranslationDirection() * Time.deltaTime;

            // Speed up movement when shift key held
            if (m_SpeedUpInputAction.IsPressed())
            {
                translation *= 10.0f;
            }

            // Modify movement by a boost factor (defined in Inspector and modified in play mode through the mouse scroll wheel)
            boost += m_SpeedInputAction.ReadValue<float>() * 0.001f;
            boost = Mathf.Clamp(boost, 0, 5);
            translation *= Mathf.Pow(2.0f, boost);
            m_TargetCameraState.Translate(translation);

            // Framerate-independent interpolation
            // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
            float positionLerpPct = 1f - Mathf.Exp(Mathf.Log(1f - 0.99f) / positionLerpTime * Time.deltaTime);
            float rotationLerpPct = 1f - Mathf.Exp(Mathf.Log(1f - 0.99f) / rotationLerpTime * Time.deltaTime);
            m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);

            m_InterpolatingCameraState.UpdateTransform(transform);
        }



        Vector3 GetInputTranslationDirection()
        {
            return m_MoveInputAction.ReadValue<Vector3>();
        }
        class CameraState
        {
            float m_Roll;
            float m_X;
            float m_Y;
            float m_Z;
            public float pitch;
            public float yaw;

            public void SetFromTransform(Transform t)
            {
                Vector3 eulerAngles = t.eulerAngles;
                pitch = eulerAngles.x;
                yaw = eulerAngles.y;
                m_Roll = eulerAngles.z;
                Vector3 position = t.position;
                m_X = position.x;
                m_Y = position.y;
                m_Z = position.z;
            }

            public void Translate(Vector3 translation)
            {
                Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, m_Roll) * translation;

                m_X += rotatedTranslation.x;
                m_Y += rotatedTranslation.y;
                m_Z += rotatedTranslation.z;
            }

            public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
            {
                yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
                pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
                m_Roll = Mathf.Lerp(m_Roll, target.m_Roll, rotationLerpPct);

                m_X = Mathf.Lerp(m_X, target.m_X, positionLerpPct);
                m_Y = Mathf.Lerp(m_Y, target.m_Y, positionLerpPct);
                m_Z = Mathf.Lerp(m_Z, target.m_Z, positionLerpPct);
            }

            public void UpdateTransform(Transform t)
            {
                t.eulerAngles = new Vector3(pitch, yaw, m_Roll);
                t.position = new Vector3(m_X, m_Y, m_Z);
            }
        }
    }
}

/*************************************************************************************************
* Copyright 2022-2024 Theai, Inc. dba Inworld AI
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
namespace Inworld.Sample
{
    // YAN: Basic camera controller. Based on Unity's default SimpleCameraController.
    public class InworldNetworkPlayer : NetworkBehaviour
    {
        public NetworkVariable<Vector3> nwEulerAngles = new NetworkVariable<Vector3>();
        public NetworkVariable<Vector3> nwPosition = new NetworkVariable<Vector3>();
        
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
        
        [Rpc(SendTo.Server)]
        public void UpdateNetworkTransformToServerRpc(Vector3 eulerAngle, Vector3 position)
        {
            
            nwEulerAngles.Value = eulerAngle;
            nwPosition.Value = position;
            NetworkLogCanvas.Instance.ServerRecv(m_InterpolatingCameraState.m_X, m_InterpolatingCameraState.m_Y, m_InterpolatingCameraState.m_Z, m_InterpolatingCameraState.yaw, m_InterpolatingCameraState.pitch, m_InterpolatingCameraState.m_Roll);
            UpdateNetworkTransformToClientRpc();
        }
        [Rpc(SendTo.ClientsAndHost)]
        public void UpdateNetworkTransformToClientRpc()
        {
            Debug.Log($"Client Received Pitch: {nwEulerAngles.Value.x} Yaw: {nwEulerAngles.Value.y} Row: {nwEulerAngles.Value.z}");
            transform.eulerAngles =  nwEulerAngles.Value;
            transform.position =  nwPosition.Value;
            NetworkLogCanvas.Instance.ClientRecv(nwPosition.Value.x, nwPosition.Value.y, nwPosition.Value.z, nwEulerAngles.Value.x, nwEulerAngles.Value.y, nwEulerAngles.Value.z);

        }
        void Update()
        {
            if (!IsOwner)
                return;
            // Exit Sample  
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }
            // Hide and lock cursor when right mouse button pressed
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            // Unlock and show cursor when right mouse button released
            if (Input.GetMouseButtonUp(0))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            // Rotation
            if (Cursor.lockState != CursorLockMode.None)
            {
                Vector2 mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * (invertY ? 1 : -1));
                float mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);
                m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
                m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
            }
            // Translation
            Vector3 translation = GetInputTranslationDirection() * Time.deltaTime;

            // Speed up movement when shift key held
            if (Input.GetKey(KeyCode.LeftShift))
            {
                translation *= 10.0f;
            }

            // Modify movement by a boost factor (defined in Inspector and modified in play mode through the mouse scroll wheel)
            boost += Input.mouseScrollDelta.y * 0.2f;
            translation *= Mathf.Pow(2.0f, boost);
            m_TargetCameraState.Translate(translation);

            // Framerate-independent interpolation
            // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
            float positionLerpPct = 1f - Mathf.Exp(Mathf.Log(1f - 0.99f) / positionLerpTime * Time.deltaTime);
            float rotationLerpPct = 1f - Mathf.Exp(Mathf.Log(1f - 0.99f) / rotationLerpTime * Time.deltaTime);
            LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);
            var angle = new Vector3(m_InterpolatingCameraState.pitch, m_InterpolatingCameraState.yaw, m_InterpolatingCameraState.m_Roll);
            var position = new Vector3(m_InterpolatingCameraState.m_X, m_InterpolatingCameraState.m_Y, m_InterpolatingCameraState.m_Z);
            UpdateNetworkTransformToServerRpc(angle, position);
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

        Vector3 GetInputTranslationDirection()
        {
            Vector3 direction = new Vector3();
            if (Input.GetKey(KeyCode.W))
            {
                Debug.Log("Get Key W");
                direction += Vector3.forward;
            }
            if (Input.GetKey(KeyCode.S))
            {
                direction += Vector3.back;
            }
            if (Input.GetKey(KeyCode.A))
            {
                direction += Vector3.left;
            }
            if (Input.GetKey(KeyCode.D))
            {
                direction += Vector3.right;
            }
            if (Input.GetKey(KeyCode.Q))
            {
                direction += Vector3.down;
            }
            if (Input.GetKey(KeyCode.E))
            {
                direction += Vector3.up;
            }
            return direction;
        }
        public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
        {
            m_InterpolatingCameraState.yaw = Mathf.Lerp(m_InterpolatingCameraState.yaw, target.yaw, rotationLerpPct);
            m_InterpolatingCameraState.pitch = Mathf.Lerp(m_InterpolatingCameraState.pitch, target.pitch, rotationLerpPct);
            m_InterpolatingCameraState.m_Roll = Mathf.Lerp(m_InterpolatingCameraState.m_Roll, target.m_Roll, rotationLerpPct);

            m_InterpolatingCameraState.m_X = Mathf.Lerp(m_InterpolatingCameraState.m_X, target.m_X, positionLerpPct);
            m_InterpolatingCameraState.m_Y = Mathf.Lerp(m_InterpolatingCameraState.m_Y, target.m_Y, positionLerpPct);
            m_InterpolatingCameraState.m_Z = Mathf.Lerp(m_InterpolatingCameraState.m_Z, target.m_Z, positionLerpPct);
            
            if (IsClient)
                NetworkLogCanvas.Instance.ClientSend(m_InterpolatingCameraState.m_X, m_InterpolatingCameraState.m_Y, m_InterpolatingCameraState.m_Z, m_InterpolatingCameraState.yaw, m_InterpolatingCameraState.pitch, m_InterpolatingCameraState.m_Roll);
        }
        public class CameraState
        {
            public float m_Roll;
            public float m_X;
            public float m_Y;
            public float m_Z;
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
        }
    }
}

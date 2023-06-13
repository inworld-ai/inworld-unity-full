﻿/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Audio;
using Siccity.GLTFUtility;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Inworld.Model.Sample
{
    /// <summary>
    ///     Default Avatar Loader.
    ///     Use GLTFUtility to download and import RPM data, such as .glb files.
    /// </summary>
    public class GLTFAvatarLoader : MonoBehaviour, IAvatarLoader
    {
        #region Inspector Variables
        [SerializeField] RuntimeAnimatorController m_Controller;
        [SerializeField] Avatar m_Avatar;
        [SerializeField] GameObject m_HeadAnimLoader;
        [SerializeField] Material m_SRPMaterial;
        InworldCharacter m_CharacterToProcess;
        public event Action<InworldCharacter> AvatarLoaded;
        #endregion

        #region Interface Functions
        public void ConfigureModel(InworldCharacter character, GameObject model)
        {
            m_CharacterToProcess = character;
            if (model)
                _ConfigureModel(model);
            _InstallAnimator();
            InstallScriptableRenderPipelineMaterials(model);
            _InstallLipsync();
            if (m_HeadAnimLoader)
                _SetupHeadMovement();
            AvatarLoaded?.Invoke(character);
        }
        public IEnumerator Import(string url)
        {
            Debug.LogError("GLTF Doesn't support streaming avatar. Please download to local files.");
            yield break;
        }
        public GameObject LoadData(byte[] content)
        {
            return Importer.LoadFromBytes(content);
        }
        public GameObject LoadData(string fileName)
        {
            return Importer.LoadFromFile(fileName);
        }
        #endregion

        #region Private Functions
        void _ConfigureModel(GameObject model)
        {
            if (m_CharacterToProcess.CurrentAvatar && m_CharacterToProcess.CurrentAvatar != model)
            {
                Debug.Log("Destroying old avatar");
                DestroyImmediate(m_CharacterToProcess.CurrentAvatar);
            }

            model.transform.SetParent(m_CharacterToProcess.transform);
            model.transform.name = "Armature";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            m_CharacterToProcess.CurrentAvatar = model;
        }

        public void InstallScriptableRenderPipelineMaterials(GameObject _model)
        {
            if (IsUsingScriptableRenderPipeline())
            {
                SkinnedMeshRenderer renderer = _model.GetComponentInChildren<SkinnedMeshRenderer>();
                Texture2D baseColorMap = renderer.sharedMaterial.GetTexture("_MainTex") as Texture2D;
                Texture2D normalMap = renderer.sharedMaterial.GetTexture("_BumpMap") as Texture2D;
                Material newMat = new Material(m_SRPMaterial);
                newMat.SetTexture("_BASE_COLOR_MAP", baseColorMap);
                newMat.SetTexture("_NORMAL_MAP", normalMap);
                renderer.material = newMat;
            }
        }
        
        void _InstallAnimator()
        {
            Animator animator = m_CharacterToProcess.GetComponent<Animator>();
            if (!animator)
                animator = m_CharacterToProcess.gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = m_Controller;
            animator.avatar = m_Avatar;
            foreach (InworldAnimation animation in m_CharacterToProcess.GetComponentsInChildren<InworldAnimation>())
            {
                animation.Init();
            }
        }
        void _InstallLipsync()
        {
            InworldLipAnimation lipsync = m_CharacterToProcess.GetComponent<InworldLipAnimation>();
            if (lipsync)
                lipsync.Init();
        }
        void _SetupHeadMovement()
        {
            IEyeHeadAnimLoader eyeHead = m_HeadAnimLoader.GetComponent<IEyeHeadAnimLoader>();
            eyeHead?.SetupHeadMovement(m_CharacterToProcess.gameObject);
        }

        bool IsUsingScriptableRenderPipeline()
        {
            return GraphicsSettings.defaultRenderPipeline != null;
        }
        #endregion
    }
}

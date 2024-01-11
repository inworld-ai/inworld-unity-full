/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Inworld.Editors
{
    public static class InworldRenderPipelineConverter
    {
        static readonly int s_LegacyBaseMap = Shader.PropertyToID("_MainTex");
        static readonly int s_LegacyNormalMap = Shader.PropertyToID("_BumpMap");
        static readonly int s_MetallicMap = Shader.PropertyToID("_MetallicGlossMap");
        static readonly int s_Smoothness = Shader.PropertyToID("_Smoothness");
        
        static readonly int s_URPBaseMap = Shader.PropertyToID("_BaseMap");
        static readonly int s_URPNormalMap = Shader.PropertyToID("_BumpMap");
        
        static readonly int s_HDRPBaseMap = Shader.PropertyToID("_BaseColorMap");
        static readonly int s_HDRPNormalMap = Shader.PropertyToID("_NormalMap");

        public static void UpgradeMaterial()
        {
            if (!GraphicsSettings.currentRenderPipeline)
            {
                InworldAI.LogError("Current Rendering pipeline is not URP or HDRP!");
                return;
            }
            InworldAI.Log($"Updating material for {Selection.activeGameObject.name}");
            Renderer[] renderers = Selection.activeGameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Material material = renderer.sharedMaterial;
                Material newMat = new Material(GraphicsSettings.currentRenderPipeline.defaultMaterial);
                if (material)
                {
                    Texture2D baseMap = material.GetTexture(s_LegacyBaseMap) as Texture2D;
                    Texture2D normalMap = material.GetTexture(s_LegacyNormalMap) as Texture2D;
                    Texture2D metallicMap = material.GetTexture(s_MetallicMap) as Texture2D;
                    newMat.SetTexture(s_URPBaseMap, baseMap);
                    newMat.SetTexture(s_HDRPBaseMap, baseMap);
                    newMat.SetTexture(s_URPNormalMap, normalMap);
                    newMat.SetTexture(s_HDRPNormalMap, normalMap);
                    newMat.SetTexture(s_MetallicMap, metallicMap);
                    newMat.SetFloat(s_Smoothness, 0.15f); // YAN: GLTF's smoothness = 1 - mainTex.g * _Roughness.
                }
                renderer.material = newMat;
            }
            InworldAI.Log($"{Selection.activeGameObject.name} Updating material completed!");
        }
    }
}
#endif
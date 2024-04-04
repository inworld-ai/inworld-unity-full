/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Assets;
using Inworld.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;
using UnityEngine;

namespace Inworld.Sample.RPM
{
    public class InworldSentisFacialAnimation  : InworldFacialAnimation
    {
        [SerializeField] protected LipsyncMap m_LipsyncMap;
        [SerializeField] ModelAsset m_Model;
        [SerializeField] SkinnedMeshRenderer m_Skin;
        [SerializeField] string m_VisemeSil;
        [SerializeField] string m_BlinkBlendShape;
        List<VisemeData> m_VisemeList = new List<VisemeData>();
        List<int> m_InputArray = new List<int>();
        int m_VisemeIndex;
        int m_BlinkIndex;
        int m_CurrentPhonemeIndex;
        int m_CurrFrame;
        Model m_RuntimeModel;
        IWorker m_CurrWorker;
        protected override void OnEnable()
        {
            base.OnEnable();
            m_RuntimeModel = ModelLoader.Load(m_Model);
            m_CurrWorker = WorkerFactory.CreateWorker(BackendType.GPUCompute, m_RuntimeModel);
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            m_CurrWorker.Dispose();
        }
        protected override bool Init()
        {
            if (!base.Init())
                return false;
            if (!m_Skin)
                m_Skin = m_Character.GetComponentInChildren<SkinnedMeshRenderer>();
            return _MappingBlendShape();
        }
        bool _MappingBlendShape()
        {
            if (!m_Skin)
                return false;
            for (int i = 0; i < m_Skin.sharedMesh.blendShapeCount; i++)
            {
                if (m_Skin.sharedMesh.GetBlendShapeName(i) == m_VisemeSil)
                {
                    m_VisemeIndex = i;
                    Debug.Log($"Find Viseme Index {m_VisemeIndex}");
                }
                if (m_Skin.sharedMesh.GetBlendShapeName(i) == m_BlinkBlendShape)
                {
                    m_BlinkIndex = i;
                    Debug.Log($"Find Blink Index {m_BlinkIndex}");
                }
            }
            return m_BlinkIndex + m_VisemeIndex != 0;
        }
        protected override void ProcessLipSync()
        {
            if (m_CurrFrame >= m_VisemeList.Count) // Finished
            {
                Reset();
                return;
            }
            ApplyMesh(m_VisemeList[m_CurrFrame]);
            m_CurrFrame++;
        }
        protected override void BlinkEyes()
        {
            if (!m_Skin)
                return;
            float blendshapeValue = Mathf.Sin(Time.time * 2f) * 100 - 99f;
            blendshapeValue = Mathf.Clamp(blendshapeValue, 0, 1);
            m_Skin.SetBlendShapeWeight(m_BlinkIndex, blendshapeValue);
        }
        protected override void Reset()
        {
            m_VisemeList.Clear();
            m_CurrFrame = 0;
            _ShutMouth();
        }
        protected override void HandleLipSync(AudioPacket audioPacket)
        {
            Reset();
            if (audioPacket.routing.source.name.ToUpper() == "PLAYER")
                return;
            LoadPhonemeInfo(audioPacket);
            if (m_InputArray.Count == 0)
                return;
            TensorInt inputTensor = new TensorInt(new TensorShape(1, m_InputArray.Count), m_InputArray.ToArray());
            m_CurrWorker.Execute(inputTensor);
            TensorFloat outputTensor = m_CurrWorker.PeekOutput() as TensorFloat;
            if (outputTensor == null)
            {
                Debug.LogError("No Output!");
                return;
            }
            outputTensor.MakeReadable();
            float[] array = outputTensor.ToReadOnlyArray();
            VisemeData data = new VisemeData();
            for (int i = 0; i < array.Length; i+=15)
            {
                for (int j = 0; j < 15; j++)
                {
                    data.visemeVal.Add(array[i + j]);
                }
                m_VisemeList.Add(data);
                data = new VisemeData();
            }
            inputTensor.Dispose();
            outputTensor.Dispose();
        }
        void LoadPhonemeInfo(AudioPacket audioPacket)
        {
            AudioClip clip = audioPacket.Clip;
            if (!clip)
                return;
            float audioLength = clip.length;
            
            // Handled by children.
            List<PhonemeInfo> phonemes = audioPacket.dataChunk.additionalPhonemeInfo;
            m_CurrentPhonemeIndex = 0; 
            m_InputArray = new List<int>();
            for (float fTime = 0; fTime < audioLength; fTime += Time.fixedDeltaTime)
            {
                if (m_CurrentPhonemeIndex >= phonemes.Count)
                    break;
                int nTensorIndex = m_LipsyncMap.TensorIndexOf(phonemes[m_CurrentPhonemeIndex].phoneme);
                if (nTensorIndex == -1)
                    nTensorIndex = m_InputArray.Count > 0 ? m_InputArray[^1] : 0;
                m_InputArray.Add(nTensorIndex);
                if (fTime > phonemes[m_CurrentPhonemeIndex].startOffset)
                {
                    m_CurrentPhonemeIndex++;
                }
            }
        }
        protected void ApplyMesh(VisemeData visemeData)
        {
            for (int i = 0; i < visemeData.visemeVal.Count; i++)
            {
                m_Skin.SetBlendShapeWeight(m_VisemeIndex + i, visemeData.visemeVal[i]);
            }
        }
        void _ShutMouth()
        {
            if (!m_Skin)
                return;
            m_Skin.SetBlendShapeWeight(m_VisemeIndex, 1);
            for (int i = 1; i < 15; i++)
            {
                m_Skin.SetBlendShapeWeight(m_VisemeIndex + i, 0);
            }
        }
    }
}


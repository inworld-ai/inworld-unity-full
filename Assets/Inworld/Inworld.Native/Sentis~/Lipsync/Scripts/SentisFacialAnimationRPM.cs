/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Packet;
using Inworld.Sample.RPM;
using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine;

namespace Inworld.Native
{
    public class SentisFacialAnimationRPM : InworldFacialAnimationRPM
    {
        [SerializeField] ModelAsset m_Model;
        Model m_RuntimeModel;
        IWorker m_CurrWorker;
        int m_CurrFrame;
        List<VisemeData> m_VisemeList = new List<VisemeData>();
        List<int> m_InputArray = new List<int>();
        protected virtual bool IsSentisSupported => m_Model;
        
        protected virtual void LoadSentis()
        {
            m_RuntimeModel = ModelLoader.Load(m_Model);
            m_CurrWorker = WorkerFactory.CreateWorker(BackendType.GPUCompute, m_RuntimeModel);
            TensorInt dummyTensor = new TensorInt(new TensorShape(1, 14), new int[14]);
            m_CurrWorker.Execute(dummyTensor);
            m_CurrWorker.PeekOutput().CompleteOperationsAndDownload();
            dummyTensor.Dispose();
        }
        protected virtual void LoadPhonemeInfo(AudioPacket audioPacket)
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
                if (fTime > phonemes[m_CurrentPhonemeIndex].StartOffset)
                {
                    m_CurrentPhonemeIndex++;
                }
            }
        }
        protected virtual void UnloadSentis()
        {
            m_CurrWorker.Dispose();
        }
        protected override void MorphLipsync()
        {
            if (m_CurrFrame >= m_VisemeList.Count) // Finished
            {
                Reset();
                return;
            }
            ApplyMesh(m_VisemeList[m_CurrFrame]);
            m_CurrFrame++;
        }
        protected override void MorphSamplePhoneme(AudioPacket audioPacket)
        {
            LoadPhonemeInfo(audioPacket);
            if (m_InputArray.Count == 0)
                return;
            Debug.Log($"Input Array Count: {m_InputArray.Count}");
            TensorInt inputTensor = new TensorInt(new TensorShape(1, m_InputArray.Count), m_InputArray.ToArray());
            m_CurrWorker.Execute(inputTensor);
            TensorFloat outputTensor = m_CurrWorker.PeekOutput() as TensorFloat;
            if (outputTensor == null)
            {
                Debug.LogError("No Output!");
                return;
            }
            outputTensor.CompleteOperationsAndDownload();
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
        protected override void Reset()
        {
            m_VisemeList.Clear();
            m_CurrFrame = 0;
            base.Reset();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            if (IsSentisSupported)
                LoadSentis();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            if (IsSentisSupported)
                UnloadSentis();
        }
        protected virtual void ApplyMesh(VisemeData visemeData)
        {
            for (int i = 0; i < visemeData.visemeVal.Count; i++)
            {
                m_Skin.SetBlendShapeWeight(m_VisemeIndex + i, visemeData.visemeVal[i]);
            }
        }
    }
}

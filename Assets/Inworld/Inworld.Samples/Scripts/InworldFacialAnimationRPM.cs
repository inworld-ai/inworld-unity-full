/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Assets;
using Inworld.Packet;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using UnityEngine;

namespace Inworld.Sample.RPM
{
    public class InworldFacialAnimationRPM : InworldFacialAnimation
    {
        const int k_VisemeLength = 15;
        [SerializeField] protected LipsyncMap m_LipsyncMap;
        [SerializeField] InworldFacialEmotion m_FacialEmotion;

        [Range(0, 1)][SerializeField] float m_LipExpression = 0.7f;
        [Range(0, 1)][SerializeField] float m_MorphTime = 0.5f;
        [Header("For custom models:")]
        [Tooltip("Find the first viseme in the blendshape of your model. NOTE: Your viseme variables should be continuous and starting from Sil to U")]
        [SerializeField] protected string m_VisemeSil = "viseme_sil";
        [SerializeField] string m_BlinkBlendShape = "eyesClosed";
        [Tooltip("If your custom model is not working, try toggle this on/off")]
        [SerializeField] bool m_CustomModel;

        FacialAnimation m_LastFacial;
        FacialAnimation m_CurrentFacial;
        Vector2 m_CurrViseme = Vector2.zero;
        Vector2 m_LastViseme = Vector2.zero;
        float m_RandomOffset;

        [SerializeField] protected SkinnedMeshRenderer m_Skin;
        protected int m_VisemeIndex;
        protected int m_BlinkIndex;
        /// <summary>
        ///     YAN: We use Vector2 to store viseme data.
        ///     Vector2.x ==> Viseme Index (-1 = continue, add to next viseme)
        ///     Vector2.y ==> Duration
        /// </summary>
        ConcurrentQueue<Vector2> m_VisemeMap = new ConcurrentQueue<Vector2>();

        #region Properties
        /// <summary>
        ///     Get/Set the Inworld Character this component used.
        /// </summary>
        public InworldCharacter Character
        {
            get => m_Character; 
            set => m_Character = value;
        }
        #endregion
        
        /// <summary>
        /// Initialize the component, including finding the first index of the viseme of the character. 
        /// </summary>
        public void InitLipSync() => enabled = Init();
        
        protected virtual void ResetVisemeMap()
        {
            #if UNITY_2022_3_OR_NEWER
            m_VisemeMap.Clear();
            #else
            m_VisemeMap = new ConcurrentQueue<Vector2>();
            #endif
        }
        protected override bool Init()
        {
            if (!base.Init())
                return false;
            if (!m_Skin)
                m_Skin = m_Character.GetComponentInChildren<SkinnedMeshRenderer>();
            if (m_VisemeMap == null)
                m_VisemeMap = new ConcurrentQueue<Vector2>();
            m_RandomOffset = UnityEngine.Random.Range(0, 6f);
            ResetVisemeMap();
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
                    InworldAI.Log($"Find Viseme Index {m_VisemeIndex}");
                }
                if (m_Skin.sharedMesh.GetBlendShapeName(i) == m_BlinkBlendShape)
                {
                    m_BlinkIndex = i;
                    InworldAI.Log($"Find Blink Index {m_BlinkIndex}");
                }
            }
            return m_BlinkIndex + m_VisemeIndex != 0;
        }
        protected override void BlinkEyes()
        {
            if (!m_Skin)
                return;
            float blendshapeValue = Mathf.Sin(Time.time * 2f + m_RandomOffset) * 100 - 99f;
            blendshapeValue = Mathf.Clamp(blendshapeValue, 0, 1);
            m_Skin.SetBlendShapeWeight(m_BlinkIndex, blendshapeValue);
        }

        protected virtual void MorphSamplePhoneme(AudioPacket audioPacket)
        {
            foreach (PhonemeInfo phoneme in audioPacket.dataChunk.additionalPhonemeInfo)
            {
                PhonemeToViseme p2vRes = m_LipsyncMap.p2vMap.FirstOrDefault(p2v => p2v.phoneme == phoneme.phoneme);
                int visemeIndex = p2vRes?.visemeIndex ?? -1;
                m_VisemeMap.Enqueue(new Vector2(visemeIndex, phoneme.StartOffset));
            }
        }
        protected virtual void MorphLipsync()
        {
            if (!m_Character.IsSpeaking)
            {
                Reset();
                return;
            }
            // 1. Move Out-dated Viseme to Last Viseme.
            if (m_Interaction.AnimFactor >= m_CurrViseme.y)
            {
                if (m_LastViseme != Vector2.zero)
                    m_Skin.SetBlendShapeWeight(m_VisemeIndex + (int)m_LastViseme.x, 0);
                m_LastViseme = m_CurrViseme;
                m_CurrViseme = Vector2.zero;
            }
            // 2. Get New Viseme if Current Viseme is illegal.
            while (m_VisemeMap.Count > 0 && (m_CurrViseme.y == 0 || m_CurrViseme.x < 0))
            {
                m_VisemeMap.TryDequeue(out m_CurrViseme);
            }
            // 3. Do Morph.
            // Gradually decrease Last to 0
            if (m_LastViseme.y > 0 && m_LastViseme.x >= 0)
                _MorphViseme(m_LastViseme, false);
            // At the same time, gradually increase Current to 1.
            if (m_CurrViseme.y > 0 && m_CurrViseme.x >= 0)
                _MorphViseme(m_CurrViseme);
        }
        protected override void ProcessLipSync()
        {
            if (!m_Skin)
                return;
            MorphLipsync();
        }

        protected override void Reset()
        {
            ResetVisemeMap();
            m_CurrViseme = Vector2.zero;
            m_LastViseme = Vector2.zero;
            _ShutMouth();
        }
        void _MorphViseme(Vector2 viseme, bool isIncreasing = true)
        {
            if (viseme.x == 0)
            {
                _ShutMouth(); // YAN: Shut Immediately.
                return;
            }
            int visemeIndex = m_VisemeIndex + (int)viseme.x;
            float lastBlendShapeWeight = m_Skin.GetBlendShapeWeight(visemeIndex);
            float scale = Time.fixedDeltaTime / (m_CurrViseme.y - m_LastViseme.y);
            if (scale <= 0)
                return;
            scale = isIncreasing ? scale : -scale;
            float maxRange = m_CustomModel ? 100 : 1;
            scale *= maxRange;
            float newWeight = Mathf.Clamp(lastBlendShapeWeight + scale, 0, m_LipExpression * maxRange);
            m_Skin.SetBlendShapeWeight(visemeIndex, newWeight);
        }
        void _ShutMouth()
        {
            if (!m_Skin)
                return;
            m_Skin.SetBlendShapeWeight(m_VisemeIndex, 1);
            for (int i = 1; i < k_VisemeLength; i++)
            {
                m_Skin.SetBlendShapeWeight(m_VisemeIndex + i, 0);
            }
        }

        protected override void HandleLipSync(AudioPacket audioPacket)
        {
            Reset();
            if (audioPacket.Source == SourceType.PLAYER)
                return;
            if (audioPacket.dataChunk?.additionalPhonemeInfo == null)
                return;
            MorphSamplePhoneme(audioPacket);
        }
        protected override void HandleEmotion(EmotionPacket packet)
        {
            _ProcessEmotion(packet.emotion.behavior);
        }

        void _ProcessEmotion(SpaffCode emotion)
        {
            EmotionMapData emoMapData = m_EmotionMap[emotion];
            if (emoMapData == null)
            {
                InworldAI.LogError($"Unhandled emotion {emotion}");
                return;
            }
            FacialAnimation targetEmo = m_FacialEmotion[emoMapData.facialEmotion.ToString()];
            if (targetEmo == null)
            {
                InworldAI.LogError($"Unhandled emotion {emotion}");
                return;
            }
            _ResetLastEmo(m_LastFacial);
            m_LastFacial = m_CurrentFacial;
            m_CurrentFacial = targetEmo;
            StartCoroutine(_MorphEmotion());
        }
        void _ResetLastEmo(FacialAnimation emo)
        {
            if (!m_Skin || emo == null)
                return;
            for (int i = 0; i < m_Skin.sharedMesh.blendShapeCount; i++)
            {
                string currIterName = m_Skin.sharedMesh.GetBlendShapeName(i);
                MorphState lastState = emo.morphStates.FirstOrDefault(morph => morph.morphName == currIterName);
                if (lastState != null)
                    m_Skin.SetBlendShapeWeight(i, 0);
            }
        }
        IEnumerator _MorphEmotion()
        {
            if (!m_Skin)
                yield break;
            float morphTime = 0;
            while (morphTime < m_MorphTime)
            {
                for (int i = 0; i < m_Skin.sharedMesh.blendShapeCount; i++)
                {
                    string currIterName = m_Skin.sharedMesh.GetBlendShapeName(i);
                    float fCurrShapeWeight = m_Skin.GetBlendShapeWeight(i);
                    MorphState lastState = m_LastFacial?.morphStates.FirstOrDefault(morph => morph.morphName == currIterName);
                    MorphState currState = m_CurrentFacial?.morphStates.FirstOrDefault(morph => morph.morphName == currIterName);
                    // 1. Reset Old
                    if (lastState != null && currState == null)
                        m_Skin.SetBlendShapeWeight(i, Mathf.Lerp(fCurrShapeWeight, 0, 0.15f));
                    // 2. Apply New
                    if (currState != null)
                        m_Skin.SetBlendShapeWeight(i, Mathf.Lerp(fCurrShapeWeight, currState.morphWeight, 0.15f));
                }
                morphTime += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
        }
    }
}


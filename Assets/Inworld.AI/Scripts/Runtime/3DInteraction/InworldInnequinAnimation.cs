using Inworld.Audio;
using Inworld.Grpc;
using Inworld.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using AudioChunk = Inworld.Packets.AudioChunk;
using EmotionEvent = Inworld.Packets.EmotionEvent;
using InworldPacket = Inworld.Packets.InworldPacket;

namespace Inworld.Sample
{
    public class InworldInnequinAnimation : MonoBehaviour
    {
        [SerializeField] Animator m_EmoteAnimator;
        [SerializeField] EmotionMap m_EmotionMap;
        [SerializeField] SkinnedMeshRenderer m_FaceMesh;
        [SerializeField] Material m_Facial;
        [SerializeField] InworldInnequinFacialEmotion m_FaceTransformData;
        [SerializeField] LipsyncMap m_LipsyncMap;
        [SerializeField] Texture m_DefaultMouth;
        [Range(-1, 1)][SerializeField] float m_BlinkRate;
        List<Texture> m_LipsyncTextures = new List<Texture>();
        List<AdditionalPhonemeInfo> m_CurrentPhoneme = new List<AdditionalPhonemeInfo>();
        InworldCharacter m_Character;
        Interactions m_Interaction;
        Texture m_CurrentEyeOpen;
        Texture m_CurrentEyeClosed;
        
        Material m_matEyeBlow;
        Material m_matEye;
        Material m_matNose;
        Material m_matMouth;
        
        bool m_IsBlinking;
        float m_CurrentAudioTime;
        static readonly int s_Emotion = Animator.StringToHash("Emotion");
        
        const int k_VisemeSil = 0;
        const int k_VisemeCount = 15;

        void Awake()
        {
            enabled = _Init();
        }
        protected virtual void OnEnable()
        {
            InworldController.Instance.OnPacketReceived += OnInteractionChanged;
            if (!m_Character || m_Character.Interaction is not AudioInteraction audioInteraction)
                return;
            audioInteraction.OnAudioStarted += OnAudioStarted;
            audioInteraction.OnAudioEnd += OnAudioFinished;
        }

        protected virtual void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnPacketReceived -= OnInteractionChanged;
            if (!m_Character || m_Character.Interaction is not AudioInteraction audioInteraction)
                return;
            audioInteraction.OnAudioStarted -= OnAudioStarted;
            audioInteraction.OnAudioEnd -= OnAudioFinished;
        }
        void Start()
        {
            _InitMaterials();
        }
        void FixedUpdate()
        {
            _BlinkEyes();
            _ProcessLipSync();
        }
        bool _Init()
        {
            m_Character ??= GetComponent<InworldCharacter>();
            m_Interaction ??= GetComponent<Interactions>();
            return m_Character && m_Interaction;
        }
        void _InitMaterials()
        {
            m_matEyeBlow = _CreateFacialMaterial($"eyeBlow_{m_Character.Data.brain.GetHashCode()}");
            m_matEye = _CreateFacialMaterial($"eye_{m_Character.Data.brain.GetHashCode()}");
            m_matNose = _CreateFacialMaterial($"nose_{m_Character.Data.brain.GetHashCode()}");
            m_matMouth = _CreateFacialMaterial($"mouth_{m_Character.Data.brain.GetHashCode()}");
            Material[] materials = m_FaceMesh.materials;
            materials[0] = m_matEyeBlow;
            materials[1] = m_matEye;
            materials[2] = m_matNose;
            materials[3] = m_matMouth;
            m_FaceMesh.materials = materials;
            _MorphFaceEmotion(FacialEmotion.Neutral);
        }
        Material _CreateFacialMaterial(string matName)
        {
            Material instance = Instantiate(m_Facial);
            instance.name = matName;
            return instance;
        }
        void _MorphFaceEmotion(FacialEmotion emotion)
        {
            FaceTransform facialData = m_FaceTransformData[emotion.ToString()];
            if (facialData == null)
                return;
            m_matEyeBlow.mainTexture = facialData.eyeBlow;
            m_CurrentEyeOpen = facialData.eye;
            m_CurrentEyeClosed = facialData.eyeClosed;
            m_matNose.mainTexture = facialData.nose;
            m_DefaultMouth = facialData.mouthDefault;
            m_LipsyncTextures = facialData.mouth;
        }
        void _ProcessEmotion(string emotionBehavior)
        {
            EmotionMapData emoMapData = m_EmotionMap[emotionBehavior];
            if (emoMapData == null)
            {
                Debug.LogError($"Unhandled emotion {emotionBehavior}");
                return;
            }
            m_EmoteAnimator.SetInteger(s_Emotion, (int)emoMapData.emoteAnimation);
            _MorphFaceEmotion(emoMapData.facialEmotion);
        }
        void _BlinkEyes()
        {
            m_IsBlinking = Mathf.Sin(Time.time) < m_BlinkRate;
            m_matEye.mainTexture = m_IsBlinking ? m_CurrentEyeClosed : m_CurrentEyeOpen;
        }
        void _ResetMouth()
        {
            m_matMouth.mainTexture = m_LipsyncTextures.Count == k_VisemeCount ? m_LipsyncTextures[k_VisemeSil] : m_DefaultMouth;
        }
        void _ProcessLipSync()
        {
            if (!m_Character.IsSpeaking)
            {
                _ResetMouth();
                return;
            }
            m_CurrentAudioTime += Time.deltaTime;
            AdditionalPhonemeInfo data = m_CurrentPhoneme.LastOrDefault(p => p.StartOffset.ToTimeSpan().TotalSeconds < m_CurrentAudioTime);
            if (data == null || string.IsNullOrEmpty(data.Phoneme))
            {
                _ResetMouth();
                return;
            }
            PhonemeToViseme p2v = m_LipsyncMap.p2vMap.FirstOrDefault(v => v.phoneme == data.Phoneme);
            if (p2v == null)
            {
                InworldAI.LogError($"Not Found! {data.Phoneme}");
                return;
            }
            if (p2v.visemeIndex >= 0 && p2v.visemeIndex < m_LipsyncTextures.Count)
                m_matMouth.mainTexture = m_LipsyncTextures[p2v.visemeIndex];
        }
        protected virtual void OnInteractionChanged(InworldPacket packet)
        {
            switch (packet)
            {
                case EmotionEvent emotionPacket:
                    HandleEmotion(emotionPacket);
                    break;
            }
        }

        protected void HandleLipSync(AudioChunk audioPacket)
        {
            m_CurrentAudioTime = 0;
            m_CurrentPhoneme = audioPacket.PhonemeInfo.ToList();
        }
        protected void HandleEmotion(EmotionEvent packet)
        {
            _ProcessEmotion(packet.SpaffCode.ToString().ToUpper());
        }
        void OnAudioStarted()
        {
            _ResetMouth();
            if (!m_Character || m_Character.Interaction is not AudioInteraction audioInteraction || audioInteraction.CurrentChunk == null)
                return;
            HandleLipSync(audioInteraction.CurrentChunk);
        }
        void OnAudioFinished()
        {
            _ResetMouth();
        }
    }
}

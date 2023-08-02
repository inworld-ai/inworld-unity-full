using Inworld.Assets;
using Inworld.Interactions;
using Inworld.Packet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inworld.Sample.Innequin
{
    public class InworldFaceAnimationInnequin : MonoBehaviour
    {
        [SerializeField] Animator m_EmoteAnimator;
        [SerializeField] EmotionMap m_EmotionMap;
        [SerializeField] SkinnedMeshRenderer m_FaceMesh;
        [SerializeField] FaceTransformData m_FaceTransformData;
        [SerializeField] LipsyncMap m_FaceAnimData;
        [SerializeField] Texture m_DefaultMouth;
        [SerializeField] Material m_Facial;
        [Range(-1, 1)][SerializeField] float m_BlinkRate;
        List<Texture> m_LipsyncTextures = new List<Texture>();
        List<PhonemeInfo> m_CurrentPhoneme = new List<PhonemeInfo>();
        InworldCharacter m_Character;
        InworldInteraction m_Interaction;
        Texture m_CurrentEyeOpen;
        Texture m_CurrentEyeClosed;
        
        Material m_matEyeBlow;
        Material m_matEye;
        Material m_matNose;
        Material m_matMouth;
        
        bool m_IsBlinking;
        float m_CurrentAudioTime;
        static readonly int s_Emotion = Animator.StringToHash("Emotion");
        static readonly int s_SrcBlend = Shader.PropertyToID("_SrcBlend");
        static readonly int s_DstBlend = Shader.PropertyToID("_DstBlend");
        static readonly int s_ZWrite = Shader.PropertyToID("_ZWrite");
        const int k_VisemeSil = 0;
        const int k_VisemeCount = 15;

        void Awake()
        {
            enabled = _Init();
        }
        protected virtual void OnEnable()
        {
            InworldController.Instance.OnCharacterInteraction += OnInteractionChanged;
        }

        protected virtual void OnDisable()
        {
            if (InworldController.Instance)
                InworldController.Instance.OnCharacterInteraction -= OnInteractionChanged;
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
            m_Interaction ??= GetComponent<InworldInteraction>();
            return m_Character && m_Interaction;
        }
        void _InitMaterials()
        {
            m_matEyeBlow = _CreateFacialMaterial($"eyeBlow_{m_Character.Data.brainName.GetHashCode()}");
            m_matEye = _CreateFacialMaterial($"eye_{m_Character.Data.brainName.GetHashCode()}");
            m_matNose = _CreateFacialMaterial($"nose_{m_Character.Data.brainName.GetHashCode()}");
            m_matMouth = _CreateFacialMaterial($"mouth_{m_Character.Data.brainName.GetHashCode()}");
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
            if (m_LipsyncTextures.Count == k_VisemeCount)
                m_matMouth.mainTexture = m_LipsyncTextures[k_VisemeSil];
            else
                m_matMouth.mainTexture = m_DefaultMouth;
        }
        void _ProcessLipSync()
        {
            if (!m_Interaction.IsSpeaking)
            {
                _ResetMouth();
                return;
            }
            m_CurrentAudioTime += Time.deltaTime;
            PhonemeInfo data = m_CurrentPhoneme.LastOrDefault(p => p.startOffset < m_CurrentAudioTime);
            if (data == null || string.IsNullOrEmpty(data.phoneme))
            {
                _ResetMouth();
                return;
            }
            PhonemeToViseme p2v = m_FaceAnimData.p2vMap.FirstOrDefault(v => v.phoneme == data.phoneme);
            if (p2v == null)
            {
                Debug.LogError($"Not Found! {data.phoneme}");
                return;
            }
            if (p2v.visemeIndex >= 0 && p2v.visemeIndex < m_LipsyncTextures.Count)
                m_matMouth.mainTexture = m_LipsyncTextures[p2v.visemeIndex];
        }
        protected virtual void OnInteractionChanged(InworldPacket packet)
        {
            if (m_Character &&
                !string.IsNullOrEmpty(m_Character.ID) &&
                packet?.routing?.source?.name == m_Character.ID || packet?.routing?.target?.name == m_Character.ID)
                ProcessPacket(packet);
        }
        protected virtual void ProcessPacket(InworldPacket incomingPacket)
        {
            switch (incomingPacket)
            {
                case AudioPacket audioPacket: // Already Played.
                    HandleLipSync(audioPacket);
                    break;
                case EmotionPacket emotionPacket:
                    HandleEmotion(emotionPacket);
                    break;
            }
        }
        protected void HandleLipSync(AudioPacket audioPacket)
        {
            m_CurrentAudioTime = 0;
            m_CurrentPhoneme = audioPacket.dataChunk.additionalPhonemeInfo;
        }
        protected void HandleEmotion(EmotionPacket packet)
        {
            _ProcessEmotion(packet.emotion.behavior.ToUpper());
        }
    }
}

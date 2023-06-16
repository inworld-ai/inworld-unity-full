using UnityEngine;
using Inworld.Interactions;
using Inworld.Packet;
using Inworld.Assets;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace Inworld.Sample
{
    [RequireComponent(typeof(InworldAudioInteraction))]
    public class InworldCharacter3D : InworldCharacter
    {
        [SerializeField] Transform m_PlayerCamera;
        [SerializeField] Animator m_BodyAnimator;
        [SerializeField] Animator m_EmoteAnimator;
        [SerializeField] SkinnedMeshRenderer m_FaceMesh;
        [SerializeField] FacialAnimationData m_FaceAnimData;
        [SerializeField] FaceTransformData m_FaceTransformData;
        [SerializeField] EmotionMap m_EmotionMap;
        [SerializeField] Texture m_DefaultMouth;
        [Range(-1, 1)][SerializeField] float m_BlinkRate;
        
        static readonly int s_Emotion = Animator.StringToHash("Emotion");
        static readonly int s_Gesture = Animator.StringToHash("Gesture");
        static readonly int s_RemainSec = Animator.StringToHash("RemainSec");
        static readonly int s_Random = Animator.StringToHash("Random");
        static readonly int s_Motion = Animator.StringToHash("MainStatus");
        const int k_VisemeSil = 0;
        const int k_VisemeCount = 15;
        Transform m_trLookAt;
        Transform m_Transform;
        Vector3 m_vecInitPosition;
        Vector3 m_vecInitEuler;
        Material m_matEyeBlow;
        Material m_matEye;
        Material m_matNose;
        Material m_matMouth;
        Texture m_CurrentEyeOpen;
        Texture m_CurrentEyeClosed;
        float m_LookAtWeight;
        float m_CurrentAudioTime;
        bool m_IsBlinking;
        List<Texture> m_LipsyncTextures = new List<Texture>();
        List<PhonemeInfo> m_CurrentPhoneme = new List<PhonemeInfo>();
        
        static readonly int s_SrcBlend = Shader.PropertyToID("_SrcBlend");
        static readonly int s_DstBlend = Shader.PropertyToID("_DstBlend");
        static readonly int s_ZWrite = Shader.PropertyToID("_ZWrite");

        public void HandleMainStatus(AnimMainStatus status) => m_BodyAnimator.SetInteger(s_Motion, (int)status);

        protected override void OnStartStopInteraction(bool isStarting)
        {
            if (isStarting)
            {
                HandleMainStatus(AnimMainStatus.Talking);
            }
            else
            {
                HandleMainStatus(AnimMainStatus.Neutral);
            }
            base.OnStartStopInteraction(isStarting);
        }
        protected override void OnCharRegistered(InworldCharacterData charData)
        {
            if (charData.brainName == Data.brainName)
                RegisterLiveSession();
        }
        protected override void OnCharChanged(InworldCharacter oldChar, InworldCharacter newChar)
        {
            if (oldChar != null && oldChar.BrainName == Data.brainName)
            {
                m_trLookAt = null;
                HandleMainStatus(AnimMainStatus.Goodbye);
                m_BodyAnimator.enabled = false;
            }
            if (newChar != null && newChar.BrainName == Data.brainName)
            {
                m_trLookAt = m_PlayerCamera ? m_PlayerCamera : Camera.main.transform;
                m_BodyAnimator.enabled = true;
                HandleMainStatus(AnimMainStatus.Hello);
            }
        }
        protected override void OnEnable()
        {
            m_Transform = transform;
            m_vecInitEuler = m_Transform.localEulerAngles;
            m_vecInitPosition = m_Transform.localPosition;
            base.OnEnable();
        }
        void Start()
        {
            _InitMaterials();
        }
        void Update()
        {
            _BlinkEyes();
            _ProcessLipSync();
        }
        void OnAnimatorIK(int layerIndex)
        {
            if (!m_BodyAnimator)
                return;
            if (m_trLookAt == null)
            {
                _StopLookAt();
                return;
            }
            _StartLookAt(m_trLookAt.position);
        }
        protected override void OnStatusChanged(InworldConnectionStatus newStatus)
        {
            if (newStatus == InworldConnectionStatus.Connected)
                InworldController.Instance.CurrentCharacter = this;
        }
        protected override void HandleLipSync(AudioPacket audioPacket)
        {
            m_CurrentAudioTime = 0;
            m_CurrentPhoneme = audioPacket.dataChunk.additionalPhonemeInfo;
        }
        protected override void HandleEmotion(EmotionPacket packet)
        {
            m_BodyAnimator.SetFloat(s_Random, Random.Range(0, 1) > 0.5f ? 1 : 0);
            m_BodyAnimator.SetFloat(s_RemainSec, m_Interaction.AudioLength);
            _ProcessEmotion(packet.emotion.behavior.ToUpper());
            base.HandleEmotion(packet);
        }
        void _ProcessEmotion(string emotionBehavior)
        {
            EmotionMapData emoMapData = m_EmotionMap[emotionBehavior];
            if (emoMapData == null)
            {
                Debug.LogError($"Unhandled emotion {emotionBehavior}");
                return;
            }
            m_BodyAnimator.SetInteger(s_Emotion, (int)emoMapData.bodyEmotion);
            m_BodyAnimator.SetInteger(s_Gesture, (int)emoMapData.bodyGesture);
            m_EmoteAnimator.SetInteger(s_Emotion, (int)emoMapData.emoteAnimation);
            _MorphFaceEmotion(emoMapData.facialEmotion);
        }
        void _InitMaterials()
        {
            m_matEyeBlow = _CreateFacialMaterial($"eyeBlow_{Data.brainName.GetHashCode()}");
            m_matEye = _CreateFacialMaterial($"eye_{Data.brainName.GetHashCode()}");
            m_matNose = _CreateFacialMaterial($"nose_{Data.brainName.GetHashCode()}");
            m_matMouth = _CreateFacialMaterial($"mouth_{Data.brainName.GetHashCode()}");
            Material[] materials = m_FaceMesh.materials;
            materials[0] = m_matEyeBlow;
            materials[1] = m_matEye;
            materials[2] = m_matNose;
            materials[3] = m_matMouth;
            m_FaceMesh.materials = materials;
            _MorphFaceEmotion(FacialEmotion.Neutral);
        }
        void _BlinkEyes()
        {
            m_IsBlinking = Mathf.Sin(Time.time) < m_BlinkRate;
            m_matEye.mainTexture = m_IsBlinking ? m_CurrentEyeClosed : m_CurrentEyeOpen;
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
            Assets.PhonemeToViseme p2v = m_FaceAnimData.p2vMap.FirstOrDefault(v => v.phoneme == data.phoneme);
            if (p2v == null)
            {
                Debug.LogError($"Not Found! {data.phoneme}");
                return;
            }
            if (p2v.visemeIndex >= 0 && p2v.visemeIndex < m_LipsyncTextures.Count)
                m_matMouth.mainTexture = m_LipsyncTextures[p2v.visemeIndex];
        }
        void _ResetMouth()
        {
            if (m_LipsyncTextures.Count == k_VisemeCount)
                m_matMouth.mainTexture = m_LipsyncTextures[k_VisemeSil];
            else
                m_matMouth.mainTexture = m_DefaultMouth;
        }
        void _StartLookAt(Vector3 lookPos)
        {
            m_LookAtWeight = Mathf.Clamp(m_LookAtWeight + 0.01f, 0, 1);
            m_BodyAnimator.SetLookAtWeight(m_LookAtWeight);
            m_BodyAnimator.SetLookAtPosition(lookPos);
        }
        void _StopLookAt()
        {
            m_Transform.localPosition = m_vecInitPosition;
            m_Transform.localEulerAngles = m_vecInitEuler;
            m_LookAtWeight = Mathf.Clamp(m_LookAtWeight - 0.01f, 0, 1);
            m_BodyAnimator.SetLookAtWeight(m_LookAtWeight);
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
        Material _CreateFacialMaterial(string matName)
        {
            int minRenderQueue = -1;
            int maxRenderQueue = 5000;
            int defaultRenderQueue = -1;
            Material matResult = new Material(Shader.Find("Standard"));
            matResult.name = matName;
            matResult.SetOverrideTag("RenderType", "Transparent");
            matResult.SetFloat(s_SrcBlend, (float)UnityEngine.Rendering.BlendMode.One);
            matResult.SetFloat(s_DstBlend, (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            matResult.SetFloat(s_ZWrite, 0.0f);
            matResult.DisableKeyword("_ALPHATEST_ON");
            matResult.DisableKeyword("_ALPHABLEND_ON");
            matResult.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            minRenderQueue = (int)UnityEngine.Rendering.RenderQueue.GeometryLast + 1;
            maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Overlay - 1;
            defaultRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            if (matResult.renderQueue < minRenderQueue || matResult.renderQueue > maxRenderQueue)
            {
                matResult.renderQueue = defaultRenderQueue;
            }
            return matResult;
        }
    }
}

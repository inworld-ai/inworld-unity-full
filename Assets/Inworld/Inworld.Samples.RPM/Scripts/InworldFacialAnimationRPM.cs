using Inworld.Assets;
using Inworld.Packet;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using UnityEngine;

namespace Inworld.Sample.RPM
{
    public class InworldFacialAnimationRPM : MonoBehaviour
    {
        const int k_VisemeLength = 15;
        [SerializeField] LipsyncMap m_LipsyncMap;
        [SerializeField] InworldFacialEmotion m_FacialEmotion;

        [Range(0, 1)][SerializeField] float m_LipExpression = 0.7f;
        [Range(0, 1)][SerializeField] float m_MorphTime = 0.5f;
        [Header("For custom models:")]
        [Tooltip("Find the first viseme in the blendshape of your model. NOTE: Your viseme variables should be continuous and starting from Sil to U")]
        [SerializeField] string m_VisemeSil = "viseme_sil";
        [SerializeField] string m_BlinkBlendShape = "eyesClosed";
        [Tooltip("If your custom model is not working, try toggle this on/off")]
        [SerializeField] bool m_CustomModel;
        FacialAnimation m_LastFacial;
        FacialAnimation m_CurrentFacial;
        Vector2 m_CurrViseme = Vector2.zero;
        Vector2 m_LastViseme = Vector2.zero;

        float m_CurrentAudioTime;

        SkinnedMeshRenderer m_Skin;
        int m_VisemeIndex;
        int m_BlinkIndex;
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
        public InworldCharacter Character { get; set; }
        #endregion
        
        void Awake()
        {
            Init();
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

        void FixedUpdate()
        {
            _BlinkEyes();
            _MorphLipsync();
        }
        
        public void Init()
        {
            Character ??= GetComponent<InworldCharacter>();
            m_Skin ??= Character.GetComponentInChildren<SkinnedMeshRenderer>();
            m_VisemeMap ??= new ConcurrentQueue<Vector2>();
            m_VisemeMap.Clear();
            _MappingBlendShape();
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
        void _BlinkEyes()
        {
            if (!m_Skin)
                return;
            float blendshapeValue = Mathf.Sin(Time.time * 2f) * 100 - 99f;
            blendshapeValue = Mathf.Clamp(blendshapeValue, 0, 1);
            m_Skin.SetBlendShapeWeight(m_BlinkIndex, blendshapeValue);
        }
        void _MorphLipsync()
        {
            if (!m_Skin)
                return;
            // 1. Move Out-dated Viseme to Last Viseme.
            if (m_CurrentAudioTime >= m_CurrViseme.y)
            {
                if (m_LastViseme != Vector2.zero)
                    m_Skin.SetBlendShapeWeight(m_VisemeIndex + (int)m_LastViseme.x, 0);
                m_LastViseme = m_CurrViseme;
                m_CurrViseme = Vector2.zero;
            }
            m_CurrentAudioTime += Time.fixedDeltaTime;
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

        void _Reset()
        {
            m_VisemeMap.Clear();
            m_CurrViseme = Vector2.zero;
            m_LastViseme = Vector2.zero;
            m_CurrentAudioTime = 0;
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
        void OnInteractionChanged(InworldPacket packet)
        {
            if (Character &&
                !string.IsNullOrEmpty(Character.ID) &&
                packet?.routing?.source?.name == Character.ID || packet?.routing?.target?.name == Character.ID)
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
            _Reset();
            if (audioPacket.dataChunk == null || audioPacket.dataChunk.additionalPhonemeInfo == null)
                return;
            foreach (var phoneme in audioPacket.dataChunk.additionalPhonemeInfo)
            {
                PhonemeToViseme p2vRes = m_LipsyncMap.p2vMap.FirstOrDefault(p2v => p2v.phoneme == phoneme.phoneme);
                int visemeIndex = p2vRes?.visemeIndex ?? -1;
                m_VisemeMap.Enqueue(new Vector2(visemeIndex, phoneme.startOffset));
            }
        }
        protected void HandleEmotion(EmotionPacket packet)
        {
            _ProcessEmotion(packet.emotion.behavior.ToUpper());
        }

        void _ProcessEmotion(string emotion)
        {
            FacialAnimation targetEmo = m_FacialEmotion.emotions.FirstOrDefault(emo => emo.emotion.ToUpper() == emotion);
            if (targetEmo != null && m_CurrentFacial != targetEmo)
            {
                _ResetLastEmo(m_LastFacial);
                m_LastFacial = m_CurrentFacial;
                m_CurrentFacial = targetEmo;
                StartCoroutine(_MorphEmotion());
            }
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


using Inworld.Assets;
using Inworld.Interactions;
using UnityEngine;

namespace Inworld.Sample
{
    [RequireComponent(typeof(InworldAudioInteraction))]
    public class InworldRPMCharacter : InworldCharacter
    {
        [SerializeField] Transform m_PlayerCamera;
        [SerializeField] Animator m_BodyAnimator;
        [SerializeField] SkinnedMeshRenderer m_FaceMesh;
        [SerializeField] LipsyncMap m_FaceAnimData;
        [Range(-1, 1)][SerializeField] float m_BlinkRate;
        
        static readonly int s_Emotion = Animator.StringToHash("Emotion");
        static readonly int s_Gesture = Animator.StringToHash("Gesture");
        static readonly int s_RemainSec = Animator.StringToHash("RemainSec");
        static readonly int s_Random = Animator.StringToHash("Random");
        static readonly int s_Motion = Animator.StringToHash("MainStatus");
        const int k_VisemeSil = 0;
        const int k_VisemeCount = 15;
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
        protected virtual void OnEnable()
        {
            InworldController.Instance.OnCharacterRegistered += OnCharRegistered;
            InworldController.Instance.OnCharacterChanged += OnCharChanged;
            InworldController.Client.OnStatusChanged += OnStatusChanged;
            m_Interaction.OnStartStopInteraction += OnStartStopInteraction;
            m_Interaction.OnInteractionChanged += OnInteractionChanged;
        }

        protected virtual void OnDisable()
        {
            m_Interaction.OnStartStopInteraction -= OnStartStopInteraction;
            m_Interaction.OnInteractionChanged -= OnInteractionChanged;
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnCharacterRegistered -= OnCharRegistered;
            InworldController.Instance.OnCharacterChanged -= OnCharChanged;
            InworldController.Client.OnStatusChanged -= OnStatusChanged;
        }
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        protected override void OnStatusChanged(InworldConnectionStatus newStatus)
        {
            if (newStatus == InworldConnectionStatus.Connected)
            {
                InworldController.Instance.CurrentCharacter = this;
                InworldController.Instance.StartAudio();
            }
        }
        protected override void OnCharRegistered(InworldCharacterData charData)
        {
            if (charData.brainName == Data.brainName)
                RegisterLiveSession();
            else
            {
                Debug.LogError($"My Brain: {BrainName} Received: {charData.brainName}");
            }
        }
    }
    
}


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
        [SerializeField] FacialAnimationData m_FaceAnimData;
        [Range(-1, 1)][SerializeField] float m_BlinkRate;
        
        static readonly int s_Emotion = Animator.StringToHash("Emotion");
        static readonly int s_Gesture = Animator.StringToHash("Gesture");
        static readonly int s_RemainSec = Animator.StringToHash("RemainSec");
        static readonly int s_Random = Animator.StringToHash("Random");
        static readonly int s_Motion = Animator.StringToHash("MainStatus");
        
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
            base.OnEnable();
        }

        protected virtual void OnDisable()
        {
            base.OnDisable();
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
                if(InworldController.Instance.CurrentCharacter == null)
                    InworldController.Instance.CurrentCharacter = this;
                else if(InworldController.Instance.CurrentCharacter.Priority < this.Priority)
                {
                    InworldController.Instance.CurrentCharacter = this;
                }
            }
        }
    }
    
}


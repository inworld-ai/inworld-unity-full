/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Packets;
using Inworld.Util;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
namespace Inworld
{
    public class InworldCharacter : MonoBehaviour
    {
        #region Inspector Variables
        [Header("Data:")]
        [SerializeField] InworldCharacterData m_Data;
        [FormerlySerializedAs("m_AudioInteraction")]
        [Header("Audio")]
        [SerializeField] Interactions m_Interaction;
        [Header("Animation")]
        [SerializeField] GameObject m_CurrentAvatar;
        [Header("Sight")]
        [Range(1, 180)]
        [SerializeField] float m_SightAngle = 90f;
        [Range(1, 100)]
        [SerializeField] float m_SightDistance = 10f;
        [SerializeField] float m_SightRefreshRate = 0.25f;
        [Header("Log")]
        [SerializeField] bool m_logUtterances;
        public UnityEvent OnBeginSpeaking;
        public UnityEvent OnFinishedSpeaking;
        public UnityEvent<string, string> OnCharacterSpeaks;
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        public bool IsSpeaking
        {
            get => m_Interaction && m_Interaction.isSpeaking;
            internal set
            {
                if (!m_Interaction)
                    return;
                if (value && !m_Interaction.isSpeaking)
                {
                    m_Interaction.isSpeaking = true;
                    OnBeginSpeaking.Invoke();
                }
                else if (!value && m_Interaction.isSpeaking)
                {
                    m_Interaction.isSpeaking = false;
                    OnFinishedSpeaking.Invoke();
                }
            }
        }
        /// <summary>
        ///     Returns Unity Event of Interaction.
        /// </summary>
        public InteractionEvent InteractionEvent { get; } = new InteractionEvent();
        public bool IsMute
        {
            get
            {
                if (PlaybackSource)
                    return true;
                return PlaybackSource.volume == 0;
            }
            set
            {
                if (PlaybackSource)
                    PlaybackSource.volume = value ? 0 : 1;
            }
        }
        public AudioSource PlaybackSource { get; set; }
        /// <summary>
        ///     This Character's Data.
        /// </summary>
        public InworldCharacterData Data => m_Data;

        /// <summary>
        ///     This Character's ID.
        ///     ID would only be generated in Runtime when session started.
        /// </summary>
        public string ID => Data ? Data.characterID : "";

        /// <summary>
        ///     This Character's Name.
        ///     NOTE:
        ///     CharacterName may not be unique.
        /// </summary>
        public string CharacterName => Data ? Data.characterName : "";

        /// <summary>
        ///     This Character's Brain Name.
        ///     NOTE:
        ///     Brain Name is actually the character's Full Name, format like  "workspace/xxx/characters/xxx".
        ///     It's unique.
        /// </summary>
        public string BrainName => Data ? Data.brain : "";

        /// <summary>
        ///     Return if the Inworld Character has Data.
        /// </summary>
        public bool IsValid => Data != null;

        /// <summary>
        ///     Check if this Inworld Character has been registered in the live session.
        /// </summary>
        public bool HasRegisteredLiveSession => !string.IsNullOrEmpty(ID);

        /// <summary>
        ///     Returns the priority of the character.
        ///     the higher the Priority is, the character is more likely responding to player.
        /// </summary>
        public float Priority { get; private set; }

        /// <summary>
        ///     Returns the current remaining time of the Inworld character's playing audio.
        ///     This data would be modified by `AudioInteraction`.
        /// </summary>
        public float CurrentAudioRemainingTime { get; set; }
        /// <summary>
        ///     Get/Set the Inworld Character's Audio Interaction.
        /// </summary>
        public Interactions Interaction
        {
            get => m_Interaction;
            set => m_Interaction = value;
        }
        /// <summary>
        ///     Get/Set the model it bound.
        /// </summary>
        public GameObject CurrentAvatar
        {
            // Yan: Cannot just Get Set as we need an initial data.
            get => m_CurrentAvatar;
            set => m_CurrentAvatar = value;
        }
        /// <summary>
        ///     Get the Character's current emotion.
        /// </summary>
        public string Emotion { get; internal set; } = "Neutral";
        /// <summary>
        ///     Get the Character's current gesture.
        /// </summary>
        public string Gesture { get; internal set; } = "Neutral";
        #endregion

        #region Monobehavior Functions
        void Awake()
        {
            if (!m_Interaction || !m_Interaction.enabled)
            {
                m_Interaction = gameObject.AddComponent<Interactions>();
                m_Interaction.enabled = true;
            }
            InworldController.Instance.RegisterCharacter(this);
        }
        void Start()
        {
            InworldController.Instance.OnCharacterChanged += OnCharacterChanged;
            OnBeginSpeaking?.AddListener(() =>
            {
                if (m_logUtterances)
                    InworldAI.Log($"{CharacterName} Started talking");
            });
            OnFinishedSpeaking?.AddListener(() =>
            {
                if (m_logUtterances)
                    InworldAI.Log($"{CharacterName} Finshed talking");
            });
            OnCharacterSpeaks?.AddListener((character, text) =>
            {
                if (m_logUtterances && !string.IsNullOrEmpty(text))
                    InworldAI.Log($"{character}: {text}");
            });
            StartCoroutine(CheckPriority());
        }
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 trPosition = transform.position;
            for (float angle = m_SightAngle * -0.5f; angle < m_SightAngle * 0.5f; angle += m_SightAngle * 0.05f)
            {
                Gizmos.DrawLine(trPosition, trPosition + Quaternion.AngleAxis(angle, transform.up) * transform.forward * m_SightDistance);
            }
            Gizmos.color = Color.red;

            if (!InworldController.Instance || !InworldController.Player)
                return;
            Vector3 vecDirection = (InworldController.Player.transform.position - trPosition).normalized;
            Gizmos.DrawLine(trPosition, trPosition + transform.forward * m_SightDistance);
            Gizmos.DrawLine(trPosition, trPosition + vecDirection * m_SightDistance);
        }
        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnCharacterChanged -= OnCharacterChanged;
        }
        #endregion

        #region Callbacks
        void OnCharacterChanged(InworldCharacter oldChar, InworldCharacter newChar)
        {
            if (oldChar == this)
            {
                _EndAudioCapture();
            }
            else if (newChar == this)
            {
                StartCoroutine(_StartAudioCapture());
            }

        }
        void OnAvatarLoaded(InworldCharacterData obj)
        {
            if (obj.brain != Data.brain)
                return; // Not Me.
            GameObject objAvatar = obj.Avatar;
            InworldAI.AvatarLoader.ConfigureModel(this, objAvatar);
        }
        #endregion

        #region Private Functions
        IEnumerator _StartAudioCapture()
        {
            yield return new WaitForSeconds(0.25f);
            InworldAI.Log($"Start Communicating with {CharacterName}: {ID}");
            InworldController.Instance.StartAudioCapture(ID);
        }
        void _EndAudioCapture()
        {
            InworldAI.Log($"End Communicating with {CharacterName}: {ID}");
            InworldController.Instance.EndAudioCapture(ID);
        }
        internal void RegisterLiveSession(string agentID)
        {
            if (!Data)
            {
                InworldAI.LogError("Error: No Data!");
                return;
            }
            Data.characterID = agentID;
        }
        IEnumerator CheckPriority()
        {
            // YAN: Update refreshed too fast. Use Coroutine for better performance.
            while (InworldController.Instance && InworldController.Player)
            {
                Transform trCharacter = transform;
                Transform trPlayer = InworldController.Player.transform;
                Priority = Vector3.Distance(trCharacter.position, trPlayer.position);
                if (Priority > m_SightDistance)
                    Priority = -1f;
                else
                {
                    Vector3 vecDirection = (trPlayer.position - trCharacter.position).normalized;
                    float fAngle = Vector3.Angle(vecDirection, trCharacter.forward);
                    if (fAngle > m_SightAngle * 0.5f)
                    {
                        Priority = -1f;
                    }
                    else
                    {
                        Vector3 vecPlayerDirection = -vecDirection;
                        Priority = Vector3.Angle(vecPlayerDirection, trPlayer.forward);
                        ;
                    }
                }
                yield return new WaitForSeconds(m_SightRefreshRate);
            }
        }
        #endregion

        #region APIs
        /// <summary>
        ///     Let InworldCharacter to bind data.
        /// </summary>
        /// <param name="incomingData">The Inworld Character Data to bind</param>
        /// <param name="model">
        ///     The model to bind.
        ///     NOTE: This model could be any kind of model (Even a cube),
        ///     If the input model is null,
        ///     it'll download a model and then attach to it if it has valuable modelUri,
        ///     Otherwise, it'll attach to default avatar.
        /// </param>
        /// <param name="addModel">Set to true if you want to support model</param>
        public void LoadCharacter(InworldCharacterData incomingData, GameObject model = null, bool addModel = true)
        {
            m_Data = incomingData;
            transform.name = incomingData.characterName;
            if (Application.isPlaying || !addModel)
                return;
            if (model)
            {
                InworldAI.AvatarLoader.ConfigureModel(this, model);
            }
            else
            {
                GameObject objAvatar = incomingData.Avatar;
                if (!objAvatar)
                {
                    InworldAI.File.OnAvatarDownloaded += OnAvatarLoaded;
                    InworldAI.File.DownloadAvatar(incomingData);
                }
                else
                {
                    GameObject avatarInstance = Instantiate(objAvatar);
                    avatarInstance.transform.name = "Armature";
                    InworldAI.AvatarLoader.ConfigureModel(this, avatarInstance);
                }
            }
        }
        /// <summary>
        ///     Reset Character's history items and cleanup Audio Caches
        /// </summary>
        public void ResetCharacter()
        {
            // Clearing a queue.
            if (m_Interaction)
                m_Interaction.Clear();
        }
        /// <summary>
        ///     Send Text to this Character via InworldPacket.
        /// </summary>
        /// <param name="text">string of the Text.</param>
        public void SendText(string text)
        {
            SendEventToAgent
            (
                new TextEvent
                {
                    Text = text,
                    SourceType = Grpc.TextEvent.Types.SourceType.TypedIn,
                    Final = true
                }
            );
        }
        /// <summary>
        ///     Send target character's trigger via InworldPacket.
        /// </summary>
        /// <param name="triggerName">
        ///     The trigger to send. Both formats are acceptable.
        ///     You could send either whole string from CharacterData.trigger, or the trigger's shortName.
        /// </param>
        public void SendTrigger(string triggerName)
        {
            string[] trigger = triggerName.Split("triggers/");
            SendEventToAgent(trigger.Length == 2 ? new CustomEvent(trigger[1]) : new CustomEvent(triggerName));
        }
        /// <summary>
        ///     Set general events to this Character.
        /// </summary>
        /// <param name="packet">The InworldPacket to send.</param>
        public void SendEventToAgent(InworldPacket packet)
        {
            if (!Data || string.IsNullOrEmpty(ID))
                return;
            packet.Routing = Routing.FromPlayerToAgent(ID);
            if (packet is TextEvent text)
            {
                // Adding text to history if interactions.
                if (m_Interaction)
                    m_Interaction.AddText(text);
            }
            InworldController.Instance.SendEvent(packet);
        }
        #endregion
    }
}

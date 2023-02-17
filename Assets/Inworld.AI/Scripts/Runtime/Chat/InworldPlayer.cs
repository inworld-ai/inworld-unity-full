/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Runtime;
using Inworld.Sample.UI;
using Inworld.Util;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
namespace Inworld.Sample
{
    /// <summary>
    ///     This is the class for global text management, by original, it's added in Player Controller.
    ///     And would be called by Keycode.Backquote.
    /// </summary>
    public class InworldPlayer : MonoBehaviour
    {
        #region Inspector Variables
        [SerializeField] InworldCameraController m_CameraController;
        [SerializeField] GameObject m_GlobalChatCanvas;
        [SerializeField] GameObject m_TriggerCanvas;
        [SerializeField] RecordButton m_RecordButton;
        [SerializeField] RectTransform m_ContentRT;
        [SerializeField] ChatBubble m_BubbleLeft;
        [SerializeField] ChatBubble m_BubbleRight;
        [SerializeField] TMP_InputField m_InputField;
        [SerializeField] RuntimeCanvas m_RTCanvas;
        [SerializeField] Vector3 m_InitPosition;
        [SerializeField] Vector3 m_InitRotation;
        #endregion

        #region Private Variables
        readonly Dictionary<string, ChatBubble> m_Bubbles = new Dictionary<string, ChatBubble>();
        readonly Dictionary<string, InworldCharacter> m_Characters = new Dictionary<string, InworldCharacter>();
        Vector2 m_ScreenSize;
        #endregion

        #region Public Function
        /// <summary>
        ///     UI Functions. Called by button "Send" clicked or Keycode.Return clicked.
        /// </summary>
        public void SendText()
        {
            if (string.IsNullOrEmpty(m_InputField.text))
                return;
            InworldController.Instance.CurrentCharacter.SendText(m_InputField.text);
            m_InputField.text = null;
        }
        public void BackToLobby()
        {
            if (!m_RTCanvas)
                return;
            m_GlobalChatCanvas.gameObject.SetActive(false);
            m_CameraController.enabled = true;
            m_RTCanvas.gameObject.SetActive(true);
            m_RTCanvas.BackToLobby();
            Transform trPlayer = transform;
            trPlayer.position = m_InitPosition;
            trPlayer.eulerAngles = m_InitRotation;
        }
        #endregion

        #region Monobehavior Functions
        void Start()
        {
            InworldController.Instance.OnStateChanged += OnControllerStatusChanged;
        }
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.BackQuote))
            {
                m_GlobalChatCanvas.SetActive(!m_GlobalChatCanvas.activeSelf);
                if (m_CameraController)
                    m_CameraController.enabled = !m_GlobalChatCanvas.activeSelf;
                if (m_TriggerCanvas)
                    m_TriggerCanvas.SetActive(!m_TriggerCanvas.activeSelf);
            }
            if (!m_GlobalChatCanvas.activeSelf)
                return;
            if (!Input.GetKeyUp(KeyCode.Return) && !Input.GetKeyUp(KeyCode.KeypadEnter))
                return;
            SendText();
        }
        #endregion

        #region Callbacks
        void OnControllerStatusChanged(ControllerStates states)
        {
            if (states != ControllerStates.Connected)
                return;
            _ClearHistoryLog();
            foreach (InworldCharacter iwChar in InworldController.Instance.Characters)
            {
                m_Characters[iwChar.ID] = iwChar;
                iwChar.InteractionEvent.AddListener(OnInteractionStatus);
            }
        }
        void OnInteractionStatus(InteractionStatus status, List<HistoryItem> historyItems)
        {
            if (status != InteractionStatus.HistoryChanged)
                return;
            if (m_ContentRT)
                _RefreshBubbles(historyItems);
        }
        #endregion

        #region Private Functions
        void _RefreshBubbles(List<HistoryItem> historyItems)
        {
            foreach (HistoryItem item in historyItems)
            {
                if (!m_Bubbles.ContainsKey(item.UtteranceId))
                {
                    if (item.Event.Routing.Source.IsPlayer())
                    {
                        m_Bubbles[item.UtteranceId] = Instantiate(m_BubbleLeft, m_ContentRT);
                        m_Bubbles[item.UtteranceId].SetBubble(InworldAI.User.Name, InworldAI.Settings.DefaultThumbnail);
                    }
                    else if (item.Event.Routing.Source.IsAgent())
                    {
                        m_Bubbles[item.UtteranceId] = Instantiate(m_BubbleRight, m_ContentRT);
                        if (m_Characters.ContainsKey(item.Event.Routing.Source.Id))
                        {
                            InworldCharacter source = m_Characters[item.Event.Routing.Source.Id];
                            m_Bubbles[item.UtteranceId].SetBubble(source.CharacterName, source.Data.Thumbnail);
                        }
                    }
                }
                m_Bubbles[item.UtteranceId].Text = item.Event.Text;
                _SetContentHeight();
            }
        }
        void _ClearHistoryLog()
        {
            foreach (KeyValuePair<string, ChatBubble> kvp in m_Bubbles)
            {
                Destroy(kvp.Value.gameObject, 0.25f);
            }
            m_Bubbles.Clear();
            m_Characters.Clear();
        }
        void _SetContentHeight()
        {
            float fHeight = m_Bubbles.Values.Sum(bubble => bubble.Height);
            m_ContentRT.sizeDelta = new Vector2(m_ContentRT.sizeDelta.x, fHeight);
        }
        #endregion
    }
}

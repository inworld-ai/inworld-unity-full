/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Util;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Inworld.Sample.UI
{
    /// <summary>
    ///     This class is used to show/hide the Inworld Characters' floating text bubble.
    /// </summary>
    public class ChatPanel3D : MonoBehaviour
    {
        readonly Dictionary<string, ChatBubble> m_Bubbles = new Dictionary<string, ChatBubble>(12);
        void Update()
        {
            m_IconColor.a *= 0.99f;
            if (m_EmoIcon)
                m_EmoIcon.color = m_IconColor;
        }
        void OnEnable()
        {
            if (m_EmoIcon)
                m_IconColor = m_EmoIcon.color;
            m_IconColor.a = 1;
            _ClearHistoryLog();
            m_Owner.InteractionEvent.AddListener(OnInteractionStatus);
        }
        internal void ProcessEmotion(FacialAnimation face)
        {
            m_IconColor.a = 1;
            if (m_EmoIcon)
                m_EmoIcon.sprite = face.icon;
        }
        void OnInteractionStatus(InteractionStatus status, List<HistoryItem> historyItems)
        {
            if (status != InteractionStatus.HistoryChanged)
                return;
            _RefreshBubbles(historyItems);
        }
        void _RefreshBubbles(List<HistoryItem> historyItems)
        {
            foreach (HistoryItem item in historyItems)
            {
                if (!m_Bubbles.ContainsKey(item.UtteranceId))
                {
                    if (item.Event.Routing.Source.IsPlayer() && item.Event.Routing.Target.Id == m_Owner.ID)
                    {
                        m_Bubbles[item.UtteranceId] = Instantiate(m_LeftBubble, m_PanelAnchor);
                        m_Bubbles[item.UtteranceId].CharacterName = InworldAI.User.Name;
                        if (m_Dots)
                            m_Dots.SetActive(true);
                    }
                    else if (item.Event.Routing.Source.IsAgent() && item.Event.Routing.Source.Id == m_Owner.ID)
                    {
                        m_Bubbles[item.UtteranceId] = Instantiate(m_RightBubble, m_PanelAnchor);
                        m_Bubbles[item.UtteranceId].CharacterName = m_Owner.CharacterName;
                        if (m_Dots)
                            m_Dots.SetActive(false);
                    }
                }
                m_Bubbles[item.UtteranceId].Text = item.Event.Text;
            }
        }
        void _ClearHistoryLog()
        {
            foreach (KeyValuePair<string, ChatBubble> kvp in m_Bubbles)
            {
                Destroy(kvp.Value.gameObject, 0.25f);
            }
        }

        #region Inspector Variables
        [SerializeField] RectTransform m_PanelAnchor;
        [SerializeField] InworldCharacter m_Owner;
        [Header("UI Expressions")]
        [SerializeField] ChatBubble m_LeftBubble;
        [SerializeField] ChatBubble m_RightBubble;
        [SerializeField] GameObject m_Dots;
        [SerializeField] Image m_EmoIcon;
        Color m_IconColor = Color.white;
        #endregion
    }
}

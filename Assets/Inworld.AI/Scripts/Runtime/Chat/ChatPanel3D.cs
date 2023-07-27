/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Packets;
using Inworld.Util;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Inworld.Sample.UI
{
    /// <summary>
    ///     This class is used to show/hide the Inworld Characters' floating text bubble.
    /// </summary>
    public class ChatPanel3D : MonoBehaviour
    {
        #region Inspector Variables
        [SerializeField] RectTransform m_PanelAnchor;
        [SerializeField] InworldCharacter m_Owner;
        [SerializeField] TMP_Text m_Relation;
        [Header("UI Expressions")]
        [SerializeField] ChatBubble m_LeftBubble;
        [SerializeField] ChatBubble m_RightBubble;
        [SerializeField] GameObject m_Dots;
        [SerializeField] Image m_EmoIcon;
        Color m_IconColor = Color.white;
        #endregion
        
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
            m_Owner.OnRelationUpdated.AddListener(OnRelationUpdate);
        }
        void OnRelationUpdate()
        {
            if (!m_Relation)
                return;
            string result = "";
            RelationState currentRelation = m_Owner.Relation;
            if (currentRelation.Attraction != 0)
                result += $"Attraction: {GetRelationIcon(currentRelation.Attraction)}\n";
            if (currentRelation.Familiar != 0)
                result += $"Familiar: {GetRelationIcon(currentRelation.Familiar)}\n";
            if (currentRelation.Flirtatious != 0)
                result += $"Flirtatious: {GetRelationIcon(currentRelation.Flirtatious)}\n";
            if (currentRelation.Respect != 0)
                result += $"Respect: {GetRelationIcon(currentRelation.Respect)}\n";
            if (currentRelation.Trust != 0)
                result += $"Trust: {GetRelationIcon(currentRelation.Trust)}\n";
            m_Relation.text = result;
        }
        string GetRelationIcon(int nRelationValue)
        {
            string result = "";
            if (nRelationValue > 0)
            {
                int nIcon = nRelationValue / 10;
                for (int i = 0; i < nIcon; i++)
                {
                    result += "<color=green>+</color>";
                }
            }
            else if (nRelationValue < 0)
            {
                int nIcon = Mathf.Abs(nRelationValue / 10);
                for (int i = 0; i < nIcon; i++)
                {
                    result += "<color=red>-</color>";
                }
            }
            return result;
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
                if (item.Event is TextEvent textEvent)
                    m_Bubbles[item.UtteranceId].Text = textEvent.Text;
                if (item.Event is ActionEvent actionEvent)
                    m_Bubbles[item.UtteranceId].Text = $"<i>{actionEvent.Content}</i>";
            }
        }
        void _ClearHistoryLog()
        {
            foreach (KeyValuePair<string, ChatBubble> kvp in m_Bubbles)
            {
                Destroy(kvp.Value.gameObject, 0.25f);
            }
        }
    }
}

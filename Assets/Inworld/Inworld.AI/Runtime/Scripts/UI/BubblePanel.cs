/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Packet;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Inworld.UI
{
    public class BubblePanel : MonoBehaviour
    {
        [SerializeField] RectTransform m_ContentAnchor;

        protected readonly Dictionary<string, InworldUIElement> m_Bubbles = new Dictionary<string, InworldUIElement>();
        
        /// <summary>
        /// Return if the UI components are existing.
        /// </summary>
        public virtual bool IsUIReady => m_ContentAnchor && m_Bubbles != null;
        
        /// <summary>
        /// Update the layout if content updates.
        /// </summary>
        public void UpdateContent()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_ContentAnchor);
        }

        protected virtual void InsertBubble(string key, InworldUIElement bubble, string title, bool isAttachMode = false, string content = null, Texture2D thumbnail = null)
        {
            if (!m_Bubbles.ContainsKey(key))
            {
                m_Bubbles[key] = Instantiate(bubble, m_ContentAnchor);
                m_Bubbles[key].SetBubble(title, thumbnail, content);
            }
            else if (isAttachMode)
            {
                m_Bubbles[key].AttachBubble(content);
            }
            else
            {
                m_Bubbles[key].SetBubble(title, thumbnail, content);
            }
            UpdateContent();
        }
        protected virtual void InsertBubbleWithPacketInfo(PacketId packetId, InworldUIElement bubble, string title, bool isAttachMode = false, string content = null, Texture2D thumbnail = null)
        {
            string key = isAttachMode ? packetId.interactionId : packetId.utteranceId;
            if (!m_Bubbles.ContainsKey(key))
            {
                m_Bubbles[key] = Instantiate(bubble, m_ContentAnchor);
                m_Bubbles[key].SetBubbleWithPacketInfo(title, packetId, thumbnail, content);
            }
            else if (isAttachMode)
            {
                m_Bubbles[key].UpdateBubbleWithPacketInfo(packetId, content);
            }
            else
            {
                m_Bubbles[key].SetBubbleWithPacketInfo(title, packetId, thumbnail, content);
            }
            UpdateContent();
        }
        protected virtual void RemoveBubble(string key)
        {
            if (string.IsNullOrEmpty(key) || !m_Bubbles.ContainsKey(key))
                return;
            InworldUIElement elementToDestroy = m_Bubbles[key];
            m_Bubbles.Remove(key);
            Destroy(elementToDestroy.gameObject);
            UpdateContent();
        }
        protected virtual void Clear()
        {
            foreach (KeyValuePair<string, InworldUIElement> kvp in m_Bubbles)
            {
                Destroy(kvp.Value);
            }
            m_ContentAnchor.sizeDelta = new Vector2(m_ContentAnchor.sizeDelta.x, 0);
        }
    }
}

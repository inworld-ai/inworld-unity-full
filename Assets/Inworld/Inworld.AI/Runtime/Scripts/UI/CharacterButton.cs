/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using UnityEngine;
using Inworld.Entities;
using System.Collections;
using System.Linq;
using UnityEngine.Networking;

namespace Inworld.UI
{
    public class CharacterButton : InworldUIElement
    {
        [SerializeField] InworldCharacterData m_Data;
        [SerializeField] InworldCharacter m_Char;
        [SerializeField] Material m_MatSelected;
        [SerializeField] Material m_MatDeselected;

        protected bool m_IsSelected;
        /// <summary>
        /// Set the character's data.
        /// </summary>
        /// <param name="data">the data to set</param>
        public IEnumerator SetData(InworldCharacterData data)
        {
            m_Data = data;
            m_Title.text = data.givenName;
            if (data.thumbnail)
            {
                m_Icon.texture = data.thumbnail;
                yield break;
            }
            string url = data.characterAssets?.ThumbnailURL;
            if (string.IsNullOrEmpty(url))
                yield break;
            UnityWebRequest uwr = new UnityWebRequest(url);
            uwr.downloadHandler = new DownloadHandlerTexture();
            yield return uwr.SendWebRequest();
            if (!uwr.isDone || uwr.result != UnityWebRequest.Result.Success) 
                yield break;
            if (!(uwr.downloadHandler is DownloadHandlerTexture downloadTexture))
                yield break;
            Texture2D texture = downloadTexture.texture;
            m_Icon.texture = texture;
            m_Title.color = InworldAssetProcessor.GetFontColor(texture);
        }
        /// <summary>
        /// Select this character to interact with.
        /// </summary>
        public void SelectCharacter(bool isSelected)
        {
            //TODO(Yan): Support Async Connections.
            if (InworldController.Status != InworldConnectionStatus.Connected)
                return;
            m_IsSelected = isSelected;
            m_Icon.material = m_IsSelected ? m_MatSelected : m_MatDeselected;
            InworldCharacter iwChar = GetCharacter();
            if (!iwChar)
            {
                iwChar = Instantiate(m_Char, InworldController.Instance.transform);
                iwChar.transform.name = m_Data.givenName;
            }
            iwChar.Data = m_Data;
            if (isSelected)
            {
                InworldController.CharacterHandler.Register(iwChar);
                if (InworldController.CharacterHandler.CurrentCharacters.Count == 1)
                    InworldController.CharacterHandler.CurrentCharacter = iwChar;
            }
            else
                InworldController.CharacterHandler.Unregister(iwChar);
        }
        /// <summary>
        /// Get this character.
        /// </summary>
        InworldCharacter GetCharacter()
        {
            foreach (Transform child in InworldController.Instance.transform)
            {
                InworldCharacter iwChar = child.GetComponent<InworldCharacter>();
                if (iwChar && iwChar.Data.brainName == m_Data.brainName)
                    return iwChar;
            }
            return null;
        }
    }
}

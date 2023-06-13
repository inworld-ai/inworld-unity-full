/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Util;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
namespace Inworld.Runtime
{
    public class CharacterChooserPanel : MonoBehaviour
    {
        [SerializeField] TMP_Text m_Title;
        [SerializeField] CharacterChooser m_CharChooser;
        [SerializeField] RectTransform m_Anchor;
        [SerializeField] int m_CacheItems = 12;
        [SerializeField] float m_BoardHeight = 800;
        
        const string k_AllCharacters = "All Characters";
        int m_WorkspaceIndex = 0;
        List<string> m_WorkspaceList = new List<string>();
        readonly SortedList<string, CharacterChooser> m_CharList = new SortedList<string, CharacterChooser>();
        int m_Characters;
        public string Title
        {
            get => m_Title.text;
            set => m_Title.text = value;
        }

        public void Clear()
        {
            m_Characters = 0;
            m_CharList.Clear();
            foreach (Transform trCharacterChooser in m_Anchor)
            {
                CharacterChooser characterChooser = trCharacterChooser.GetComponent<CharacterChooser>();
                if (characterChooser)
                    characterChooser.ClearList();
                trCharacterChooser.gameObject.SetActive(false);
            }
        }
        void _SpawnInList(InworldCharacterData charData)
        {
            if (m_CharList.ContainsKey(charData.brain))
                return;
            int nSibIndex = Mathf.Max(m_Characters - 1, 0);
            CharacterChooser characterChooser = m_Anchor.transform.GetChild(nSibIndex).GetComponent<CharacterChooser>();
            if (characterChooser == null)
                return;
            characterChooser.gameObject.SetActive(true);
            characterChooser.transform.name = charData.characterName;
            characterChooser.CharacterData = charData;
            m_CharList[charData.brain] = characterChooser;
        }
        public void EstablishWSList()
        {
            m_WorkspaceList.Clear();
            m_WorkspaceList.Add(k_AllCharacters);
            Title = m_WorkspaceList[m_WorkspaceIndex];
            m_WorkspaceList.AddRange(InworldAI.User.Workspaces.Values.Select(wsData => wsData.title));
            ArrangeIndex();
        }
        public void Prev()
        {
            m_WorkspaceIndex = (m_WorkspaceIndex - 1 + m_WorkspaceList.Count) % m_WorkspaceList.Count;
            _ListCharacters();
        }
        public void Next()
        {
            m_WorkspaceIndex = (m_WorkspaceIndex + 1) % m_WorkspaceList.Count;
            _ListCharacters();
        }
        void _SetupScroolHeight(int nCount)
        {
            Vector2 sizeDelta = m_Anchor.sizeDelta;
            sizeDelta = new Vector2(sizeDelta.x, (nCount / 12 + 1) * m_BoardHeight);
            m_Anchor.sizeDelta = sizeDelta;
            m_Anchor.anchoredPosition = Vector2.zero;
        }
        void _ListCharacters()
        {
            Clear();
            Title = m_WorkspaceList[m_WorkspaceIndex];
            if (Title == k_AllCharacters)
            {
                // List All Characters.
                _SetupScroolHeight(InworldAI.User.Characters.Count);
                foreach (InworldCharacterData charData in InworldAI.User.Characters.Values)
                {
                    Spawn(charData);
                }
            }
            else
            {
                // List All Characters.
                InworldWorkspaceData wsData = InworldAI.User.Workspaces.Values.FirstOrDefault
                    (ws => ws.title == Title);
                if (wsData == null)
                    return;
                _SetupScroolHeight(wsData.characters.Count);
                foreach (InworldCharacterData charData in wsData.characters)
                {
                    Spawn(charData);
                }
            }
            ArrangeIndex();
        }
        public void ArrangeIndex()
        {
            int i = 0;
            foreach (KeyValuePair<string, CharacterChooser> currBtn in m_CharList)
            {
                currBtn.Value.transform.SetSiblingIndex(i);
                i++;
            }
        }
        public void Spawn(InworldCharacterData charData)
        {
            m_Characters++;
            if (m_Characters >= m_Anchor.childCount)
            {
                for (int i = 0; i < m_CacheItems; i++)
                {
                    CharacterChooser newChooser = Instantiate(m_CharChooser, m_Anchor);
                    newChooser.gameObject.SetActive(false);
                }
                Vector2 sizeDelta = m_Anchor.sizeDelta;
                sizeDelta = new Vector2(sizeDelta.x, sizeDelta.y + m_BoardHeight);
                m_Anchor.sizeDelta = sizeDelta;
            }
            _SpawnInList(charData);
        }
    }
}

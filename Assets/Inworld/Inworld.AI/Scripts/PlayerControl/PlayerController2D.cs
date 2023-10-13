using System.Collections.Generic;
using Inworld.UI;
using UnityEngine;


namespace Inworld
{
    public class PlayerController2D : PlayerController
    {
        [SerializeField] protected RectTransform m_CharContentAnchor;
        [SerializeField] protected CharacterButton m_CharSelectorPrefab;
       
        protected readonly Dictionary<string, CharacterButton> m_Characters = new Dictionary<string, CharacterButton>();
        

        protected override void Start()
        {
            if (m_PushToTalk)
            {
                InworldController.CharacterHandler.ManualAudioHandling = true;
                InworldController.Audio.AutoPush = false;
            }
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            InworldController.CharacterHandler.OnCharacterRegistered += OnCharacterRegistered;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (!InworldController.Instance)
                return;
            InworldController.CharacterHandler.OnCharacterRegistered -= OnCharacterRegistered;
        }
        
        protected virtual void OnCharacterRegistered(InworldCharacterData charData)
        {
            if (!m_Characters.ContainsKey(charData.brainName))
                m_Characters[charData.brainName] = Instantiate(m_CharSelectorPrefab, m_CharContentAnchor);
            m_Characters[charData.brainName].SetData(charData);
            SetContentHeight(m_CharContentAnchor, m_CharSelectorPrefab);
        }
        

    }
}


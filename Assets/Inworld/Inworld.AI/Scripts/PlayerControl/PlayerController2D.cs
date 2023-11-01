/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System.Collections.Generic;
using Inworld.UI;
using Inworld.Entities;
using UnityEngine;


namespace Inworld.Sample
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
            StartCoroutine(m_Characters[charData.brainName].SetData(charData));
            SetContentHeight(m_CharContentAnchor, m_CharSelectorPrefab);
        }
    }
}


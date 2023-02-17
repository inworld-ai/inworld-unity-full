/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Model;
using Inworld.Packets;
using Inworld.Util;
using System.Collections;
using UnityEngine;
namespace Inworld.Sample
{
    public class TransformCanvas : DemoCanvas
    {
        [SerializeField] GameObject m_Stone;
        [SerializeField] GameObject m_Avatar;
        [SerializeField] InworldCharacterData m_CharData;
        [SerializeField] InworldLipAnimation m_LipAnimation;
        InworldCharacter m_CurrentCharacter;

        // Start is called before the first frame update
        void Start()
        {
            InworldController.Instance.OnStateChanged += OnStatusChanged;
            InworldController.Instance.OnCharacterChanged += OnCharacterChanged;
            StartCoroutine(ShowRealAnswer());
        }
        
        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnStateChanged -= OnStatusChanged;
            InworldController.Instance.OnCharacterChanged -= OnCharacterChanged;
        }

        protected override void OnCharacterChanged(InworldCharacter oldCharacter, InworldCharacter newCharacter)
        {
            if (!newCharacter && oldCharacter)
                m_Title.text = $"{oldCharacter.transform.name} Disconnected!";
            else
            {
                m_Title.text = $"{newCharacter.transform.name} connected!";
                m_CurrentCharacter = newCharacter;
                if (m_CurrentCharacter.Data.characterName == m_CharData.characterName)
                {
                    InworldAI.Log("Sending trigger " + m_CurrentCharacter.Data.triggers[0] + " for the character to start the conversation first");
                    m_CurrentCharacter.SendTrigger(m_CurrentCharacter.Data.triggers[0]);
                }
            }
        }
        
        IEnumerator ShowRealAnswer()
        {
            yield return new WaitForSeconds(60f);
            m_Content.text = "The answer for the spell is <color=green>WWW</color>\nTry say that!";
        }
    }
}

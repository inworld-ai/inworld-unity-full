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
            InworldController.Instance.OnPacketReceived += OnPacketEvents;
            StartCoroutine(ShowRealAnswer());
        }
        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnStateChanged -= OnStatusChanged;
            InworldController.Instance.OnCharacterChanged -= OnCharacterChanged;
            InworldController.Instance.OnPacketReceived -= OnPacketEvents;
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
                    m_CurrentCharacter.SendTrigger(m_CurrentCharacter.Data.triggers[0]);
                }
            }
        }
        void OnPacketEvents(InworldPacket packet)
        {
            if (!InworldController.Instance.CurrentCharacter)
                return;
            string charID = InworldController.Instance.CurrentCharacter.ID;
            if (packet.Routing.Target.Id != charID)
                return;
            if (packet is TextEvent textEvent)
                _HandleTextEvent(textEvent);
        }
        void _HandleTextEvent(TextEvent textEvent)
        {
            int nWCount = 0, nStartIndex = -1, nEndIndex = -1;
            for (int i = 0; i < textEvent.Text.Length; i++)
            {
                if (textEvent.Text[i] != 'W' && textEvent.Text[i] != 'w')
                    continue;
                nWCount++;
                if (nStartIndex == -1)
                    nStartIndex = i;
                else
                    nEndIndex = i;
            }
            // YAN: Have some margin as the answer "WWW" is not recognized well.
            if (nWCount >= 3 && nEndIndex - nStartIndex < 5 && nEndIndex - nStartIndex > 0)
            {
                m_CurrentCharacter.SendTrigger(m_CurrentCharacter.Data.triggers[1]);
                m_Stone.SetActive(false);
                m_Avatar.SetActive(true);
                m_LipAnimation.Init();
            }
        }
        IEnumerator ShowRealAnswer()
        {
            yield return new WaitForSeconds(60f);
            m_Content.text = "The answer for the spell is <color=green>WWW</color>\nTry say that!";
        }
    }
}

/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Inworld.Sample.RPM
{
    public class TransformCanvas : DemoCanvas
    {
        public string trigger;
        [SerializeField] GameObject m_Stone;
        [SerializeField] GameObject m_Avatar;
        [SerializeField] InworldCharacterData m_CharData;
        [SerializeField] InworldFacialAnimationRPM m_LipAnimation;
        [SerializeField] string m_CheckTrigger;
        InworldCharacter m_CurrentCharacter;

        // Start is called before the first frame update
        void Start()
        {
            InworldController.Client.OnStatusChanged += OnStatusChanged;
            InworldController.Instance.OnCharacterChanged += OnCharacterChanged;
            StartCoroutine(ShowRealAnswer());
        }
        
        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Client.OnStatusChanged -= OnStatusChanged;
            InworldController.Instance.OnCharacterChanged -= OnCharacterChanged;
        }
        protected override void OnStatusChanged(InworldConnectionStatus newStatus)
        {
            if (newStatus != InworldConnectionStatus.Connected)
                return;
            if (m_CurrentCharacter.Data.givenName == m_CharData.givenName)
            {
                InworldController.Client.SendTrigger(m_CurrentCharacter.Data.agentId, "initconvo", new Dictionary<string, string>());
            }
        }
        protected override void OnCharacterChanged(InworldCharacter oldCharacter, InworldCharacter newCharacter)
        {
            if (!newCharacter && oldCharacter)
                m_Title.text = $"{oldCharacter.transform.name} Disconnected!";
            else
            {
                m_Title.text = $"{newCharacter.transform.name} connected!";
                m_CurrentCharacter = newCharacter;
            }
        }
        
        IEnumerator ShowRealAnswer()
        {
            yield return new WaitForSeconds(60f);
            m_Content.text = "The answer for the spell is <color=green>WWW</color>\nTry say that!";
        }

        public void OnGoalComplete(string trigger)
        {
            if (trigger != m_CheckTrigger)
                return;
            if (!m_CurrentCharacter)
                return;
            m_Stone.SetActive(false);
            m_Avatar.SetActive(true);
            m_LipAnimation.Init();
        }
    }
}

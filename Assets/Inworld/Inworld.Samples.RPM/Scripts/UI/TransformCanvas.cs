/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System.Collections;
using UnityEngine;

namespace Inworld.Sample.RPM
{
    public class TransformCanvas : DemoCanvas
    {
        [SerializeField] GameObject m_Stone;
        [SerializeField] GameObject m_Avatar;
        [SerializeField] InworldFacialAnimationRPM m_LipAnimation;
        [SerializeField] string m_InitTrigger;
        [SerializeField] string m_CheckTrigger;
        [SerializeField] InworldCharacter m_CurrentCharacter;

        bool m_InitTriggerSent;

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            StartCoroutine(ShowRealAnswer());
        }

        protected override void OnStatusChanged(InworldConnectionStatus incomingStatus)
        {
            if (incomingStatus == InworldConnectionStatus.Connected && m_CurrentCharacter && !m_InitTriggerSent)
            {
                m_CurrentCharacter.SendTrigger(m_InitTrigger);
                m_InitTriggerSent = true;
            }
        }

        protected override void OnCharacterChanged(InworldCharacter oldCharacter, InworldCharacter newCharacter)
        {
            if (!newCharacter && oldCharacter)
                m_Title.text = $"{oldCharacter.transform.name} Disconnected!";
        }
        
        IEnumerator ShowRealAnswer()
        {
            yield return new WaitForSeconds(60f);
            m_Content.text = "The answer for the spell is <color=green>WWW</color>\nTry say that!";
        }

        /// <summary>
        /// Callback function registered in the UnityEvent of InworldCharacter.
        /// </summary>
        /// <param name="trigger">the callback trigger to process.</param>
        public void OnGoalComplete(string trigger)
        {
            if (trigger != m_CheckTrigger)
                return;
            if (!m_CurrentCharacter)
                return;
            m_Stone.SetActive(false);
            m_Avatar.SetActive(true);
            m_LipAnimation.InitLipSync();
        }
    }
}

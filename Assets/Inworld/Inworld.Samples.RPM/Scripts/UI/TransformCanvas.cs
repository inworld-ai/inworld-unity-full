/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
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

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
            StartCoroutine(ShowRealAnswer());
        }
        
        IEnumerator ShowRealAnswer()
        {
            yield return new WaitForSeconds(60f);
            m_Content.text = "The answer for the spell is <color=green>WWW</color>\nTry say that!";
        }
        /// <summary>
        /// Callback function when the character is selected.
        /// </summary>
        /// <param name="brainName">the character's brain Name who received the goal.</param>
        protected override void OnCharacterSelected(string brainName)
        {
            if (m_CurrentCharacter.BrainName == brainName)
                m_CurrentCharacter.SendTrigger(m_InitTrigger);
        }
        /// <summary>
        /// Callback function registered in the UnityEvent of InworldCharacter.
        /// </summary>
        /// <param name="brainName">the character's brain Name who received the goal.</param>
        /// <param name="trigger">the callback trigger to process.</param>
        public void OnGoalComplete(string brainName, string trigger)
        {
            if (trigger != m_CheckTrigger)
                return;
            if (m_CurrentCharacter.BrainName != brainName)
                return;
            m_Stone.SetActive(false);
            m_Avatar.SetActive(true);
            m_LipAnimation.InitLipSync();
        }
    }
}

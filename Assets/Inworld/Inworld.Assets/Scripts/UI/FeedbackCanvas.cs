/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Entities;
using Inworld.Sample;
using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Inworld.Assets
{
    public class FeedbackCanvas : PlayerCanvas
    {
        [SerializeField] TMP_InputField m_InputField;
        [SerializeField] GameObject m_Result;
        string m_InteractionID;
        string m_CorrelationID;
        InputAction m_SubmitInputAction;
        
        Feedback m_Feedback = new Feedback();

        protected override void Awake()
        {
            base.Awake();
            m_SubmitInputAction = InworldAI.InputActions["Submit"];
        }

        protected override void HandleInput()
        {
            base.HandleInput();
            if (!m_InputField)
                return;
            if (m_CanvasObj.activeSelf && m_SubmitInputAction != null && m_SubmitInputAction.WasReleasedThisFrame())
                Submit();
        }
        
        public void Init(string interactionID, string correlationID)
        {
            m_InteractionID = interactionID;
            m_CorrelationID = correlationID;
            gameObject.SetActive(true);
        }
        public void SetLike(bool isOn)
        {
            _CheckInit();
            m_Feedback.isLike = isOn;
        }
        public void SetIrrelevant(bool isOn) => _SetParameter(isOn, FeedbackType.INTERACTION_DISLIKE_TYPE_IRRELEVANT);

        public void SetUnsafe(bool isOn) => _SetParameter(isOn, FeedbackType.INTERACTION_DISLIKE_TYPE_UNSAFE);

        public void SetUntrue(bool isOn) => _SetParameter(isOn, FeedbackType.INTERACTION_DISLIKE_TYPE_UNTRUE);

        public void SetIncorrectKnowledge(bool isOn) => _SetParameter(isOn, FeedbackType.INTERACTION_DISLIKE_TYPE_INCORRECT_USE_KNOWLEDGE);

        public void SetUnexpectedAction(bool isOn) => _SetParameter(isOn, FeedbackType.INTERACTION_DISLIKE_TYPE_UNEXPECTED_ACTION);

        public void SetUnexpectedGoalBehavior(bool isOn) => _SetParameter(isOn, FeedbackType.INTERACTION_DISLIKE_TYPE_UNEXPECTED_GOAL_BEHAVIOR);

        public void SetRepetition(bool isOn) => _SetParameter(isOn, FeedbackType.INTERACTION_DISLIKE_TYPE_REPETITION);

        public void Submit()
        {
            _CheckInit();
            m_Feedback.comment = m_InputField.text;
            m_InputField.text = "";
            InworldController.Client.SendFeedbackAsync(m_InteractionID, m_CorrelationID, m_Feedback);
            if (!m_Feedback.isLike)
            {
                InworldController.CharacterHandler.CurrentCharacter.CancelResponse();
                // TODO(Yan): Replace bubble to contain more info.
                InworldController.Client.SendRegenerateEvent(InworldController.CharacterHandler.CurrentCharacter.ID,m_InteractionID); 
            }
                
            m_Result.SetActive(true);
        }

        void _CheckInit()
        {
            if (m_Feedback == null)
                m_Feedback = new Feedback();
            if (m_Feedback.type == null)
                m_Feedback.type = new List<string>();
        }
        void _SetParameter(bool isOn, FeedbackType type)
        {
            _CheckInit();
            if (isOn)
            {
                if (!m_Feedback.type.Contains(type.ToString()))
                    m_Feedback.type.Add(type.ToString());
            }
            else
            {
                if (m_Feedback.type.Contains(type.ToString()))
                    m_Feedback.type.Remove(type.ToString());
            }
        }
    }
}

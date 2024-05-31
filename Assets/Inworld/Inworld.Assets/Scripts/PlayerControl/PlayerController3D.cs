/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using UnityEngine;
using Inworld.UI;


namespace Inworld.Sample
{
    public class PlayerController3D : PlayerController
    {
        [SerializeField] protected GameObject m_ChatCanvas;
        [SerializeField] protected GameObject m_ErrorCanvas;
        [SerializeField] protected GameObject m_FeedbackCanvas;
        [SerializeField] protected GameObject m_OptionCanvas;
        [SerializeField] protected BubblePanel m_BubblePanel;
        
        /// <summary>
        /// Get if any canvas (except Status Canvas) is open.
        /// </summary>
        public override bool IsAnyCanvasOpen => m_ChatCanvas && m_ChatCanvas.activeSelf || 
                                                m_ErrorCanvas && m_ErrorCanvas.activeSelf ||
                                                m_FeedbackCanvas && m_FeedbackCanvas.activeSelf ||
                                                m_OptionCanvas && m_OptionCanvas.activeSelf;
        
        CharSelectingMethod m_PrevSelectingMethod;
        
        protected override void OnCharacterJoined(InworldCharacter newChar)
        {
            base.OnCharacterJoined(newChar);
            newChar.Event.onCharacterSelected.AddListener(OnCharSelected);
            newChar.Event.onCharacterDeselected.AddListener(OnCharDeselected);
        }
        
        protected override void OnCharacterLeft(InworldCharacter newChar)
        {
            base.OnCharacterLeft(newChar);
            newChar.Event.onCharacterSelected.RemoveListener(OnCharSelected);
            newChar.Event.onCharacterDeselected.RemoveListener(OnCharDeselected);
        }
        protected virtual void OnCharSelected(string newCharBrainName)
        {
            if (!m_Dropdown)
                return;
            string givenName = InworldController.CharacterHandler.GetCharacterByBrainName(newCharBrainName)?.Name;
            if (string.IsNullOrEmpty(givenName))
            {
                m_Dropdown.value = 0;
                m_Dropdown.RefreshShownValue();
            }
            else
            {
                int value = m_Dropdown.options.FindIndex(o => o.text == givenName);
                if (value == -1)
                    return;
                m_Dropdown.value = value;
                m_Dropdown.RefreshShownValue();
            }
            RefreshUIInteractive(true);
        }
        protected virtual void OnCharDeselected(string newCharBrainName)
        {
            if (!m_Dropdown)
                return;
            m_Dropdown.value = 0;
            m_Dropdown.RefreshShownValue();
            RefreshUIInteractive(false);
        }
        protected virtual void RefreshUIInteractive(bool isON)
        {
            if (m_InputField)
                m_InputField.interactable = isON;
            if (m_SendButton)
                m_SendButton.interactable = isON;
            if (m_RecordButton)
                m_RecordButton.interactable = isON;
        }
        protected override void HandleInput()
        {
            _HandleChatCanvas();
            _HandleOptionCanvas();
            InworldController.Audio.AutoDetectPlayerSpeaking = !IsAnyCanvasOpen;
        }
        protected void _HandleOptionCanvas()
        {
            if (!Input.GetKeyUp(optionKey))
                return;
            m_OptionCanvas.SetActive(!m_OptionCanvas.activeSelf);
        }
        protected void _HandleChatCanvas()
        {
            if (m_ChatCanvas.activeSelf)
                base.HandleInput();
            
            if (!Input.GetKeyUp(uiKey))
                return;
            
            m_ChatCanvas.SetActive(!m_ChatCanvas.activeSelf);
            if (m_ChatCanvas.activeSelf)
            {
                m_PrevSelectingMethod = InworldController.CharacterHandler.SelectingMethod;
                InworldController.CharacterHandler.SelectingMethod = CharSelectingMethod.Manual;
            }
            else
            {
                InworldController.CharacterHandler.SelectingMethod = m_PrevSelectingMethod;
            }
            if (m_BubblePanel)
                m_BubblePanel.UpdateContent();
        }
    }
}

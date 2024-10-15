/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using UnityEngine;
using Inworld.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

namespace Inworld.Sample.RPM
{
    public class MultiCharCanvas : DemoCanvas
    {
        [SerializeField] protected InputAction m_SwitchSelectionMethod;
        [SerializeField] protected InputAction m_AutoChat;
        const string k_SelectBySight = "Automatically select characters by sight and angle.\n";
        const string k_AutoChat = "The characters are chatting automatically.\n";
        const string k_SelectCharacter = "Please select characters\n";
        const string k_GroupChat = "Now <color=green>BroadCasting</color>";
        const string k_SelectByKey = "Press <color=green>\"1\"</color> and <color=green>\"2\"</color> to switch interact characters.\nPress <color=green>\"0\"</color> to broadcast.\n";
        string m_Instruction = "Press <color=green>\"Tab\"</color> to switch character selection method.\n";
        string m_CurrentMethod;
        string m_CharacterIndicator = k_GroupChat;
        bool m_AutoChatInitiated;
        protected virtual void Awake()
        {
            string switchSelection = m_SwitchSelectionMethod.bindings[0].path;
            switchSelection = switchSelection.Substring(switchSelection.LastIndexOf("/", StringComparison.Ordinal) + 1).ToUpper();
            string autoChat = m_AutoChat.bindings[0].path;
            autoChat = autoChat.Substring(autoChat.LastIndexOf("/", StringComparison.Ordinal) + 1).ToUpper();
            m_Instruction = $"Press <color=green>\"{switchSelection}\"</color> to switch character selection method.\n" +
                            $"Press <color=green>\"{autoChat}\"</color> to toggle auto chat.\n";
        }
        
        protected override void OnCharacterSelected(string newCharacter)
        {
            InworldCharacter character = InworldController.CharacterHandler[newCharacter];
            m_CharacterIndicator = character ? $"Now Talking to <color=green>{character.name}</color>" : k_GroupChat;
        }
        protected override void OnCharacterDeselected(string newCharacter)
        {
            m_CharacterIndicator = InworldController.Client.EnableGroupChat && InworldController.CharacterHandler.CurrentCharacters.Count > 0 ? k_GroupChat : k_SelectCharacter;
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            m_SwitchSelectionMethod.Enable();
            m_AutoChat.Enable();
            m_CurrentMethod = _GetCurrentInstruction();
            if (InworldController.CharacterHandler)
            {
                InworldController.CharacterHandler.Event.onCharacterListJoined.AddListener(OnCharacterListJoined);
                InworldController.CharacterHandler.Event.onConversationUpdated.AddListener(OnConversationUpdated);
            }
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            m_SwitchSelectionMethod.Disable();
            m_AutoChat.Disable();
            if (InworldController.CharacterHandler)
            {
                InworldController.CharacterHandler.Event.onCharacterListJoined.RemoveListener(OnCharacterListJoined);
                InworldController.CharacterHandler.Event.onConversationUpdated.RemoveListener(OnConversationUpdated);
            }
        }
        protected bool IsAutoChatInitiatingValid => !m_AutoChatInitiated &&
                                              InworldController.Client &&
                                              InworldController.Client.AutoChat &&
                                              InworldController.CharacterHandler &&
                                              InworldController.CharacterHandler.CurrentCharacters != null &&
                                              InworldController.CharacterHandler.CurrentCharacters.Count >= 2;
        
        
        protected void OnCharacterListJoined(InworldCharacter character)
        {
            if (!IsAutoChatInitiatingValid)
                return;
            // YAN: Use cached update conversation to start server immediately, if AutoChat is toggled. 
            InworldController.Client.UpdateConversation(InworldController.CharacterHandler.ConversationID, InworldController.CharacterHandler.CurrentCharacterNames, false);
        }
        protected void OnConversationUpdated()
        {
            // YAN: If Server started and conversation updated callback received. Immediately send 
            if (!IsAutoChatInitiatingValid)
                return;
            InworldController.CharacterHandler.NextTurn();
            m_AutoChatInitiated = true;
        }
        void Update()
        {
            if (m_Content)
                m_Content.text = $"{m_Instruction}{m_CurrentMethod}{m_CharacterIndicator}";
            if (!InworldController.Instance)
                return;
            if (m_AutoChat.WasReleasedThisFrame())
                InworldController.Client.AutoChat = !InworldController.Client.AutoChat;
            if (!m_SwitchSelectionMethod.WasReleasedThisFrame())
                return;
            InworldController.CharacterHandler.ChangeSelectingMethod();
            m_CurrentMethod = _GetCurrentInstruction();
        }
        string _GetCurrentInstruction()
        {
            switch (InworldController.CharacterHandler.SelectingMethod)
            {
                case CharSelectingMethod.KeyCode:
                    return k_SelectByKey;
                case CharSelectingMethod.SightAngle:
                    return k_SelectBySight;
            }
            return "";
        }
    }
}

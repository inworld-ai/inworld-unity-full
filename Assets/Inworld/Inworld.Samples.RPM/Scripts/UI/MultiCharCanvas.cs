/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Entities;
using UnityEngine;


namespace Inworld.Sample.RPM
{
    public class MultiCharCanvas : DemoCanvas
    {
        const string k_Instruction = "Press <color=green>\"Tab\"</color> to switch character selection method.\n";
        const string k_SelectByKey = "Press <color=green>\"1\"</color> and <color=green>\"2\"</color> to switch interact characters.\nPress <color=green>\"0\"</color> to broadcast.\n";
        const string k_SelectBySight = "Automatically select characters by sight and angle.\n";
        const string k_AutoChat = "The characters are chatting automatically.\n";
        string m_CurrentMethod;
        string m_CharacterIndicator = "Now <color=green>BroadCasting</color>";

        protected override void OnCharacterChanged(InworldCharacter oldCharacter, InworldCharacter newCharacter)
        {
            m_CharacterIndicator = newCharacter ? $"Now Talking to <color=green>{newCharacter.Name}</color>" : "Now <color=green>BroadCasting</color>";
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            m_CurrentMethod = _GetCurrentInstruction();
        }
        void Update()
        {
            m_Content.text = $"{k_Instruction}{m_CurrentMethod}{m_CharacterIndicator}";
            if (!Input.GetKeyUp(KeyCode.Tab))
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
                case CharSelectingMethod.AutoChat:
                    return k_AutoChat;
            }
            return "";
        }
        public void NextCharacterSpeaking(InworldCharacter character)
        {
            if (InworldController.CharacterHandler.SelectingMethod != CharSelectingMethod.AutoChat)
                return;
            if (string.IsNullOrEmpty(character.ID))
                return;
            InworldController.Instance.SendTrigger(InworldMessenger.NextTurn, character.ID);
        }
    }
}

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
        const string k_SelectByKey = "Press <color=green>\"1\"</color> and <color=green>\"2\"</color> to switch interact characters.\nPress <color=green>\"0\"</color> to broadcast";
        const string k_SelectBySight = "Automatically select characters by sight and angle.\n";
        const string k_AutoChat = "The characters are chatting automatically";
        string m_CurrentMethod;
        string m_CharacterName;

        protected override void OnCharacterChanged(InworldCharacter oldCharacter, InworldCharacter newCharacter)
        {
            m_CharacterName = newCharacter ? newCharacter.Name : "BroadCasting";
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            m_CurrentMethod = _GetCurrentInstruction();
        }
        void Update()
        {
            m_Content.text = $"{k_Instruction}{m_CurrentMethod}Now Talking to <color=green>{m_CharacterName}</color>";
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
                    InworldController.Instance.AutoChat(false);
                    return k_SelectByKey;
                case CharSelectingMethod.SightAngle:
                    InworldController.Instance.AutoChat(false);
                    return k_SelectBySight;
                case CharSelectingMethod.AutoChat:
                    InworldController.Instance.AutoChat(true);
                    return k_AutoChat;
            }
            return "";
        }
    }
}

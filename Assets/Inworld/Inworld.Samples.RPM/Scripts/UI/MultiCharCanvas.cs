/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using UnityEngine;


namespace Inworld.Sample.RPM
{
    public class MultiCharCanvas : DemoCanvas
    {
        const string k_Instruction = "Press <color=green>\"Tab\"</color> to switch character selection method.\n";
        const string k_SelectByKey = "Press <color=green>\"1\"</color> and <color=green>\"2\"</color> to switch interact characters.\n";
        const string k_SelectBySight = "Automatically select characters by sight and angle.\n";
        string m_CurrentMethod;
        string m_CharacterName;
        
        protected override void OnCharacterChanged(InworldCharacter oldCharacter, InworldCharacter newCharacter)
        {
            if (newCharacter)
                m_CharacterName = newCharacter.Name;
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
                    return k_SelectByKey;
                case CharSelectingMethod.SightAngle:
                    return k_SelectBySight;
            }
            return "";
        }
    }
}

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
        [SerializeField] InworldCharacter m_Character1;
        [SerializeField] InworldCharacter m_Character2;
        // Start is called before the first frame update
        const string k_ContentHeader = "Press <color=green>\"1\"</color> and <color=green>\"2\"</color> to switch interact characters.\n";

        protected override void OnStatusChanged(InworldConnectionStatus incomingStatus)
        {
            if (incomingStatus == InworldConnectionStatus.Connected)
            {
                m_Title.text = $"Inworld Connected!";
                m_CharacterHandler.CurrentCharacter = m_Character1;
            }
            else
            {
                m_Title.text = $"Inworld Disconnected!";
            }
            base.OnStatusChanged(incomingStatus);
        }
        protected override void OnCharacterChanged(InworldCharacter oldCharacter, InworldCharacter newCharacter)
        {
            if (newCharacter)
                m_Content.text = $"{k_ContentHeader}Now Talking to <color=green>{newCharacter.Name}</color>";
        }
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Alpha1))
                m_CharacterHandler.CurrentCharacter = m_Character1;
            if (Input.GetKeyUp(KeyCode.Alpha2))
                m_CharacterHandler.CurrentCharacter = m_Character2;
        }
    }
}

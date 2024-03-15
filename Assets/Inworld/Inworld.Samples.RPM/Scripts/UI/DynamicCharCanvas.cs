/*************************************************************************************************
* Copyright 2022-2024 Theai, Inc. dba Inworld AI
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using UnityEngine;

namespace Inworld.Sample.RPM
{
    public class DynamicCharCanvas : DemoCanvas
    {
        [SerializeField] Transform m_Player;
        [SerializeField] InworldCharacter m_Model;
        [SerializeField] float m_Distance = 5f;
        InworldCharacter m_CurrentCharacter;
        const string k_Instruction = "Press <color=green>F</color> to Instantiate a Character";

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Content.text = k_Instruction;
        }
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.F))
            {
                _CreateCharacter();
            }
        }
        
        void _CreateCharacter()
        {
            if (m_CurrentCharacter)
                DestroyImmediate(m_CurrentCharacter.gameObject);
            m_CurrentCharacter = Instantiate(m_Model, m_Player.position + m_Player.rotation * Vector3.forward * m_Distance, Quaternion.identity);
        }

        protected override void OnCharacterJoined(InworldCharacter character)
        {
            base.OnCharacterJoined(character);
            m_Content.text = $"Now talking to {character.Name}";
        }
        protected override void OnCharacterDeselected(string charName)
        {
            m_Content.text = k_Instruction;
        }
    }
}

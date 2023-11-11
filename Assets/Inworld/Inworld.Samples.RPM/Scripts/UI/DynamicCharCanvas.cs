/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
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
        [SerializeField] string m_RuntimeInstructions;
        InworldCharacter m_CurrentCharacter;

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
            m_CurrentCharacter.RegisterLiveSession();
            InworldController.CurrentCharacter = m_CurrentCharacter;
        }

        protected override void OnStatusChanged(InworldConnectionStatus incomingStatus)
        {
            base.OnStatusChanged(incomingStatus);
            m_Content.text = "Press <color=green>F</color> to Instantiate a Character";
        }
        protected override void OnCharacterChanged(InworldCharacter oldCharacter, InworldCharacter newCharacter)
        {
            m_Content.text = m_RuntimeInstructions;
        }
    }
}

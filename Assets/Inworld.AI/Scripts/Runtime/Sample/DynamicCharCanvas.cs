/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/

using Inworld.Util;
using UnityEngine;
namespace Inworld.Sample
{
    public class DynamicCharCanvas : DemoCanvas
    {
        [SerializeField] InworldCharacter m_Model;
        [SerializeField] float m_Distance = 5f;
        [SerializeField] string m_RuntimeInstructions;
        InworldCharacter m_CurrentCharacter;
        void Start()
        {
            InworldController.Instance.OnStateChanged += OnStatusChanged;
            InworldController.Instance.OnCharacterChanged += OnCharacterChanged;
        }
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.F))
            {
                _CreateCharacter();
            }
        }
        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnStateChanged -= OnStatusChanged;
            InworldController.Instance.OnCharacterChanged -= OnCharacterChanged;
        }
        void _CreateCharacter()
        {
            if (m_CurrentCharacter)
                Destroy(m_CurrentCharacter.gameObject);
            m_CurrentCharacter = Instantiate(m_Model, InworldController.Player.transform.position + Vector3.forward * m_Distance, Quaternion.identity);
            m_CurrentCharacter.RegisterLiveSession();
        }

        protected override void OnStatusChanged(ControllerStates incomingStatus)
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

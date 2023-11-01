/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using TMPro;
using UnityEngine;

namespace Inworld.Sample.RPM
{
    public class DemoCanvas : MonoBehaviour
    {
        [SerializeField] protected TMP_Text m_Title;
        [SerializeField] protected TMP_Text m_Content;
        // Start is called before the first frame update
        protected string m_ServerStatus;
        protected CharacterHandler m_CharacterHandler;

        protected virtual void Awake()
        {
            m_CharacterHandler = InworldController.CharacterHandler;
        }
        protected virtual void Start()
        {
            
        }
        protected virtual void OnEnable()
        { 
            InworldController.Client.OnStatusChanged += OnStatusChanged;
            m_CharacterHandler.OnCharacterChanged += OnCharacterChanged;
        }
        protected virtual void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Client.OnStatusChanged -= OnStatusChanged;
            m_CharacterHandler.OnCharacterChanged -= OnCharacterChanged;
        }
        protected virtual void OnStatusChanged(InworldConnectionStatus incomingStatus)
        {
            if (!m_Title)
                return;
            m_ServerStatus = incomingStatus.ToString();
            m_Title.text = $"Inworld {incomingStatus}";
        }
        protected virtual void OnCharacterChanged(InworldCharacter oldCharacter, InworldCharacter newCharacter)
        {
            if (!m_Title)
                return;
            if (!newCharacter && oldCharacter)
                m_Title.text = $"Inworld Disconnected!";
            else if (newCharacter && !oldCharacter)
                m_Title.text = $"Inworld Connected!";
        }
    }
}

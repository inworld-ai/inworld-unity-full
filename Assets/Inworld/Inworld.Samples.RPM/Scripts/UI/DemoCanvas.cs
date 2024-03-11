/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
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

        protected virtual void Start()
        {
            
        }
        protected virtual void OnEnable()
        {
            m_Title.text = $"Inworld {InworldController.Client.Status}";
            InworldController.Client.OnStatusChanged += OnStatusChanged;
            InworldController.CharacterHandler.OnCharacterListJoined += OnCharacterJoined;
            InworldController.CharacterHandler.OnCharacterListLeft += OnCharacterLeft;
        }
        protected virtual void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Client.OnStatusChanged -= OnStatusChanged;
            InworldController.CharacterHandler.OnCharacterListJoined -= OnCharacterJoined;
            InworldController.CharacterHandler.OnCharacterListLeft -= OnCharacterLeft;
        }
        protected virtual void OnStatusChanged(InworldConnectionStatus incomingStatus)
        {
            if (!m_Title)
                return;
            m_ServerStatus = incomingStatus.ToString();
            m_Title.text = $"Inworld {incomingStatus}";
        }
        protected virtual void OnCharacterJoined(InworldCharacter character)
        {
            m_Content.text = $"{character.Name} joined";
            character.Event.onCharacterSelected.AddListener(OnCharacterSelected);
            character.Event.onCharacterDeselected.AddListener(OnCharacterDeselected);
        }
        protected virtual void OnCharacterLeft(InworldCharacter character)
        {
            m_Content.text = $"{character.Name} left";
            character.Event.onCharacterSelected.RemoveListener(OnCharacterSelected);
            character.Event.onCharacterDeselected.RemoveListener(OnCharacterDeselected);
        }
        protected virtual void OnCharacterSelected(string charName)
        {
            
        }
        protected virtual void OnCharacterDeselected(string charName)
        {
            
        }
    }
}

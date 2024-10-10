/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Inworld.Sample
{
    public class CharacterHandler3D : CharacterHandler
    {
        [SerializeField] protected CharSelectingMethod m_SelectingMethod = CharSelectingMethod.SightAngle;
        [Tooltip("Only the priority lower that threshold would be selected.")][Range(0.1f, 2f)]
        [SerializeField] protected float m_SelectingThreshold = 0.5f;
        [Tooltip("How often do we calculate the priority:")][Range(0.1f, 1f)]
        [SerializeField] protected float m_RefreshRate = 0.5f;
        
        protected InputAction m_CharacterSelectInputAction;
        protected float m_CurrentTime;

        /// <summary>
        ///     Get the current Character Selecting Method.
        /// </summary>
        public override CharSelectingMethod SelectingMethod
        {
            get => m_SelectingMethod;
            set
            {
                if (m_SelectingMethod == value)
                    return;
                m_SelectingMethod = value;
                Event.onCharacterSelectingModeUpdated?.Invoke(m_SelectingMethod);
            }
        }
        /// <summary>
        ///     Change the method of how to select character.
        /// </summary>
        public override void ChangeSelectingMethod()
        {
            if (SelectingMethod == CharSelectingMethod.Manual || SelectingMethod == CharSelectingMethod.KeyCode)
                SelectingMethod = CharSelectingMethod.SightAngle;
            else if (SelectingMethod == CharSelectingMethod.SightAngle)
                SelectingMethod = CharSelectingMethod.KeyCode;
        }

        protected virtual void Awake()
        {
            m_CharacterSelectInputAction = InworldAI.InputActions["CharacterSelect"];
        }

        void Update()
        {
            switch (m_SelectingMethod)
            {
                case CharSelectingMethod.KeyCode:
                    SelectCharacterByKey();
                    break;
                case CharSelectingMethod.SightAngle:
                    SelectCharacterBySightAngle();
                    break;
            }
        }
        protected virtual void SelectCharacterBySightAngle()
        {
            m_CurrentTime += Time.deltaTime;
            if (m_CurrentTime < m_RefreshRate)
                return;
            m_CurrentTime = 0;
            float fPriority = m_SelectingThreshold;
            InworldCharacter targetCharacter = null;
            foreach (InworldCharacter character in m_CharacterList.Where(character => character && character.Priority >= 0 && character.Priority < fPriority))
            {
                fPriority = character.Priority;
                targetCharacter = character;
            }
            CurrentCharacter = targetCharacter;
        }
        protected virtual void SelectCharacterByKey()
        {
            if (!m_CharacterSelectInputAction.WasPressedThisFrame())
                return;
            
            int characterSelectValue = (int)m_CharacterSelectInputAction.ReadValue<float>();
            if (characterSelectValue == 0)
                CurrentCharacter = null;
            else if (characterSelectValue <= m_CharacterList.Count)
                CurrentCharacter = m_CharacterList[characterSelectValue - 1];
        }

    }
}

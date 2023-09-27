using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inworld
{
    public class CharacterHandler : SingletonBehavior<CharacterHandler>
    {
        [SerializeField] bool m_ManualAudioHandling;
        public bool ManualAudioHandling
        {
            get => m_ManualAudioHandling;
            set
            {
                if (m_ManualAudioHandling == value)
                    return;
                m_ManualAudioHandling = value;
                if (m_ManualAudioHandling)
                    _StopAudio();
                else 
                    _StartAudio();
            }
        }
        
        public event Action<InworldCharacter, InworldCharacter> OnCharacterChanged;
        
        public InworldCharacter CurrentCharacter
        {
            get => m_CurrentCharacter;
            set
            {
                string oldBrainName = m_CurrentCharacter ? m_CurrentCharacter.BrainName : "";
                string newBrainName = value ? value.BrainName : "";
                if (oldBrainName == newBrainName)
                    return;
                _StopAudio();
                m_LastCharacter = m_CurrentCharacter;
                m_CurrentCharacter = value;
                if(!ManualAudioHandling)
                    _StartAudio();
                OnCharacterChanged?.Invoke(m_LastCharacter, m_CurrentCharacter);
            }
        }

        [SerializeField] InworldCharacter m_DefaultCharacter;
        
        InworldCharacter m_CurrentCharacter;
        InworldCharacter m_LastCharacter;
        bool m_IsFirstConnection = true;
        
        public void SendText(string text)
        {
            if (!m_CurrentCharacter)
                return;
            // 1. Interrupt current speaking.
            m_CurrentCharacter.CancelResponse();
            // 2. Send Text.
            InworldController.Instance.SendText(m_CurrentCharacter.ID, text);
        }

        public void StartAudio()
        {
            if (!ManualAudioHandling)
                return;
            _StartAudio();
        }

        void _StartAudio()
        {
            if (!m_CurrentCharacter)
                return;
            try
            {
                InworldController.Instance.StartAudio(m_CurrentCharacter.ID);
            }
            catch (InworldException e)
            {
                
            }
        }

        public void StopAudio()
        {
            if (!ManualAudioHandling)
                return;
            _StopAudio();
        }
        
        public void _StopAudio()
        {
            if (!m_CurrentCharacter)
                return;
            try
            {
                InworldController.Instance.StopAudio(m_CurrentCharacter.ID);
            }
            catch (InworldException e)
            {
                
            }
        }
        
        public void PushAudio()
        {
            if (!m_CurrentCharacter)
                return;
            try
            {
                InworldController.Instance.PushAudio();
            }
            catch (InworldException e)
            {
                
            }
        }

        protected virtual void OnStatusChanged(InworldConnectionStatus newStatus)
        {
            if (newStatus == InworldConnectionStatus.Connected)
            {
                if (m_IsFirstConnection)
                {
                    if(m_DefaultCharacter)
                        CurrentCharacter = m_DefaultCharacter;
                    m_IsFirstConnection = false;
                }
                if(!ManualAudioHandling)
                    _StartAudio();
            }
            else 
                _StopAudio();
        }

        protected virtual void OnEnable()
        {
            InworldController.Client.OnStatusChanged += OnStatusChanged;
        }

        protected virtual void OnDisable()
        {
            InworldController.Client.OnStatusChanged -= OnStatusChanged;
        }
    }
}


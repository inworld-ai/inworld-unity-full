/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Inworld.Entities;

namespace Inworld
{
    public enum CharSelectingMethod
    {
        Manual,
        KeyCode,
        SightAngle
    }
    public class CharacterHandler : MonoBehaviour
    {
        [SerializeField] bool m_ManualAudioHandling;
        InworldCharacter m_CurrentCharacter;
        InworldCharacter m_LastCharacter;
        public event Action<InworldCharacterData> OnCharacterRegistered;
        public event Action<InworldCharacter, InworldCharacter> OnCharacterChanged;

        // YAN: Now LiveSessionID is handled by CharacterHandler Only. It'll always be updated. 
        //      Both Keys are BrainNames
        protected readonly Dictionary<string, string> m_LiveSession = new Dictionary<string, string>();
        // YAN: Although InworldCharacterData also has agentID, it won't be always updated. Please check m_LiveSession
        //      And Call RegisterLiveSession if outdated.
        protected readonly Dictionary<string, InworldCharacterData> m_Characters = new Dictionary<string, InworldCharacterData>();

        /// <summary>
        ///     Return if any character is speaking.
        /// </summary>
        public virtual bool IsAnyCharacterSpeaking => CurrentCharacter.IsSpeaking;

        /// <summary>
        /// Gets/Sets the current interacting character.
        /// If set, it'll also start audio sampling if `ManualAudioHandling` is false, and invoke the event OnCharacterChanged
        /// </summary>
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
        /// <summary>
        /// If it's false, AudioCapture of the InworldController will automatically start recording player's voice when at least a character is selected.
        /// Otherwise, developers need to manually call `InworldController.Instance.StartAudio()` to start microphone.
        /// </summary>
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
        /// <summary>
        ///     Get the current Character Selecting Method. By default it's manual.
        /// </summary>
        public virtual CharSelectingMethod SelectingMethod => CharSelectingMethod.Manual;

        /// <summary>
        ///     Change the method of how to select character.
        /// </summary>
        public virtual void ChangeSelectingMethod() {}
        /// <summary>
        /// Check if a character is registered.
        /// </summary>
        /// <param name="characterID">The live session ID of the Inworld character.</param>
        public bool IsRegistered(string characterID) => !string.IsNullOrEmpty(characterID) && m_LiveSession.ContainsValue(characterID);
        /// <summary>
        /// Get the live session ID for an Inworld character.
        /// </summary>
        /// <param name="character">The request Inworld character.</param>
        public virtual string GetLiveSessionID(InworldCharacter character)
        {
            if (!character || string.IsNullOrEmpty(character.BrainName))
                return null;
            // ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd
            if (!m_Characters.ContainsKey(character.BrainName))
                m_Characters[character.BrainName] = character.Data;
            return m_LiveSession[character.BrainName];
        }
        /// <summary>
        /// Get the InworldCharacterData by character's live session ID.
        /// </summary>
        /// <param name="agentID">the request character's live session ID.</param>
        public InworldCharacterData GetCharacterDataByID(string agentID)
        {
            if (!m_LiveSession.ContainsValue(agentID))
            {
                InworldAI.LogError($"{agentID} Not Registered!");
                return null;
            }
            string key = m_LiveSession.First(kvp => kvp.Value == agentID).Key;
            if (m_Characters.TryGetValue(key, out InworldCharacterData character))
                return character;
            InworldAI.LogError($"{key} Not Registered!");
            return null;
        }
        protected virtual void OnEnable()
        {
            InworldController.Client.OnStatusChanged += OnStatusChanged;
        }

        protected virtual void OnDisable()
        {
            InworldController.Client.OnStatusChanged -= OnStatusChanged;
        }

        protected void _StartAudio()
        {
            if (!m_CurrentCharacter || InworldController.Client.Status != InworldConnectionStatus.Connected)
                return;
            try
            {
                InworldController.Instance.StartAudio(m_CurrentCharacter.ID);
            }
            catch (InworldException e)
            {
                InworldAI.LogWarning($"Audio failed to start: {e}");
            }
        }
        
        protected void _StopAudio()
        {
            if (!m_CurrentCharacter)
                return;
            try
            {
                InworldController.Instance.StopAudio(m_CurrentCharacter.ID);
            }
            catch (InworldException e)
            {
                InworldAI.LogWarning($"Audio failed to stop: {e}");
            }
        }
        IEnumerator UpdateThumbnail(InworldCharacterData agent)
        {
            if (agent.thumbnail)
                yield break;
            string url = agent.characterAssets?.ThumbnailURL;
            if (string.IsNullOrEmpty(url))
                yield break;
            UnityWebRequest uwr = new UnityWebRequest(url);
            uwr.downloadHandler = new DownloadHandlerTexture();
            yield return uwr.SendWebRequest();
            if (uwr.isDone && uwr.result == UnityWebRequest.Result.Success)
            {
                agent.thumbnail = (uwr.downloadHandler as DownloadHandlerTexture)?.texture;
            }
        }
        protected virtual void OnStatusChanged(InworldConnectionStatus newStatus)
        {
            if (newStatus == InworldConnectionStatus.LoadingSceneCompleted)
            {
                _RegisterLiveSession();
            }
            if (newStatus == InworldConnectionStatus.Connected)
            {
                if (!ManualAudioHandling)
                    _StartAudio();
            }
            else
            {
                _StopAudio();
            }
        }
        protected void _RegisterLiveSession()
        {
            LoadSceneResponse response = InworldController.Client.GetLiveSessionInfo();
            if (response == null)
                return;
            m_LiveSession.Clear();
            foreach (InworldCharacterData agent in response.agents.Where(agent => !string.IsNullOrEmpty(agent.agentId) && !string.IsNullOrEmpty(agent.brainName)))
            {
                m_LiveSession[agent.brainName] = agent.agentId;
                m_Characters[agent.brainName] = agent;
                StartCoroutine(UpdateThumbnail(agent));
                OnCharacterRegistered?.Invoke(agent);
            }
        }
    }
}


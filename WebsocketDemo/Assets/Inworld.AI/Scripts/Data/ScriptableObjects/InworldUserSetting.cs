using System;
using System.Collections.Generic;
using UnityEngine;
namespace Inworld
{
    [Serializable]
    public class InworldPlayerProfile
    {
        public string property;
        public string value;
    }
    public class InworldUserSetting : ScriptableObject
    {
        [SerializeField] string m_PlayerName;
        [SerializeField] List<Fields> m_PlayerData;

        public string Name
        {
            get => string.IsNullOrEmpty(m_PlayerName) ? "player" : m_PlayerName;
            set => m_PlayerName = value;
        }
        public User Request => new User
        {
            name = Name
        };
        public UserSetting Setting => new UserSetting
        {
            viewTranscriptConsent = true,
            playerProfile = new PlayerProfile
            {
                fields = m_PlayerData
            }
        };
    }
}

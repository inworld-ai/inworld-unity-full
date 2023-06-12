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
        [SerializeField] List<InworldPlayerProfile> m_PlayerData;

        public string Name
        {
            get => string.IsNullOrEmpty(m_PlayerName) ? "player" : m_PlayerName;
            set => m_PlayerName = value;
        }
    }
}

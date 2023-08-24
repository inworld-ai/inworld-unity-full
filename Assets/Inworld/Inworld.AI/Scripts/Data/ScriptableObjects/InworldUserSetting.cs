using System;
using System.Collections.Generic;
using System.Linq;
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
        
        [SerializeField] List<InworldPlayerProfile> m_PlayerData = new List<InworldPlayerProfile>();
        [SerializeField] List<InworldWorkspaceData> m_Workspaces = new List<InworldWorkspaceData>();
        [HideInInspector][SerializeField] string m_BillingAccount;
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
                fields = m_PlayerData.Select(data => new PlayerProfileField
                {
                    fieldId = data.property,
                    fieldValue = data.value
                })
            }
        };
        public string BillingAccount
        {
            get => m_BillingAccount;
            set => m_BillingAccount = value;
        }
        public List<InworldWorkspaceData> Workspace => m_Workspaces;
    }
}

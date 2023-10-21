/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
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
        [HideInInspector][SerializeField] string m_UserID;
        [HideInInspector][SerializeField] string m_BillingAccount;
        public string Name
        {
            get => string.IsNullOrEmpty(m_PlayerName) ? "player" : m_PlayerName;
            set => m_PlayerName = value;
        }
        public UserRequest Request => new UserRequest
        {
            name = Name,
            id = ID
        };
        public UserSetting Setting => new UserSetting(m_PlayerData);
        public List<InworldPlayerProfile> PlayerProfiles => m_PlayerData;
        public string BillingAccount
        {
            get => m_BillingAccount;
            set => m_BillingAccount = value;
        }
        public string Account => $"{Name}:{BillingAccount}";
        public string ID
        {
            get => m_UserID;
            set => m_UserID = value;
        }
        public List<InworldWorkspaceData> Workspace => m_Workspaces;
        public List<string> WorkspaceList => m_Workspaces.Select(ws => ws.displayName).ToList();
        public string GetWorkspaceFullName(string displayName) => m_Workspaces.FirstOrDefault(ws => ws.displayName == displayName)?.name;
        public InworldWorkspaceData GetWorkspaceByDisplayName(string displayName) => m_Workspaces.FirstOrDefault(ws => ws.displayName == displayName);
        public InworldSceneData GetSceneByFullName(string sceneFullName)
        {
            string workspaceName = sceneFullName.Substring(0, sceneFullName.IndexOf("/scenes/", StringComparison.Ordinal));
            InworldWorkspaceData wsData = m_Workspaces.FirstOrDefault(ws => ws.name == workspaceName);
            return wsData?.scenes.FirstOrDefault(scene => scene.name == sceneFullName);
        }
    }
}

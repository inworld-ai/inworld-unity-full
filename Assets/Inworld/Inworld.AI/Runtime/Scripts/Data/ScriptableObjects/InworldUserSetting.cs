/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inworld.Entities;

namespace Inworld
{

    public class InworldUserSetting : ScriptableObject
    {
        [SerializeField] string m_PlayerName;
        [SerializeField] Texture2D m_PlayerThumbnail;
        [SerializeField] List<PlayerProfileField> m_PlayerData = new List<PlayerProfileField>();
        [SerializeField] List<InworldProjectData> m_Projects = new List<InworldProjectData>();
        [SerializeField] List<InworldWorkspaceData> m_Workspaces = new List<InworldWorkspaceData>();
        [SerializeField] string m_UserID;
        [HideInInspector][SerializeField] string m_BillingAccount;
        /// <summary>
        /// Get/Set the player name, which will be displayed in the game.
        /// If you want to change the name in the runtime, you need to call SendUserConfig().
        /// </summary>
        public string Name
        {
            get => string.IsNullOrEmpty(m_PlayerName) ? "player" : m_PlayerName;
            set => m_PlayerName = value;
        }
        /// <summary>
        /// Get/Set the player's thumbnail. By default it would be used in the global chat panel.
        /// </summary>
        public Texture2D Thumbnail
        {
            get => m_PlayerThumbnail ? m_PlayerThumbnail : InworldAI.DefaultThumbnail;
            set => m_PlayerThumbnail = value;
        }
        /// <summary>
        /// Get the User request used for loading scene.
        /// </summary>
        public UserRequest Request => new UserRequest
        {
            name = Name,
            id = ID,
            userSettings = Setting
        };
        /// <summary>
        /// Get the User setting (Player profile in Inworld Studio)
        /// </summary>
        public UserSetting Setting => new UserSetting(m_PlayerData);
        /// <summary>
        /// Get the User Setting's player profile list.
        /// </summary>
        public List<PlayerProfileField> PlayerProfiles => m_PlayerData;
        /// <summary>
        /// Get the user's billing account. It's generated via Inworld Studio Panel.
        /// Please do not modify, otherwise the Inworld interaction will not work.
        /// </summary>
        public string BillingAccount
        {
            get
            {
                if (string.IsNullOrEmpty(m_BillingAccount))
                    m_BillingAccount = InworldAuth.Guid(); // YAN: Only for default demo workspace.
                return m_BillingAccount;
            }
            set => m_BillingAccount = value;
        }
        /// <summary>
        /// Get the account id that are sent to Inworld's data analyze server.
        /// </summary>
        public string Account => $"{BillingAccount}:{Name}";
        /// <summary>
        /// Get the user ID.
        /// It's generated via Inworld Studio Panel. Please do not modify, otherwise the Inworld interaction will not work.
        /// </summary>
        public string ID
        {
            get
            {
                if (string.IsNullOrEmpty(m_UserID))
                    m_UserID = InworldAuth.Guid();
                return m_UserID;
            }
            set => m_UserID = value;
        }
        /// <summary>
        /// Get the projects list by InworldProjectData.
        /// </summary>
        public List<InworldProjectData> Projects => m_Projects;
        /// <summary>
        /// Get the workspace list by InworldWorkspaceData.
        /// </summary>
        public List<InworldWorkspaceData> Workspace => m_Workspaces;
        /// <summary>
        /// Get the workspace list by workspace full name.
        /// </summary>
        public List<string> WorkspaceList => m_Workspaces.Select(ws => ws.displayName).ToList();
        public InworldProjectData GetProjectByDisplayName(string displayName) => m_Projects.FirstOrDefault(proj => proj.name.Contains(displayName));
        /// <summary>
        /// Get the workspace full name by its display name
        /// </summary>
        /// <param name="displayName">the display name of target workspace</param>
        public string GetWorkspaceFullName(string displayName) => m_Workspaces.FirstOrDefault(ws => ws.displayName == displayName)?.name;
        /// <summary>
        /// Get the InworldWorkspaceData by its display name
        /// </summary>
        /// <param name="displayName">the display name of target workspace</param>
        public InworldWorkspaceData GetWorkspaceByDisplayName(string displayName) => m_Workspaces.FirstOrDefault(ws => ws.displayName == displayName);
        /// <summary>
        /// Get the InworldSceneData by its display name
        /// </summary>
        /// <param name="sceneFullName">the display name of target scene</param>
        public InworldSceneData GetSceneByFullName(string sceneFullName)
        {
            string workspaceName = sceneFullName.Substring(0, sceneFullName.IndexOf("/scenes/", StringComparison.Ordinal));
            InworldWorkspaceData wsData = m_Workspaces.FirstOrDefault(ws => ws.name == workspaceName);
            return wsData?.scenes.FirstOrDefault(scene => scene.name == sceneFullName);
        }
        public InworldCharacterData GetCharacterByFullName(string characterFullName)
        {
            string workspaceName = characterFullName.Substring(0, characterFullName.IndexOf("/characters/", StringComparison.Ordinal));
            InworldWorkspaceData wsData = m_Workspaces.FirstOrDefault(ws => ws.name == workspaceName);
            return wsData?.characters.FirstOrDefault(character => character.brainName == characterFullName);
        }
    }
}

﻿/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Grpc;
using Inworld.Sample;
using UnityEngine;
namespace Inworld.Util
{
    /// <summary>
    ///     The default setting class for the path of all the default assets or references.
    ///     Could have multiple copies.
    ///     Its name start as "DefaultSettings".
    /// </summary>
    public class InworldSettings : ScriptableObject
    {
        #region Inspector Variables
        [Header("Game Settings:")]
        [Tooltip("These settings are not editable during runtime.")]
        [SerializeField] bool m_CanReceiveAudio = true;
        [SerializeField] bool m_Interruptible = false;
        [SerializeField] bool m_SaveConversation = false;
        [Space(10)][Header("Local File Path:")]
        [SerializeField] string m_ThumbnailFolder;
        [SerializeField] string m_AvatarFolder;
        [Space(10)][Header("Resource File Path:")]
        [SerializeField] string m_CharacterDataFolder;
        [SerializeField] string m_SceneDataFolder;
        [SerializeField] string m_WorkspaceDataFolder;
        [SerializeField] string m_KeySecretDataFolder;
        [Space(10)][Header("Default Resources:")]
        [SerializeField] Texture2D m_DefaultThumbnail;
        [SerializeField] GameObject m_DefaultAvatar;
        [SerializeField] MainCanvas m_MainCanvas;
        [SerializeField] InworldWorkspaceData m_DefaultWorkspace;
        [Space(10)][Header("Debug:")]
        [SerializeField] bool m_EnableVerboseLog;
        [SerializeField] bool m_EnableSharedCharacters = true;
        [SerializeField] bool m_AutoSelectCharacter = true;
        [SerializeField] bool m_AutoGenerateCharacter = true;
        #endregion

        #region Properties
        public bool AutoSelectCharacter => m_AutoSelectCharacter;
        /// <summary>
        ///     Get if server will send audio voices to client.
        /// </summary>
        public bool ReceiveAudio => m_CanReceiveAudio;
        /// <summary>
        ///     Get if player can interrupt character speaking.
        /// </summary>
        public bool Interruptible => m_Interruptible;
        /// <summary>
        ///     The game will save and load conversation if checked.
        /// </summary>
        public bool SaveConversation => m_SaveConversation;
        /// <summary>
        ///     Returns the Thumbnails Path.
        /// </summary>
        public string ThumbnailPath => m_ThumbnailFolder;
        /// <summary>
        ///     Returns the Avatars Path.
        /// </summary>
        public string AvatarPath => m_AvatarFolder;
        /// <summary>
        ///     Returns the Inworld Workspace Data Path.
        /// </summary>
        public string WorkspaceDataPath => m_WorkspaceDataFolder;
        /// <summary>
        ///     Returns the Inworld KeySecret Data Path.
        /// </summary>
        public string KeySecretDataPath => m_KeySecretDataFolder;
        /// <summary>
        ///     Returns the Inworld Scene Data Path.
        /// </summary>
        public string InworldSceneDataPath => m_SceneDataFolder;
        /// <summary>
        ///     Returns the Character Data Path.
        /// </summary>
        public string CharacterDataPath => m_CharacterDataFolder;
        /// <summary>
        ///     Returns the Default Thumbnail.
        /// </summary>
        public Texture2D DefaultThumbnail => m_DefaultThumbnail;
        /// <summary>
        ///     Returns the Default Avatar.
        /// </summary>
        public GameObject DefaultAvatar
        {
            get => m_DefaultAvatar;
            set => m_DefaultAvatar = value;
        }
        /// <summary>
        ///     Returns the Default Workspace Data.
        /// </summary>
        public InworldWorkspaceData DefaultWorkspace => m_DefaultWorkspace;
        /// <summary>
        ///     Returns if it's in Verbose Log Mode.
        /// </summary>
        public bool IsVerboseLog => m_EnableVerboseLog;
        /// <summary>
        ///     Returns if it receives ML animation data from server.
        /// </summary>
        public bool AutoGenerateCharacter => m_AutoGenerateCharacter;
        /// <summary>
        ///     Returns if Sharing Characters are enabled to be loaded.
        /// </summary>
        public bool EnableSharedCharacters => m_EnableSharedCharacters;
        /// <summary>
        ///     Returns the main canvas.
        /// </summary>
        public MainCanvas MainCanvas => m_MainCanvas;
        /// <summary>
        ///     Returns the capabilities settings for communicating with Inworld Server.
        /// </summary>
        public CapabilitiesRequest Capabilities => new CapabilitiesRequest
        {
            Audio = m_CanReceiveAudio,
            NarratedActions = true,
            Emotions = true,
            Relations = true,
            Interruptions = true,
            Text = true,
            DebugInfo = true,
#pragma warning disable CS0612
            Triggers = true, // YAN: Although it's called obsolete, we cannot remove this settings now. Need feedback from backend.
#pragma warning restore CS0612
            TurnBasedStt = !m_Interruptible,
            PhonemeInfo = true
        };
        #endregion
    }
}

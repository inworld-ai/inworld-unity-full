/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Model;
using System;
using UnityEngine;
namespace Inworld.Util
{
    /// <summary>
    ///     The master data object for storing all the settings.
    ///     It's singleton. Please make sure there's only 1 copy,
    ///     and stored in "Assets/.../Resources/GlobalSettings/InworldAI.asset".
    /// </summary>
    public class InworldAI : ScriptableObject
    {
        #region Inspector Variables
        [Header(k_GlobalInstruction)]
        [Space(10)]
        [SerializeField] InworldGameSettings m_GameSettings;
        [SerializeField] InworldSettings m_DefaultSettings;
        [SerializeField] InworldUserSettings m_UserSettings;
        [SerializeField] InworldUISettings m_UISettings;
        [Space(10)]
        [SerializeField] InworldFileDownloader m_FileDownloader;
        [SerializeField] GameObject m_AvatarLoader;
        [SerializeField] GameObject m_PlayerController;
        [Space(10)]
        [SerializeField] InworldController m_ControllerPrefab;
        [SerializeField] InworldCharacter m_CharacterPrefab;
        #endregion

        #region Private Variables
        public const string k_CompanyName = "Inworld.AI";
        const string k_GlobalDataPath = "GlobalSettings/InworldAI";
        const string k_GlobalInstruction = "Double-click the following settings to change:";
        static InworldAI __inst;
        #endregion

        #region Properties
        /// <summary>
        ///     Get the instance of the InworldAI.
        ///     By default, it's stored at "Inworld.AI/Resources/GlobalSettings/InworldAI.asset".
        ///     Please do not modify it.
        /// </summary>
        public static InworldAI Instance
        {
            get
            {
                if (__inst)
                    return __inst;
                __inst = Resources.Load<InworldAI>(k_GlobalDataPath);
                return __inst;
            }
        }
        /// <summary>
        ///     Get InworldFileDownloader scriptableObject.
        ///     InworldFileDownloader is the downloader to fetch thumbnails/models of InworldCharacterData.
        /// </summary>
        public static InworldFileDownloader File => Instance.m_FileDownloader;
        /// <summary>
        ///     Get InworldGameSettings scriptableObject.
        ///     InworldGameSettings is the scriptableObject instance that always
        ///     update the current workspace/scene/character data, etc.
        /// </summary>
        public static InworldGameSettings Game
        {
            get => Instance.m_GameSettings; 
            set => Instance.m_GameSettings = value;
        }
        /// <summary>
        ///     Get InworldUISettings scriptableObject.
        /// </summary>
        public static InworldUISettings UI => Instance.m_UISettings;
        /// <summary>
        ///     Get DefaultSettings scriptableObject.
        ///     This data could also be modified in "Edit > Preferences > Inworld.AI"
        /// </summary>
        public static InworldSettings Settings => Instance.m_DefaultSettings;
        /// <summary>
        ///     Get InworldUserSettings scriptableObject.
        ///     This data could also be modified in "Edit > Project Settings > Inworld.AI"
        ///     Not only does it contain user's userName or companyName,
        ///     it also stores user's userToken,
        ///     and its workspace/scene/character/model/thumbnails that retrieved from server.
        /// </summary>
        public static InworldUserSettings User => Instance.m_UserSettings;
        /// <summary>
        ///     Get the AvatarLoader of current package settings.
        ///     By default, Inworld uses GLTFAvatarloader to load Ready Player Me Avatars.
        ///     If you'd like to use your own avatar,
        ///     please let your own Avatarloader and inherit with IAvatarLoader.
        /// </summary>
        public static IAvatarLoader AvatarLoader => Instance.m_AvatarLoader.GetComponent<IAvatarLoader>();
        /// <summary>
        ///     Get the InworldController Prefab
        ///     InworldController is also a singleton object.
        ///     If you want to utilize Inworld feature,
        ///     you should make sure there's one and only one InworldController in your Unity scene.
        /// </summary>
        public static InworldController ControllerPrefab => Instance.m_ControllerPrefab;
        /// <summary>
        ///     Get the InworldCharacter Prefab.
        ///     InworldCharacter contains audio source, but doesn't contain models.
        ///     Usually, InworldCharacter should be added to loaded models by GLTFAvatarLoader.
        /// </summary>
        public static InworldCharacter CharacterPrefab => Instance.m_CharacterPrefab;
        /// <summary>
        ///     Get the PlayerController Prefab.
        ///     PlayerController is the object that contains main camera, simple camera controller.
        ///     It also contains global chat panel.
        /// </summary>
        public static GameObject PlayerControllerPrefab => Instance.m_PlayerController;
        /// <summary>
        ///     Get if it's in Debug Mode.
        ///     DebugMode could be set in Edit > Preferences > Inworld.AI > IsVerboseLog,
        ///     or Default Settings.asset.
        /// </summary>
        public static bool IsDebugMode => Instance.m_DefaultSettings.IsVerboseLog;
        #endregion

        #region Functions
        /// <summary>
        ///     Send debug log.
        ///     If IsVerboseLog is checked, it'll also be displayed in console.
        /// </summary>
        /// <param name="log">log to send</param>
        public static void Log(string log)
        {
            InworldLog.Log(log);
        }
        /// <summary>
        ///     Send warning type of debug log.
        ///     If IsVerboseLog is checked, it'll also be displayed in console.
        /// </summary>
        /// <param name="log">log to send</param>
        public static void LogWarning(string log)
        {
            InworldLog.LogWarning(log);
        }
        /// <summary>
        ///     Send error type of debug log.
        ///     If IsVerboseLog is checked, it'll also be displayed in console.
        /// </summary>
        /// <param name="log">log to send</param>
        public static void LogError(string log)
        {
            InworldLog.LogError(log);
        }
        public static void LogException(string exception) => InworldLog.LogException(exception);
        #endregion
    }
}

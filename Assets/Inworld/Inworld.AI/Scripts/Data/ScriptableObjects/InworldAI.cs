/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld.Sample;
using Inworld.Entities;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Inworld
{
    public class InworldAI : ScriptableObject
    {
        [SerializeField] InworldUserSetting m_UserSetting;

        [Header("Default Assets")]
        [SerializeField] Capabilities m_Capabilities;
        [SerializeField] Texture2D m_DefaultThumbnail;
        [SerializeField] InworldUserSetting m_DefaultUserSetting;
        [SerializeField] InworldController m_ControllerPrefab;
        [SerializeField] SplashScreen m_SplashScreen;
        [Space(10)]
        [SerializeField] string m_Version;
        [Space(10)][SerializeField] bool m_DebugMode;
        public const string k_CompanyName = "Inworld.AI";
        const string k_GlobalDataPath = "InworldAI";
        static InworldAI __inst;

        /// <summary>
        /// Gets an instance of InworldAI.
        /// By default, it is at `Assets/Inworld/Inworld.AI/Resources/InworldAI.asset`.
        /// Please do not modify it.
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
        /// Gets/Sets the current User Setting.
        /// </summary>
        public static InworldUserSetting User
        {
            get => Instance.m_UserSetting ? Instance.m_UserSetting : Instance.m_DefaultUserSetting;
            set => Instance.m_UserSetting = value;
        }
        /// <summary>
        /// Gets if it's in debug mode. Could be toggled in the `InworldAI.asset`.
        /// </summary>
        public static bool IsDebugMode => Instance.m_DebugMode;

        /// <summary>
        /// Get the default Client Request that are sending to the server.
        /// </summary>
        public static Client UnitySDK => new Client
        {
            id = "unity",
            version = Version,
            description = $"{Protocol}; {Version}; {Application.unityVersion}; {SystemInfo.operatingSystem}; {Application.productName}"
        };
        /// <summary>
        /// Gets the Splash Screen prefab. Could be set in the `InworldAI.asset`.
        /// </summary>
        public static SplashScreen SplashScreen => Instance.m_SplashScreen;
        /// <summary>
        /// Gets the controller prefab to instantiate. Usually used in Editor scripts.
        /// </summary>
        public static InworldController ControllerPrefab => Instance.m_ControllerPrefab;
        /// <summary>
        /// Get the current capabilities.
        /// Capabilities are the settings for loading scene.
        /// </summary>
        public static Capabilities Capabilities
        {
            get => Instance.m_Capabilities;
            set => Instance.m_Capabilities = value;
        }
        /// <summary>
        /// Get the default thumbnail for player and characters.
        /// This thumbnail is used in the demo chat panel. 
        /// </summary>
        public static Texture2D DefaultThumbnail => Instance.m_DefaultThumbnail;
        /// <summary>
        /// Get the path for all the Inworld assets.
        /// </summary>
        public static string InworldPath => "Assets/Inworld";
        /// <summary>
        /// String of protocal. Set in runtime.
        /// </summary>
        public static string Protocol { get; set; }
        /// <summary>
        /// Get the current version of Inworld Unity SDK.
        /// </summary>
        public static string Version
        {
            get => Instance ? Instance.m_Version : "";
            set
            {
                if (!Instance)
                    return;
                #if UNITY_EDITOR
                Instance.m_Version = value;
                EditorUtility.SetDirty(Instance);
                AssetDatabase.SaveAssets();
                #endif
            }
        }
        /// <summary>
        /// Logs a basic type of debug message used in Inworld.
        /// If the DebugMode is enabled, the message will also be displayed in the console.
        /// </summary>
        /// <param name="log">The message to log</param>
        public static void Log(string log)
        {
            if (IsDebugMode)
                InworldLog.Log(log);
        }
        /// <summary>
        /// Logs a warning type of debug message used in Inworld.
        /// If the DebugMode is enabled, the message will also be displayed in the console.
        /// </summary>
        /// <param name="log">the warning message to log</param>
        public static void LogWarning(string log)
        {
            if (IsDebugMode)
                InworldLog.LogWarning(log);
        }
        /// <summary>
        /// Logs an error type of debug message used in Inworld.
        /// If the DebugMode is enabled, the message will also be displayed in the console.
        /// </summary>
        /// <param name="log">the error message to log</param>
        public static void LogError(string log)
        {
            if (IsDebugMode)
                InworldLog.LogError(log);
        }
        /// <summary>
        /// Logs an exception message used in Inworld.
        /// This method is used to log exceptions in Inworld, and the provided exception message will be recorded.
        /// </summary>
        /// <param name="exception">The exception message to log</param>
        public static void LogException(string exception) => InworldLog.LogException(exception);
    }
}


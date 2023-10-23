/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Inworld;
using Inworld.Sample;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

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

    const string k_GlobalDataPath = "InworldAI";
    static InworldAI __inst;
    
    /// <summary>
    /// Gets an instance of [**InworldAI**](InworldAI).
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
        id = "unity"
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
    /// Send basic type of the debug log used in Inworld.
    /// If DebugMode is checked, it'll also be displayed in console.
    /// </summary>
    /// <param name="log">log to send</param>
    public static void Log(string log)
    {
        if (IsDebugMode)
            InworldLog.Log(log);
    }
    /// <summary>
    /// Send warning type of the debug log used in Inworld.
    /// If DebugMode is checked, it'll also be displayed in console.
    /// </summary>
    /// <param name="log">log to send</param>
    public static void LogWarning(string log)
    {
        if (IsDebugMode)
            InworldLog.LogWarning(log);
    }
    /// <summary>
    /// Send error type of the debug log used in Inworld.
    /// If DebugMode is checked, it'll also be displayed in console.
    /// </summary>
    /// <param name="log">log to send</param>
    public static void LogError(string log)
    {
        if (IsDebugMode)
            InworldLog.LogError(log);
    }
    public static void LogException(string exception) => InworldLog.LogException(exception);
    

}

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

    public const string k_CompanyName = "Inworld.AI";
    const string k_GlobalDataPath = "InworldAI";
    static InworldAI __inst;
    
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

    public static InworldUserSetting User
    {
        get => Instance.m_UserSetting ? Instance.m_UserSetting : Instance.m_DefaultUserSetting;
        set => Instance.m_UserSetting = value;
    }
    public static bool IsDebugMode => Instance.m_DebugMode;
    public static Client UnitySDK => new Client
    {
        id = "unity"
    };
    public static SplashScreen SplashScreen => Instance.m_SplashScreen;
    public static InworldController ControllerPrefab => Instance.m_ControllerPrefab;
    public static Capabilities Capabilities
    {
        get => Instance.m_Capabilities;
        set => Instance.m_Capabilities = value;
    }
    public static Texture2D DefaultThumbnail => Instance.m_DefaultThumbnail;
    public static void Log(string log)
    {
        if (IsDebugMode)
            InworldLog.Log(log);
    }
    /// <summary>
    ///     Send warning type of debug log.
    ///     If IsVerboseLog is checked, it'll also be displayed in console.
    /// </summary>
    /// <param name="log">log to send</param>
    public static void LogWarning(string log)
    {
        if (IsDebugMode)
            InworldLog.LogWarning(log);
    }
    /// <summary>
    ///     Send error type of debug log.
    ///     If IsVerboseLog is checked, it'll also be displayed in console.
    /// </summary>
    /// <param name="log">log to send</param>
    public static void LogError(string log)
    {
        if (IsDebugMode)
            InworldLog.LogError(log);
    }
    public static void LogException(string exception) => InworldLog.LogException(exception);
    
    public static string InworldPath => "Assets/Inworld";
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
}

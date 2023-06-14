using Inworld;
using System.Diagnostics;
using UnityEngine;

public class InworldAI : ScriptableObject
{
    [SerializeField] InworldUserSetting m_UserSetting;
    [SerializeField] Texture2D m_DefaultThumbnail;
    [SerializeField] Capabilities m_Capabilities;
    [Space(10)][SerializeField] bool m_DebugMode;
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

    public static InworldUserSetting User => Instance.m_UserSetting;
    public static bool IsDebugMode => Instance.m_DebugMode;
    public static Client UnitySDK => new Client
    {
        id = "unity"
    };
    public static Capabilities Capabilities => Instance.m_Capabilities;
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
}

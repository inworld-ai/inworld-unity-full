using Inworld;
using UnityEditor;
using UnityEngine;

public class InworldAI : ScriptableObject
{
    [SerializeField] InworldUserSetting m_UserSetting;
    [SerializeField] Texture2D m_DefaultThumbnail;
    [SerializeField] Capabilities m_Capabilities;
    [SerializeField] string m_Version;
    [SerializeField] string m_ImportedTime;
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

    public static InworldUserSetting User
    {
        get => Instance.m_UserSetting;
        set => Instance.m_UserSetting = value;
    }
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
    
    public static string ImportedTime //TODO(Yan): Move to InworldEditor.asset
    {
        get => Instance ? Instance.m_ImportedTime : "";
        set 
        {
            if (!Instance)
                return;
            #if !UNITY_WEBGL
            Instance.m_ImportedTime = value;
            EditorUtility.SetDirty(Instance);
            AssetDatabase.SaveAssets();
            #endif
        }
    }
    
    public static string Version //TODO(Yan): Move to InworldEditor.asset
    {
        get => Instance ? Instance.m_Version : "";
        set 
        {
            if (!Instance)
                return;
            #if !UNITY_WEBGL
            Instance.m_Version = value;
            EditorUtility.SetDirty(Instance);
            AssetDatabase.SaveAssets();
            #endif
        }
    }
}

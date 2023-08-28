using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Inworld.AI.Editor
{
    public enum EditorStatus
    {
        Init,
        SelectGameData,
        SelectCharacter,
        Error
    }
    public class InworldEditor : ScriptableObject
    {
        [Header("Assets")]
        [SerializeField] Texture2D m_Banner;
        [SerializeField] Readme m_Readme;
        [SerializeField] bool m_DisplayReadmeOnLoad;
        [SerializeField] InworldCharacter m_Character2DPrefab;
        // TODO(Yan): Let other package's editor script to Upload those characters.
        [SerializeField] InworldCharacter m_RPMPrefab;
        [SerializeField] InworldCharacter m_InnequinPrefab;
        [Space(10)][Header("Status")]
        [SerializeField] EditorStatus m_CurrentStatus;
        EditorStatus m_LastStatus;
        [Header("Paths:")]
        [SerializeField] string m_UserDataPath;
        [SerializeField] string m_GameDataPath;
        [SerializeField] string m_ThumbnailPath;
        [SerializeField] string m_AvatarPath;
        [Header("URLs:")]
        [SerializeField] InworldServerConfig m_ServerConfig;
        [SerializeField] string m_BillingAccountURL;
        [SerializeField] string m_WorkspaceURL;
        [SerializeField] string m_KeyURL;
        [SerializeField] string m_ScenesURL;
        
        const string k_GlobalDataPath = "InworldEditor";
        static InworldEditor __inst;
        
        Dictionary<EditorStatus, IEditorState> m_InworldEditorStates = new Dictionary<EditorStatus, IEditorState>();
        string m_StudioTokenForExchange;
        string m_ErrorMsg;

        public static InworldEditor Instance
        {
            get
            {
                if (__inst)
                    return __inst;
                __inst = Resources.Load<InworldEditor>(k_GlobalDataPath);
                return __inst;
            }
        }
        public static Texture2D Banner => Instance.m_Banner;
        public static bool LoadedReadme
        {
            get => Instance.m_DisplayReadmeOnLoad;
            set => Instance.m_DisplayReadmeOnLoad = value;
        }
        public static Readme ReadMe => Instance.m_Readme;

        public EditorStatus Status
        {
            get => m_CurrentStatus;
            set
            {
                m_LastStatus = m_CurrentStatus;
                m_CurrentStatus = value;
                LastState.OnExit();
                CurrentState.OnEnter();
            }
        }
        public IEditorState LastState => m_InworldEditorStates[m_LastStatus];
        public IEditorState CurrentState => m_InworldEditorStates[m_CurrentStatus];
        public static string UserDataPath => $"{InworldAI.InworldPath}/{Instance.m_UserDataPath}";
        public static string GameDataPath => Instance.m_GameDataPath;
        public static string ThumbnailPath => Instance.m_ThumbnailPath;
        public static string AvatarPath => Instance.m_AvatarPath;
        void OnEnable()
        {
            m_InworldEditorStates[EditorStatus.Init] = new InworldEditorInit();
            m_InworldEditorStates[EditorStatus.SelectGameData] = new InworldEditorSelectGameData();
            m_InworldEditorStates[EditorStatus.SelectCharacter] = new InworldEditorSelectCharacter();
            m_InworldEditorStates[EditorStatus.Error] = new InworldEditorError();
        }
        public static string TokenForExchange
        {
            get => Instance.m_StudioTokenForExchange;
            set => Instance.m_StudioTokenForExchange = value;
        }
        public static string Token => $"Bearer {TokenForExchange.Split(':')[0]}";

        public GUIStyle TitleStyle => new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(10, 10, 0, 0)
        };
        public GUIStyle BtnStyle => new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            fixedWidth = 100,
            margin = new RectOffset(10, 10, 10, 10),
        };
        public GUIStyle DropDownStyle => new GUIStyle("MiniPullDown")
        {
            margin = new RectOffset(10, 10, 0, 0)
        };
        public static string BillingAccountURL => $"https://{Instance.m_ServerConfig.web}/v1alpha/{Instance.m_BillingAccountURL}";
        public static string ListWorkspaceURL => $"https://{Instance.m_ServerConfig.web}/v1alpha/{Instance.m_WorkspaceURL}";
        public static string ListScenesURL(string wsFullName) => $"https://{Instance.m_ServerConfig.web}/v1alpha/{wsFullName}/{Instance.m_ScenesURL}";
        public static string ListKeyURL(string wsFullName) => $"https://{Instance.m_ServerConfig.web}/v1alpha/{wsFullName}/{Instance.m_KeyURL}";
        public string Error
        {
            get => m_ErrorMsg;
            set
            {
                Debug.LogError(value);
                Status = EditorStatus.Error;
                m_ErrorMsg = value;
            }
        }
        public void SaveData()
        {
            EditorUtility.SetDirty(InworldAI.Instance);
            EditorUtility.SetDirty(InworldAI.User);
            EditorUtility.SetDirty(Instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

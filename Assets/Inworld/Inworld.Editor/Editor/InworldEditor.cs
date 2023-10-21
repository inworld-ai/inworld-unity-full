/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] PlayerController m_PlayerController;
        [Space(10)][Header("Status")]
        [SerializeField] EditorStatus m_CurrentStatus;
        EditorStatus m_LastStatus;
        [Header("Paths:")]
        [SerializeField] string m_UserDataPath;
        [SerializeField] string m_GameDataPath;
        [SerializeField] string m_ThumbnailPath;
        [SerializeField] string m_AvatarPath;
        [SerializeField] string m_PrefabPath;
        [Header("URLs:")]
        [SerializeField] InworldServerConfig m_ServerConfig;
        [SerializeField] string m_BillingAccountURL;
        [SerializeField] string m_WorkspaceURL;
        [SerializeField] string m_KeyURL;
        [SerializeField] string m_ScenesURL;
        
        const string k_GlobalDataPath = "InworldEditor";
        public const string k_TokenErrorInstruction = "Token Error or Expired.\nPlease login again";
        const float k_LuminanceRed = 0.2126f;
        const float k_LuminanceGreen = 0.7152f;
        const float k_LuminanceBlue = 0.0722f;
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
        public static string PrefabPath => Instance.m_PrefabPath;
        public static bool Is3D => Instance.m_RPMPrefab != null || Instance.m_InnequinPrefab != null;
        public static PlayerController PlayerController => Instance.m_PlayerController;
        public static bool UseInnequin => Instance.m_InnequinPrefab != null;
        public InworldCharacter DefaultPrefab => Is3D ? UseInnequin ? m_InnequinPrefab : m_RPMPrefab : m_Character2DPrefab;
        public InworldCharacter RPMPrefab
        {
            get => m_RPMPrefab;
            set => m_RPMPrefab = value;
        }
        public InworldCharacter InnequinPrefab
        {
            get => m_InnequinPrefab;
            set => m_InnequinPrefab = value;
        }
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

        public static string GetError(string strErrorFromWeb) => strErrorFromWeb.Contains("Unauthorized") ? k_TokenErrorInstruction : strErrorFromWeb;

        float GetLuminance(Color color) => k_LuminanceRed * color.r + k_LuminanceGreen * color.g + k_LuminanceBlue * color.b;

        public GUIStyle TitleStyle => new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(10, 10, 0, 0)
        };
        public GUIStyle ErrorStyle => new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            normal = new GUIStyleState
            {
                textColor = Color.red
            },
            padding = new RectOffset(10, 10, 10, 50),
            wordWrap = true
        };
        public GUIStyle BtnStyle => new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            fixedWidth = 100,
            margin = new RectOffset(10, 10, 10, 10),
        };
        public GUIStyle BtnCharStyle(Texture2D bg)
        {
            float avgLuminance = bg == InworldAI.DefaultThumbnail ? 0 : bg.GetPixels().Average(GetLuminance);
            return new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 100,
                fixedWidth = 100,
                margin = new RectOffset(10, 10, 10, 10),
                alignment = TextAnchor.LowerCenter,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = avgLuminance > 0.5 ? Color.black : Color.white,
                    background = bg
                }
            };
        }

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
#endif
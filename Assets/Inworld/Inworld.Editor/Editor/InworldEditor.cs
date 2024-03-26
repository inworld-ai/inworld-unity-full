/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Inworld.Sample;
using Inworld.UI;
using UnityEngine.Serialization;

namespace Inworld.Editors
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
        [FormerlySerializedAs("m_Readme")][SerializeField] InworldReadme m_InworldReadme;
        [SerializeField] bool m_DisplayReadmeOnLoad;
        [SerializeField] InworldController m_ControllerPrefab;
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

        /// <summary>
        /// Gets an instance of InworldEditor.
        /// By default, it is at `Assets/Inworld/Inworld.Editor/Resources/InworldEditor.asset`.
        /// Please do not modify it.
        /// </summary>
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
        /// <summary>
        /// Gets the banner image displayed at the top of the Inworld Studio Panel.
        /// </summary>
        public static Texture2D Banner => Instance.m_Banner;
        /// <summary>
        /// Gets/Sets if you want the Readme to be displayed on load.
        /// </summary>
        public static bool LoadedReadme
        {
            get => Instance.m_DisplayReadmeOnLoad;
            set => Instance.m_DisplayReadmeOnLoad = value;
        }
        /// <summary>
        /// Gets the default Readme asset.
        /// </summary>
        public static InworldReadme ReadMe => Instance.m_InworldReadme;
        /// <summary>
        /// Gets/Sets the current status of Inworld Editor.
        /// </summary>
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
        /// <summary>
        /// Gets the last Editor State.
        /// </summary>
        public IEditorState LastState => m_InworldEditorStates[m_LastStatus];
        /// <summary>
        /// Gets the current Editor State.
        /// </summary>
        public IEditorState CurrentState => m_InworldEditorStates[m_CurrentStatus];
        /// <summary>
        /// Gets the location for generating and storing user data.
        /// </summary>
        public static string UserDataPath => $"{InworldAI.InworldPath}/{Instance.m_UserDataPath}";
        /// <summary>
        /// Gets the location for generating and storing game data.
        /// </summary>
        public static string GameDataPath => Instance.m_GameDataPath;
        /// <summary>
        /// Gets the location for downloading and storing thumbnails.
        /// </summary>
        public static string ThumbnailPath => Instance.m_ThumbnailPath;
        /// <summary>
        /// Gets the location for downloading and storing user data.
        /// </summary>
        public static string AvatarPath => Instance.m_AvatarPath;
        /// <summary>
        /// Gets the location for generating and storing the prefabs for the Inworld character.
        /// </summary>
        public static string PrefabPath => Instance.m_PrefabPath;
        /// <summary>
        /// Gets if the current Inworld Character prefab is 3D.
        /// </summary>
        public static bool Is3D => Instance.m_RPMPrefab != null || Instance.m_InnequinPrefab != null;
        /// <summary>
        /// Gets the current Player Controller prefab.
        /// </summary>
        public static PlayerController PlayerController => Instance.m_PlayerController;
        /// <summary>
        /// Gets if it's using Innequin model.
        /// </summary>
        public static bool UseInnequin => Instance.m_InnequinPrefab != null;
        /// <summary>
        /// Gets the default prefab for Inworld Controller
        /// </summary>
        public InworldController ControllerPrefab => m_ControllerPrefab;
        /// <summary>
        /// Gets the current default prefab for Inworld character.
        /// </summary>
        public InworldCharacter DefaultPrefab => Is3D ? UseInnequin ? m_InnequinPrefab : m_RPMPrefab : m_Character2DPrefab;
        /// <summary>
        /// Gets the current prefab for Inworld Character with Ready Player Me implementation.
        /// </summary>
        public InworldCharacter RPMPrefab
        {
            get => m_RPMPrefab;
            set => m_RPMPrefab = value;
        }
        /// <summary>
        /// Gets the current prefab for Inworld Character with Innequin implementation.
        /// </summary>
        public InworldCharacter InnequinPrefab
        {
            get => m_InnequinPrefab;
            set => m_InnequinPrefab = value;
        }
        /// <summary>
        /// Gets/Sets the token used for login Inworld Studio.
        /// </summary>
        public static string TokenForExchange
        {
            get => Instance.m_StudioTokenForExchange;
            set => Instance.m_StudioTokenForExchange = value;
        }
        /// <summary>
        /// Gets the actual token part.
        /// </summary>
        public static string Token => $"Bearer {TokenForExchange.Split(':')[0]}";

        /// <summary>
        /// Gets the GUI style for the title in Inworld Studio Panel.
        /// </summary>
        public GUIStyle TitleStyle => new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(10, 10, 0, 0),
            wordWrap = true
        };
        /// <summary>
        /// Gets the GUI style for the error text in Inworld Studio Panel.
        /// </summary>
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
        /// <summary>
        /// Gets the GUI style for the button in Inworld Studio Panel.
        /// </summary>
        public GUIStyle BtnStyle => new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            fixedWidth = 100,
            margin = new RectOffset(10, 10, 10, 10),
        };
        /// <summary>
        /// Gets the GUI style for the drop down fields in Inworld Studio Panel.
        /// </summary>
        public GUIStyle DropDownStyle => new GUIStyle("MiniPullDown")
        {
            margin = new RectOffset(10, 10, 0, 0)
        };
        /// <summary>
        /// Gets the URL for fetching billing account.
        /// </summary>
        public static string BillingAccountURL => $"https://{Instance.m_ServerConfig.web}/v1alpha/{Instance.m_BillingAccountURL}";
        /// <summary>
        /// Gets the URL for listing workspaces
        /// </summary>
        public static string ListWorkspaceURL => $"https://{Instance.m_ServerConfig.web}/v1alpha/{Instance.m_WorkspaceURL}";
        /// <summary>
        /// Gets/Sets the current Error message.
        /// If setting, also set the current status of InworldEditor.
        /// </summary>
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
        /// <summary>
        /// Get the error messages, if it's related to `Unauthorized`, rename it with a better explanation.
        /// </summary>
        /// <param name="strErrorFromWeb">the received error message.</param>
        public static string GetError(string strErrorFromWeb) => strErrorFromWeb.Contains("Unauthorized") ? k_TokenErrorInstruction : strErrorFromWeb;
        /// <summary>
        /// Get the picture's luminance. Used to display the opposite color of the text.
        /// </summary>
        /// <param name="color">the color of a pixel.</param>
        float GetLuminance(Color color) => k_LuminanceRed * color.r + k_LuminanceGreen * color.g + k_LuminanceBlue * color.b;
        /// <summary>
        /// Gets the GUI style for the text floating on the thumbnails.
        /// </summary>
        /// <param name="bg">The character's thumbnail</param>
        /// <returns></returns>
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
        /// <summary>
        /// Gets the url for listing Inworld scenes.
        /// </summary>
        /// <param name="wsFullName">the full name of the target workspace</param>
        public static string ListScenesURL(string wsFullName) => $"https://{Instance.m_ServerConfig.web}/v1alpha/{wsFullName}/{Instance.m_ScenesURL}";
        /// <summary>
        /// Gets the url for listing keys.
        /// </summary>
        /// <param name="wsFullName">the full name of the target workspace</param>
        public static string ListKeyURL(string wsFullName) => $"https://{Instance.m_ServerConfig.web}/v1alpha/{wsFullName}/{Instance.m_KeyURL}";

        /// <summary>
        /// Save all the current scriptable objects.
        /// </summary>
        public void SaveData()
        {
            EditorUtility.SetDirty(InworldAI.Instance);
            EditorUtility.SetDirty(InworldAI.User);
            EditorUtility.SetDirty(Instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        void OnEnable()
        {
            m_InworldEditorStates[EditorStatus.Init] = new InworldEditorInit();
            m_InworldEditorStates[EditorStatus.SelectGameData] = new InworldEditorSelectGameData();
            m_InworldEditorStates[EditorStatus.SelectCharacter] = new InworldEditorSelectCharacter();
            m_InworldEditorStates[EditorStatus.Error] = new InworldEditorError();
        }
    }
}
#endif
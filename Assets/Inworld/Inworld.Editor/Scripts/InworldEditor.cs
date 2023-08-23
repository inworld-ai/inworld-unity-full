using System.Collections.Generic;
using UnityEngine;

namespace Inworld.AI.Editor
{
    public enum EditorStatus
    {
        Init,
        SelectWorkspace,
        SelectKeySecret,
        SelectCharacter,
        Error
    }
    public class InworldEditor : ScriptableObject
    {
        [SerializeField] Texture2D m_Banner;
        [SerializeField] Readme m_Readme;
        [SerializeField] bool m_DisplayReadmeOnLoad;
        [SerializeField] EditorStatus m_CurrentStatus;
        
        const string k_GlobalDataPath = "InworldEditor";
        static InworldEditor __inst;
        
        Dictionary<EditorStatus, IEditorState> m_InworldEditorStates = new Dictionary<EditorStatus, IEditorState>();
        string m_StudioToken;
        GUIStyle m_BtnStyle;
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

        public static EditorStatus Status
        {
            get => Instance.m_CurrentStatus;
            set => Instance.m_CurrentStatus = value;
        }
        public IEditorState CurrentState => m_InworldEditorStates[m_CurrentStatus];
        void OnEnable()
        {
            m_InworldEditorStates[EditorStatus.Init] = new InworldEditorInit();
            m_InworldEditorStates[EditorStatus.SelectWorkspace] = new InworldEditorSelectWorkspace();
            m_InworldEditorStates[EditorStatus.SelectKeySecret] = new InworldEditorSelectKeySecret();
            m_InworldEditorStates[EditorStatus.SelectCharacter] = new InworldEditorSelectCharacter();
            m_InworldEditorStates[EditorStatus.Error] = new InworldEditorError();
        }
        public static string TokenForExchange
        {
            get => Instance.m_StudioToken;
            set => Instance.m_StudioToken = value;
        }
        public GUIStyle BtnStyle
        {
            get
            {
                if (m_BtnStyle != null)
                    return m_BtnStyle;
                m_BtnStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 12,
                    fixedWidth = 100,
                    margin = new RectOffset(10, 10, 0, 0)
                };
                return m_BtnStyle;
            }
        }
    }
}

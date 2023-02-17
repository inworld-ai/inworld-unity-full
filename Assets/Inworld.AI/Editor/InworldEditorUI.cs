/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
#if UNITY_EDITOR
using Inworld.Editor.States;
using Inworld.Util;
using UnityEditor;
using UnityEngine.UIElements;
namespace Inworld.Editor
{
    /// <summary>
    ///     Inworld Editor has 3 parts.
    ///     This part is UI rendering.
    ///     The other parts are for connecting to server, and local data saving.
    /// </summary>
    public partial class InworldEditor
    {
        EditorState m_CurrentState = new EditorPlaying();

        static internal string Title
        {
            set
            {
                Label labelTitle = Instance.rootVisualElement.Q<Label>("Title");
                if (labelTitle != null)
                    labelTitle.text = value;
            }
        }
        static internal string ErrorMessage
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;
                EditorUtility.DisplayDialog("InworldAI", value, "OK");
                Instance.m_CurrentState.OnError();
            }
        }
        static internal VisualElement Root => Instance.rootVisualElement;
        /// <summary>
        ///     Set or get the current Inworld Editor Status.
        ///     The Old Status would call `OnExit()` and the New Status would call `OnEnter()`
        /// </summary>
        public static InworldEditorStatus Status
        {
            get => InworldAI.User.EditorStatus;
            set
            {
                InworldAI.User.EditorStatus = value;
                Instance.m_CurrentState?.OnExit();
                Instance.m_CurrentState = Instance.m_States[InworldAI.User.EditorStatus];
                Instance.m_CurrentState?.OnEnter();
            }
        }
        void Awake()
        {
            InworldAI.File.OnAvatarFailed += LoadDefaultAvatar;
            InworldAI.File.OnThumbnailFailed += LoadDefaultThumbnail;
            InworldAI.User.LoadData();
            Init();
        }
        public void CreateGUI()
        {
            _DrawBanner();
            m_CurrentState?.OnExit(); // YAN: Solve Overlapping Layout on Mac.
            m_CurrentState?.OnEnter();
        }
        void OnInspectorUpdate()
        {
            // Call Repaint on OnInspectorUpdate as it repaints the windows
            // less times as if it was OnGUI/Update
            m_CurrentState?.PostUpdate();
            Repaint();
        }
        void _InitStates()
        {
            m_States[InworldEditorStatus.AppPlaying] = new EditorPlaying();
            m_States[InworldEditorStatus.Default] = new EditorDefault();
            m_States[InworldEditorStatus.Init] = new EditorInit();
            m_States[InworldEditorStatus.WorkspaceChooser] = new EditorWorkspaceChooser();
            m_States[InworldEditorStatus.SceneChooser] = new EditorSceneChooser();
            m_States[InworldEditorStatus.CharacterChooser] = new EditorCharacterChooser();
            m_States[InworldEditorStatus.Error] = new EditorError();
            Status = InworldAI.User.EditorStatus;
        }
    }
}
#endif

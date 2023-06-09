/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using UnityEngine;
using UnityEngine.UIElements;
namespace Inworld.Util
{
    /// <summary>
    ///     This scriptable object stores all the uxml that would be used in display Inworld Studio Panel.
    /// </summary>
    public class InworldUISettings : ScriptableObject
    {
        #region Inspector Variables
        [SerializeField] VisualTreeAsset m_Header;
        [SerializeField] VisualTreeAsset m_WorkspaceChooser;
        [SerializeField] VisualTreeAsset m_SceneChooser;
        [SerializeField] VisualTreeAsset m_CharacterChooser;
        [Space(10)][Header("Playing:")]
        [SerializeField] VisualTreeAsset m_ApplicationPlaying;
        [Space(10)][Header("Default:")]
        [SerializeField] VisualTreeAsset m_DefaultContentPanel;
        [SerializeField] VisualTreeAsset m_DefaultBotPanel;
        [Space(10)][Header("Init:")]
        [SerializeField] VisualTreeAsset m_InputField;
        [SerializeField] VisualTreeAsset m_InitBotPanel;
        [Space(10)][Header("WorkspaceChooser:")]
        [SerializeField] VisualTreeAsset m_WSChooserBot;
        [Space(10)][Header("Connected:")]
        [SerializeField] VisualTreeAsset m_ConnectedBot;
        [Space(10)][Header("Error:")]
        [SerializeField] VisualTreeAsset m_ErrorBot;
        [Space(20)]
        [SerializeField] Vector2 m_MinSize;
        #endregion

        #region Properties
        /// <summary>
        ///     Returns the header (Inworld Logo) page.
        /// </summary>
        public VisualTreeAsset Header => m_Header;
        /// <summary>
        ///     Returns the Inworld Workspace Chooser page.
        /// </summary>
        public VisualTreeAsset WorkspaceChooser => m_WorkspaceChooser;
        /// <summary>
        ///     Returns the Inworld Scene Chooser page.
        /// </summary>
        public VisualTreeAsset SceneChooser => m_SceneChooser;
        /// <summary>
        ///     Returns the Inworld Character chooser page.
        /// </summary>
        public VisualTreeAsset CharacterChooser => m_CharacterChooser;
        /// <summary>
        ///     Returns the Content panel for the default page. (Default Workspace/ Celeste-spaceship and Olympus Theatre)
        /// </summary>
        public VisualTreeAsset DefaultContentPanel => m_DefaultContentPanel;
        /// <summary>
        ///     Returns the button panel for the default page.
        /// </summary>
        public VisualTreeAsset DefaultBotPanel => m_DefaultBotPanel;
        /// <summary>
        ///     Returns the content panel for Init (Inputting token).
        /// </summary>
        public VisualTreeAsset InputField => m_InputField;
        /// <summary>
        ///     Returns the buttons panel for Init.
        /// </summary>
        public VisualTreeAsset InitBotPanel => m_InitBotPanel;
        /// <summary>
        ///     Returns the buttons panel for the workspace chooser page.
        /// </summary>
        public VisualTreeAsset WSChooserBot => m_WSChooserBot;
        /// <summary>
        ///     Returns the "Actual" default page
        ///     (showcasing Application is playing, or data need to be refreshed)
        ///     Because at that time, the State is null.
        /// </summary>
        public VisualTreeAsset ApplicationPlaying => m_ApplicationPlaying;
        /// <summary>
        ///     Returns the buttons panel for "InworldScene Chooser/Character chooser" page.
        /// </summary>
        public VisualTreeAsset ConnectedBotPanel => m_ConnectedBot;
        /// <summary>
        ///     Returns the buttons panel for Error page. (Back to Default page.)
        /// </summary>
        public VisualTreeAsset ErrorBotPanel => m_ErrorBot;
        /// <summary>
        ///     Returns the minimal size of Inworld Studio Panel.
        ///     Otherwise some data could not be displayed.
        /// </summary>
        public Vector2 MinSize => m_MinSize;
        #endregion
    }
}

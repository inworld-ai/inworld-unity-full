/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Studio;
using Inworld.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
namespace Inworld.Editor.States
{
    /// <summary>
    ///     This State triggers when Workspace/Key/Scene/ has all been selected.
    ///     Start Fetching Thumbnails, avatars, and generate Character Buttons.
    /// </summary>
    public class EditorAllCharacterChooser : EditorState
    {
         #region Private Variables
        DropdownField m_KeyChooser;
        DropdownField m_SceneChooser;
        VisualElement m_CharacterChooser;
        Button m_HyperLink;
        Button m_Tutorial;
        VisualElement m_Instruction;
        bool m_IsReconnected;
        bool m_IsWorkspaceInitialized;
        bool m_DataInitialized;
        #endregion

        #region State Functions
        public override void OnEnter()
        {
            InworldEditor.ErrorMessage = "";
            InworldEditor.Title = $"Welcome {InworldAI.User.Name}!\n";
            _SetupContentPanel(InworldAI.UI.CharacterChooser);
            _SetupBotPanel(InworldAI.UI.ConnectedBotPanel);
            
        }

        public override void OnExit()
        {
            m_KeyChooser = null;
            m_SceneChooser = null;
            m_CharacterChooser = null;
            m_Instruction = null;
            m_HyperLink = null;
            m_Tutorial = null;
            base.OnExit();
        }

        void UpdateSceneKeyChoosers(InworldWorkspaceData wsData)
        {
            m_HyperLink.visible = false;
            if (m_KeyChooser != null)
            {
                m_KeyChooser.choices = wsData.integrations.Select(key => key.ShortName).ToList();
                m_KeyChooser.visible = true;
            }
            if (m_SceneChooser != null)
            {
                m_SceneChooser.choices = wsData.scenes.Select(scene => scene.ShortName).ToList();
                m_SceneChooser.visible = true;
            }
        }
        
        public override void PostUpdate()
        {
            InworldWorkspaceData wsData = InworldAI.Game.currentWorkspace;
            if (!wsData)
            {
                return;
            }

            UpdateSceneKeyChoosers(wsData);

            if (InworldEditor.Progress.ContainsKey(wsData))
            {
                if (!m_IsWorkspaceInitialized)
                {
                    float wsProgress = InworldEditor.Progress[wsData].Progress;
                    Debug.Log(("Getting All Characters Progress: " + wsProgress + "%"));
                    if (wsProgress > 95f)
                    {
                        EditorUtility.ClearProgressBar();
                        m_IsWorkspaceInitialized = true;
                        if (m_IsReconnected)
                        {
                            _RefreshData();
                            _LoadingCharacters();
                        }
                    }
                    else
                        EditorUtility.DisplayProgressBar("InworldAI", $"Loading Workspace {wsProgress}% Completed", wsProgress * 0.01f);
                }
            }
        
            if (m_IsReconnected)
                return;
            float downloadingProgress = InworldAI.File.Progress;
            if (downloadingProgress > 99f)
            {
                InworldAI.User.UseCharacterSpecificScenes = true;
                InworldEditor.Instance.ListScenes(wsData);
                InworldEditor.Instance.ListAPIKey(wsData);
                InworldEditor.Instance.ListCharacters(wsData);
                m_IsReconnected = true;
                m_Instruction.visible = true;
                AssetDatabase.Refresh();
                _SetupCharacters();
                EditorUtility.ClearProgressBar();
            }
            else
            {
                m_IsReconnected = false;
                EditorUtility.DisplayProgressBar("InworldAI", $"Loading Characters {downloadingProgress}% Completed", downloadingProgress * 0.01f);
            }
        }
        
        void _CheckProceed()
        {
            if (InworldAI.Game.currentWorkspace && InworldAI.Game.currentKey && InworldAI.Game.currentScene && InworldAI.Game.currentCharacter.brain != InworldAI.Game.currentScene.fullName)
                InworldEditor.Status = InworldEditorStatus.CharacterChooser;
            else
            {
                _LoadingCharacters();

            }
        }
        
        public override void OnConnected()
        {
            m_IsWorkspaceInitialized = false;
            m_DataInitialized = false;
            if (!InworldAI.Game.currentWorkspace)
                return;

            InworldAI.User.UseCharacterSpecificScenes = true;
            InworldWorkspaceData wsData = InworldAI.Game.currentWorkspace;
            InworldEditor.Progress[wsData] = new WorkspaceFetchingProgress();
            InworldEditor.Instance.ListScenes(wsData);
            InworldEditor.Instance.ListAPIKey(wsData);
            InworldEditor.Instance.ListCharacters(wsData);
            m_IsReconnected = true;
        }
        #endregion

        #region UI Functions
        protected override void _SetupContentPanel(VisualTreeAsset contentPanel = null)
        {
            base._SetupContentPanel(contentPanel);
            _SetupContents();
            // Actual what's doing:
            _LoadingCharacters();
        }
        
        protected override void _SetupContents()
        {
            SetupDropDown
            (
                "WorkspaceChooser",
                InworldAI.User.Workspaces.Select(kvp => kvp.Value.title).ToList(),
                OnWorkspaceChanged, InworldAI.Game.currentWorkspace.title
            );

            string targetKey = InworldAI.Game.currentKey ? InworldAI.Game.currentKey.ShortName : null;
            string targetScene = InworldAI.Game.currentScene ? InworldAI.Game.currentScene.ShortName : null;

            if (InworldAI.Game.currentKey == null)
            {
                m_KeyChooser = SetupDropDown("KeyChooser", null, OnKeyChanged, null, false);
            }
            else
            {
                m_KeyChooser = SetupDropDown
                (
                    "KeyChooser", InworldAI.Game.currentWorkspace.integrations.Select(key => key.ShortName).ToList(),
                    OnKeyChanged, targetKey
                );
            }

            // if (InworldAI.Game.currentScene == null)
            //     m_SceneChooser = SetupDropDown("SceneChooser", null, OnSceneChanged, null, false);
            // else
            List<string> scenes = InworldAI.Game.currentWorkspace.scenes.Select(scene => scene.ShortName).ToList();
            scenes.Add(m_IndividualCharacterSceneString);
            m_SceneChooser = SetupDropDown
            (
                "SceneChooser", InworldAI.Game.currentWorkspace.scenes.Select(scene => scene.ShortName).ToList(),
                OnSceneChanged, m_IndividualCharacterSceneString
            );
            
            m_SceneChooser.visible = true;
            m_CharacterChooser = InworldEditor.Root.Q<VisualElement>("CharacterChooser");
            m_HyperLink = SetupButton("HyperLink", () => Help.BrowseURL($"{InworldAI.Game.currentServer.web}/{InworldAI.Game.currentWorkspace.fullName}"), false);
            m_Tutorial = SetupButton("Tutorial", () => Help.BrowseURL($"{InworldAI.Game.currentServer.tutorialPage}/{InworldAI.Game.currentWorkspace.fullName}"), false);
            m_Instruction = InworldEditor.Root.Q<VisualElement>("Instruction");
            m_Instruction.visible = false;
        }
        
        protected override void _SetupBotPanel(VisualTreeAsset botPanel = null)
        {
            base._SetupBotPanel(botPanel);
            _SetupBotContents();
        }
        protected override void _SetupBotContents()
        {
            SetupButton
            (
                "BtnAddCtrl", () =>
                {
                    if (EditorUtility.DisplayDialog("Load Player Controller", k_LoadingPlayerControllerMSG, "OK", "Cancel"))
                        InworldEditor.Instance.LoadPlayerController();
                }
            );
            SetupButton("BtnRefresh", InworldEditor.Instance.Reconnect);
            SetupButton("BtnLogout", () => InworldEditor.Status = InworldEditorStatus.Init);
        }
        #endregion

        #region Callbacks
        void OnWorkspaceChanged(string newValue)
        {
            // 1. Get Correspondent wsData.
            InworldEditor.ErrorMessage = "";
            InworldWorkspaceData wsData = InworldAI.User.Workspaces.Values.FirstOrDefault(data => data.title == newValue);
            if (!wsData)
                return;
            if (wsData.title == InworldAI.Game.currentWorkspace.title)
                return;
            // 2. Show Dialog
            if (EditorUtility.DisplayDialog
            (
                "Change Workspace", k_SwitchWSMSG, "Proceed", "Cancel",
                DialogOptOutDecisionType.ForThisMachine, k_SwitchWSKey
            ))
            {
                InworldAI.Game.CurrentWorkspace = wsData;
                InworldEditor.Status = InworldEditorStatus.WorkspaceChooser;
            }
        }
        void OnKeyChanged(string newValue)
        {
            if (InworldAI.Game.currentKey && InworldAI.Game.currentKey.ShortName == newValue)
                return;
            
            InworldAI.Game.currentKey = InworldAI.Game.currentWorkspace.integrations.FirstOrDefault(key => key.ShortName == newValue);
        }
        void OnSceneChanged(string newValue)
        {
            if (InworldAI.Game.currentScene && InworldAI.Game.currentScene.fullName == newValue)
                return;
            // 2. Show Dialog
            if (EditorUtility.DisplayDialog
            (
                "Change Inworld Scene", k_SwitchSceneMSG, "Proceed", "Cancel",
                DialogOptOutDecisionType.ForThisMachine, k_SwitchSceneKey
            ))
            {
                InworldAI.Game.currentScene = InworldAI.Game.currentWorkspace.scenes.FirstOrDefault(scene => scene.ShortName == newValue);
                _CheckProceed();
            }
        }
        #endregion

        #region Private Functions
        void _ClearCharacterChoosers()
        {
            while (m_CharacterChooser != null && m_CharacterChooser.childCount > 0)
            {
                m_CharacterChooser.RemoveAt(0);
            }
        }
        void _SetupCharacters()
        {
            // 1. Clear Data.
            _ClearCharacterChoosers();
            InworldEditor.SetupInworldController();
            foreach (InworldCharacterData charData in InworldAI.Game.currentWorkspace.characters)
            {
                // 3. Create Buttons.
                Button btnCharacter = CreateCharacterButton(charData);
                btnCharacter.clickable.clicked += () =>
                {
                    Object character = AssetDatabase.LoadAssetAtPath<Object>(charData.LocalAvatarFileName);
                    Selection.activeObject = character;
                    InworldController.CurrentScene = InworldAI.Game.currentScene = InworldAI.Game.currentWorkspace.scenes.Find(scene => scene.fullName == charData.brain);
                    InworldAI.Game.currentCharacter = charData;
                    EditorUtility.FocusProjectWindow();
                };
                m_CharacterChooser.Add(btnCharacter);
            }
            Debug.Log(("Should have setup the chars"));

        }
        void _RefreshData()
        {
            if (InworldAI.Game.currentScene != null)
            {
                InworldSceneData iwSceneData = InworldAI.Game.currentWorkspace.scenes.FirstOrDefault
                    (sceneData => sceneData.fullName == InworldAI.Game.currentScene.fullName);
                if (iwSceneData)
                {
                    InworldAI.Game.currentScene.CopyFrom(iwSceneData);
                }
            }

            if (InworldAI.Game.currentKey != null)
            {
                InworldKeySecret iwKeySecret = InworldAI.Game.currentWorkspace.integrations.FirstOrDefault
                    (keySecret => keySecret.key == InworldAI.Game.currentKey.key);
                if (iwKeySecret)
                {
                    InworldAI.Game.currentKey.secret = iwKeySecret.secret;
                }
            }
        }
        void _LoadingCharacters()
        {
            bool isValid = InworldEditor.IsDataValid;
            m_IsReconnected = false;
            if (isValid)
            {
                m_HyperLink.visible = false;
                InworldEditor._SaveCurrentSettings();
                InworldAI.File.Init();
                foreach (InworldCharacterData charData in InworldAI.Game.currentWorkspace.characters.Where
                    (charData => charData.Progress < 0.95f))
                {
                    InworldAI.File.DownloadCharacterData(charData);
                }
            }
            else
            {
                m_Instruction.visible = false;
                m_HyperLink.visible = true;
                m_Tutorial.visible = true;
            }
        }
        #endregion
    }
}
#endif

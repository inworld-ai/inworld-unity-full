/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
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
    ///     This is the default page, before login.
    ///     Developers could always try to use the default inworld scenes and characters.
    /// </summary>
    public class EditorDefault : EditorState
    {
        #region Private Variables
        InworldWorkspaceData[] m_Workspaces;
        VisualElement m_CharacterChooser;
        VisualElement m_Instruction;
        Button m_BtnApply;
        Label m_TryLogin;
        bool m_CharacterInitialized;
        #endregion

        #region State Functions
        public override void OnEnter()
        {
            InworldEditor.Title = "Welcome to Inworld.AI!\n";
            m_Workspaces = Resources.LoadAll<InworldWorkspaceData>(InworldAI.Settings.WorkspaceDataPath);
            _SetupContentPanel(InworldAI.UI.DefaultContentPanel);
            _SetupBotPanel(InworldAI.UI.DefaultBotPanel);
        }
        public override void OnExit()
        {
            m_BtnApply = null;
            m_CharacterChooser = null;
            m_TryLogin = null;
            m_Instruction = null;
            base.OnExit();
        }
        public override void PostUpdate()
        {
            if (!InworldEditor.IsDataValid)
            {
                m_Instruction.visible = false;
                _ClearCharacterChoosers();
                return;
            }
            float downloadingProgress = InworldAI.File.Progress;
            if (m_CharacterInitialized)
                return;
            if (downloadingProgress > 99f)
            {
                m_Instruction.visible = true;
                AssetDatabase.Refresh();
                _SetupCharacters();
                m_CharacterInitialized = true;
                EditorUtility.ClearProgressBar();
            }
            else
            {
                EditorUtility.DisplayProgressBar("InworldAI", $"Downloading Characters {downloadingProgress}% Completed", downloadingProgress * 0.01f);
            }
        }
        #endregion

        #region UI Functions
        protected override void _SetupContentPanel(VisualTreeAsset contentPanel = null)
        {
            if (m_Workspaces.Length != 1)
            {
                InworldEditor.ErrorMessage = "Data Error.\nPlease make sure there's only 1 default workspace in the Resources folder.";
                InworldEditor.Status = InworldEditorStatus.Error;
                return;
            }
            _SetupDefaultData();
            base._SetupContentPanel(contentPanel);
            _SetupContents();
        }
        protected override void _SetupContents()
        {
            DropdownField sceneChooser = SetupDropDown("SceneChooser");
            List<string> listScenes = InworldAI.Game.currentWorkspace.scenes.Select(scene => scene.ShortName).ToList();
            ActivateDropDown(ref sceneChooser, listScenes, OnSceneChanged);

            m_CharacterChooser = InworldEditor.Root.Q<VisualElement>("CharacterChooser");
            m_Instruction = InworldEditor.Root.Q<VisualElement>("Instruction");
            m_Instruction.visible = false;
        }
        protected override void _SetupBotPanel(VisualTreeAsset botPanel = null)
        {
            InworldSceneData sceneData = InworldAI.Game.currentWorkspace.scenes.Contains(InworldAI.Game.currentScene) ? InworldAI.Game.currentScene : m_Workspaces[0].scenes[0];
            InworldEditor.Root.Q<DropdownField>("SceneChooser").value = sceneData.ShortName;
            base._SetupBotPanel(botPanel);
            _SetupBotContents();
        }
        protected override void _SetupBotContents()
        {
            m_TryLogin = SetupLabel("TxtTryLogin");
            SetupButton
            (
                "BtnAddCtrl", () =>
                {
                    if (EditorUtility.DisplayDialog("Load Player Controller", "Adding player controller would delete current main camera.\nContinue?", "OK", "Cancel"))
                        InworldEditor.Instance.LoadPlayerController();
                }
            );
            SetupButton
            (
                "BtnLogin", () =>
                {
                    InworldEditor.Status = string.IsNullOrEmpty(InworldAI.User.RefreshToken) ? InworldEditorStatus.Init : InworldEditorStatus.WorkspaceChooser;
                }
            );
        }
        #endregion

        #region Private Functions
        void OnSceneChanged(string newValue)
        {
            InworldAI.Game.currentScene = InworldAI.Game.currentWorkspace.scenes.FirstOrDefault(scene => scene.ShortName == newValue);
            _CheckValid();
        }
        void _SetupDefaultData()
        {
            InworldAI.Game.currentWorkspace = m_Workspaces[0];
            InworldAI.Game.currentKey = InworldAI.Game.currentWorkspace.integrations[0];
        }
        void _ClearCharacterChoosers()
        {
            while (m_CharacterChooser.childCount > 0)
            {
                m_CharacterChooser.RemoveAt(0);
            }
        }
        void _SetupCharacters()
        {
            // 1. Clear Data.
            _ClearCharacterChoosers();
            foreach (string brain in InworldAI.Game.currentScene.characters)
            {
                InworldCharacterData charData = InworldAI.Game.currentWorkspace.characters.FirstOrDefault(charData => charData.brain == brain);
                if (!charData)
                    continue;
                Button btnCharacter = CreateCharacterButton(charData);
                btnCharacter.clickable.clicked += () =>
                {
                    Selection.activeObject = charData.Avatar;
                    EditorUtility.FocusProjectWindow();
                };
                m_CharacterChooser.Add(btnCharacter);
            }
        }
        void _CheckValid()
        {
            bool isValid = InworldEditor.IsDataValid;
            if (m_BtnApply != null)
                m_BtnApply.visible = isValid;
            if (m_TryLogin != null)
                m_TryLogin.visible = isValid;

            if (!isValid)
                return;
            m_CharacterInitialized = false;
            InworldAI.File.Init();
            foreach (string brain in InworldAI.Game.currentScene.characters.Where
                (brain => InworldAI.User.Characters.ContainsKey(brain) && InworldAI.User.Characters[brain].Progress < 0.95f))
            {
                InworldAI.File.DownloadCharacterData(InworldAI.User.Characters[brain]);
            }
        }
        #endregion
    }
}
#endif

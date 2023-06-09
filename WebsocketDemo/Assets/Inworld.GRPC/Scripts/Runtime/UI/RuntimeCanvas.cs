/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Inworld.Studio;
using Inworld.Util;
using UnityEngine;
namespace Inworld.Runtime
{
    public class RuntimeCanvas : SingletonBehavior<RuntimeCanvas>
    {
        [SerializeField] protected CharacterChooserPanel m_CharChooserPanel;
        [SerializeField] protected LoadingPanel m_LoadingPanel;
        [SerializeField] protected LoginPanel m_LoginPanel;
        [SerializeField] RuntimeFileDownloader m_FileDownloader;
        InworldController m_Controller;
        [SerializeField] GameObject m_UICamera;
        [SerializeField] GameObject m_InworldPlayer;
        public static RuntimeFileDownloader File => Instance.m_FileDownloader;
        
        public string Log
        {
            set
            {
                m_LoadingPanel.gameObject.SetActive(true);
                m_LoadingPanel.ShowLog(value);
            }
        }
        void Start()
        {
            Init();
        }
        protected virtual void Init()
        {
            AddListener();
            m_CharChooserPanel.gameObject.SetActive(false);
            m_LoadingPanel.gameObject.SetActive(false);
            m_LoginPanel.gameObject.SetActive(true);
        }
        protected void AddListener() => RuntimeInworldStudio.Event.AddListener(OnStudioStatusChanged);

        public void Back()
        {
            m_LoadingPanel.gameObject.SetActive(false);
        }
        public void Error(string strTitle, string strContent)
        {
            m_LoadingPanel.gameObject.SetActive(true);
            m_LoadingPanel.ShowError(strTitle, strContent);
        }
        public void Loading(string strProgress)
        {
            m_LoadingPanel.gameObject.SetActive(true);
            m_LoadingPanel.ShowWait(strProgress);
        }

        void _ProcessLoading(string msg)
        {
            Loading(msg);
            if (RuntimeInworldStudio.Instance.Progress > 95 && m_LoadingPanel.gameObject.activeSelf)
            {
                // Start Switching page.
                m_LoadingPanel.gameObject.SetActive(false);
                m_CharChooserPanel.EstablishWSList();
            }
        }
        void OnStudioStatusChanged(StudioStatus incomingStatus, string msg)
        {
            switch (incomingStatus)
            {
                case StudioStatus.Initialized:
                    m_CharChooserPanel.gameObject.SetActive(true);
                    Loading("0");
                    m_CharChooserPanel.Clear();
                    RuntimeInworldStudio.Instance.ListWorkspace();
                    break;
                case StudioStatus.ListWorkspaceCompleted:
                case StudioStatus.ListSceneCompleted:
                case StudioStatus.ListCharacterCompleted:
                case StudioStatus.ListKeyCompleted:
                    _ProcessLoading(msg);
                    break;
                case StudioStatus.ListACharacter:
                    // Figure Thumbnails & Avatars
                    if (!InworldAI.User.Characters.ContainsKey(msg))
                        return;
                    m_CharChooserPanel.Spawn(InworldAI.User.Characters[msg]);
                    break;
                case StudioStatus.ListAKey:
                    break;
                case StudioStatus.ListSharedCharacterCompleted:
                    break;
                case StudioStatus.ListSharedCharacterFailed:
                    Error("List Shared Character Failed", msg);
                    break;
                case StudioStatus.InitFailed:
                    Error("Init Failed", msg);
                    break;
                case StudioStatus.ListWorkspaceFailed:
                    Error("List Workspace Failed", msg);
                    break;
                case StudioStatus.ListSceneFailed:
                    Error("List Scene Failed", msg);
                    break;
                case StudioStatus.ListCharacterFailed:
                    Error("List Character Failed", msg);
                    break;
                case StudioStatus.ListKeyFailed:
                    Error("List Key Failed", msg);
                    break;
            }
        }
        public InworldCharacter BindCharacter(InworldCharacterData data)
        {
            if (!m_Controller)
                m_Controller = Instantiate(InworldAI.ControllerPrefab);
            InworldController.Player = m_InworldPlayer;
            InworldCharacter result = Instantiate(InworldAI.CharacterPrefab, m_Controller.transform);
            result.LoadCharacter(data);
            return result;
        }
        public virtual void GotoCharacter()
        {
            m_UICamera.SetActive(false);
            m_InworldPlayer.SetActive(true);
            gameObject.SetActive(false);
        }
        public virtual void BackToLobby()
        {
            RemoveController();
            m_InworldPlayer.SetActive(false);
            m_UICamera.SetActive(true);
        }
        protected void RemoveController()
        {
            if (!InworldController.Instance)
                return;
            Debug.Log("Destroy it");
            Destroy(InworldController.Instance.gameObject);
            m_Controller = null;
        }
    }
}

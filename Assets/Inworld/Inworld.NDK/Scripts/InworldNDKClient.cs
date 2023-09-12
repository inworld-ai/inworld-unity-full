using System.Collections.Generic;


namespace Inworld.NDK
{
    public class InworldNDKClient : InworldClient
    {
        public List<AgentInfo> AgentList { get; } = new List<AgentInfo>();

        InworldConnectionStatus m_LastStatus, m_CurrentStatus;
        public override InworldConnectionStatus Status
        {
            get => m_CurrentStatus;
            set
            {
                m_LastStatus = m_CurrentStatus;
                m_CurrentStatus = value;
            }
        }
        
        void OnDisable() => NDKInterop.Unity_EndSession();

        void Update()
        {
            _ProcessStatusChange();
        }


        void OnDestroy() => NDKInterop.Unity_DestroyWrapper();
        
        protected override void Init()
        {
            base.Init();
            InworldNDK.API.Init();
        }
        public override void Disconnect() => NDKInterop.Unity_EndSession();

        public override void GetAccessToken()
        {
            Status = InworldConnectionStatus.Initializing;
            string responseJson = m_CustomToken;
            if (!string.IsNullOrEmpty(responseJson))
                return;
            if (string.IsNullOrEmpty(m_APIKey))
            {
                Error = "Please fill API Key!";
                return;
            }
            if (string.IsNullOrEmpty(m_APISecret))
            {
                Error = "Please fill API Secret!";
                return;
            }
            InworldNDK.API.GetAccessToken(m_ServerConfig.RuntimeServer, m_APIKey, m_APISecret);
        }
        public override void LoadScene(string sceneFullName) => InworldNDK.API.LoadScene(sceneFullName);

        public override LoadSceneResponse GetLiveSessionInfo() => InworldNDK.From.NDKLoadSceneResponse(AgentList);
        
        public override void StartSession()
        {
            if (!IsTokenValid)
                return;
            Status = InworldConnectionStatus.Connecting;
            NDKInterop.Unity_StartSession();
            Status = InworldConnectionStatus.Connected;
        }

        public override void SendText(string characterID, string textToSend)
        {
            if (string.IsNullOrEmpty(characterID) || string.IsNullOrEmpty(textToSend))
                return;
            Dispatch(InworldNDK.To.TextPacket(characterID, textToSend));
            NDKInterop.Unity_SendText(characterID, textToSend);
        }

        public override void SendCancelEvent(string characterID, string interactionID)
        {
            if (string.IsNullOrEmpty(characterID))
                return;
            NDKInterop.Unity_CancelResponse(characterID, interactionID);
        }

        public override void SendTrigger(string charID, string triggerName, Dictionary<string, string> parameters)
        {
            if (string.IsNullOrEmpty(charID))
                return;
            if (parameters.Count == 0)
                NDKInterop.Unity_SendTrigger(charID, triggerName);
            else
            {
                foreach (KeyValuePair<string, string> data in parameters)
                {
                    NDKInterop.Unity_SendTriggerParam(charID, triggerName, data.Key, data.Value);
                }
            }
        }
        public override void StartAudio(string charID)
        {
            if (string.IsNullOrEmpty(charID))
                return;
            NDKInterop.Unity_StartAudio(charID);
            if (!m_AudioCapture.IsCapturing)
                m_AudioCapture.StartRecording();
        }
        public override void StopAudio(string charID)
        {
            if (string.IsNullOrEmpty(charID))
                return;
            NDKInterop.Unity_StopAudio(charID);
            if (m_AudioCapture.IsCapturing)
                m_AudioCapture.StopRecording();
        }
        public override void SendAudio(string charID, string base64)
        {
            if (string.IsNullOrEmpty(charID) || string.IsNullOrEmpty(base64))
                return;
            NDKInterop.Unity_SendAudio(charID, base64);
        }
        
        void _ProcessStatusChange()
        {
            if (m_CurrentStatus == m_LastStatus)
                return;
            ChangeStatus(m_CurrentStatus);
            m_LastStatus = m_CurrentStatus;
        }
    }
}

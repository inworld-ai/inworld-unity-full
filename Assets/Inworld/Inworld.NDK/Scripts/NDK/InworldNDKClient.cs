using Inworld.Packet;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;


namespace Inworld.NDK
{
    public class InworldNDKClient : InworldClient
    {
        public List<AgentInfo> AgentList { get; } = new List<AgentInfo>();
        protected ConcurrentQueue<InworldPacket> m_IncomingQueue = new ConcurrentQueue<InworldPacket>();

        protected InworldConnectionStatus m_LastStatus, m_CurrentStatus;
        public override InworldConnectionStatus Status
        {
            get => m_CurrentStatus;
            set
            {
                m_LastStatus = m_CurrentStatus;
                m_CurrentStatus = value;
            }
        }

        protected virtual void OnDisable() => NDKInterop.Unity_EndSession();
        
        void Update()
        {
            _ProcessStatusChange();
            _ProcessPackage();
        }


        protected virtual void OnDestroy() => NDKInterop.Unity_DestroyWrapper();
        
        protected override void Init()
        {
            InworldNDKAPI.Init();
        }
        public override void Disconnect()
        {
            NDKInterop.Unity_EndSession();
        }

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
            InworldNDKAPI.GetAccessToken(m_ServerConfig.RuntimeServer, m_APIKey, m_APISecret);
        }
        public override void LoadScene(string sceneFullName) => InworldNDKAPI.LoadScene(sceneFullName);

        public override LoadSceneResponse GetLiveSessionInfo() => InworldNDK.From.NDKLoadSceneResponse(AgentList);
        
        public override void StartSession() => StartCoroutine(_StartSession());

        protected IEnumerator _StartSession()
        {
            if (!IsTokenValid)
                yield break;
            yield return new WaitForEndOfFrame();
            Status = InworldConnectionStatus.Connecting;
            yield return new WaitForEndOfFrame();
            NDKInterop.Unity_StartSession();
            yield return new WaitForEndOfFrame();
            Status = InworldConnectionStatus.Connected;
            yield return new WaitForEndOfFrame();
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
            if (parameters == null || parameters.Count == 0)
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
        }
        public override void StopAudio(string charID)
        {
            if (string.IsNullOrEmpty(charID))
                return;
            NDKInterop.Unity_StopAudio(charID);
        }
        public override void SendAudio(string charID, string base64)
        {
            if (string.IsNullOrEmpty(charID) || string.IsNullOrEmpty(base64))
                return;
            NDKInterop.Unity_SendAudio(charID, base64);
        }
        
        protected void _ProcessStatusChange()
        {
            if (m_CurrentStatus == m_LastStatus)
                return;
            ChangeStatus(m_CurrentStatus);
            m_LastStatus = m_CurrentStatus;
        }
        protected void _ProcessPackage()
        {
            if (m_IncomingQueue.Count == 0)
                return;
            if (m_IncomingQueue.TryDequeue(out InworldPacket packet))
                Dispatch(packet);
        }
        public void Enqueue(InworldPacket packet)
        {
            m_IncomingQueue.Enqueue(packet);
        }
    }
}


    using TMPro;
    using UnityEngine;

    namespace Inworld.Sample
    {
        public class NetworkLogCanvas : SingletonBehavior<NetworkLogCanvas>
        {
            [SerializeField] protected TMP_Text m_Title;
            [SerializeField] protected TMP_Text m_Content;
            // Start is called before the first frame update

            string clientSend;
            string clientRecv;
            string svrSend;
            string svrRecv;
            
            public void ClientSend(float x, float y, float z, float yall, float pitch, float raw)
            {
                clientSend = $"Client Send: {x:F2} {y:F2} {z:F2} {yall:F2} {pitch:F2} {raw:F2}\n";
                m_Content.text = $"{clientSend}{clientRecv}{svrSend}{svrRecv}";
            }
            public void ClientRecv(float x, float y, float z, float yall, float pitch, float raw)
            {
                clientRecv = $"Client Recv: {x:F2} {y:F2} {z:F2} {yall:F2} {pitch:F2} {raw:F2}\n";
                m_Content.text = $"{clientSend}{clientRecv}{svrSend}{svrRecv}";
            }
            public void ServerSend(float x, float y, float z, float yall, float pitch, float raw)
            {
                svrSend = $"Svr Send: {x:F2} {y:F2} {z:F2} {yall:F2} {pitch:F2} {raw:F2}\n";
                m_Content.text = $"{clientSend}{clientRecv}{svrSend}{svrRecv}";
            }
            public void ServerRecv(float x, float y, float z, float yall, float pitch, float raw)
            {
                svrRecv = $"Svr Recv: {x:F2} {y:F2} {z:F2} {yall:F2} {pitch:F2} {raw:F2}\n";
                m_Content.text = $"{clientSend}{clientRecv}{svrSend}{svrRecv}";
            }
        }
    }


#if !NET_LEGACY && (UNITY_EDITOR || !UNITY_WEBGL) && !UNITY_WEB_SOCKET_ENABLE_ASYNC
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityWebSocket
{
    [DefaultExecutionOrder(-10000)]
    public class WebSocketManager : MonoBehaviour
    {
        const string rootName = "[UnityWebSocket]";
        static WebSocketManager _instance;
        public static WebSocketManager Instance
        {
            get
            {
                if (!_instance) 
                    CreateInstance();
                return _instance;
            }
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public static void CreateInstance()
        {
            GameObject go = GameObject.Find("/" + rootName);
            if (!go) go = new GameObject(rootName);
            _instance = go.GetComponent<WebSocketManager>();
            if (!_instance) 
                _instance = go.AddComponent<WebSocketManager>();
        }

        readonly List<WebSocket> sockets = new List<WebSocket>();

        public bool Contains(string sessionURL) => sockets.Any(s => s.Address == sessionURL);
        public static WebSocket GetWebSocket(string sessionURL) => Instance.sockets.FirstOrDefault(s => s.Address == sessionURL);
        public void Add(WebSocket socket)
        {
            if (!Contains(socket.Address))
                sockets.Add(socket);
        }

        public void Remove(WebSocket socket)
        {
            if (sockets.Contains(socket))
                sockets.Remove(socket);
        }

        void Update()
        {
            if (sockets.Count <= 0) return;
            for (int i = sockets.Count - 1; i >= 0; i--)
                sockets[i].Update();
        }

    }
}
#endif
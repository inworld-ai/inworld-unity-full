#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using System.Linq;

namespace Inworld
{
    /// <summary>
    /// Class providing static access methods to work with JSLIB WebSocket
    /// </summary>
    public static class WebSocketManager
    {
        /* Map of websocket instances */
        private static Dictionary<int, WebSocket> sockets = new Dictionary<int, WebSocket>();

        /* Delegates */
        public delegate void OnOpenCallback(int instanceId);
        public delegate void OnMessageCallback(int instanceId, IntPtr msgPtr, int msgSize);
        public delegate void OnMessageStrCallback(int instanceId, IntPtr msgStrPtr);
        public delegate void OnErrorCallback(int instanceId, IntPtr errorPtr);
        public delegate void OnCloseCallback(int instanceId, int closeCode, IntPtr reasonPtr);

        public static WebSocket GetWebSocket(string sessionURL) => sockets.Values.FirstOrDefault(s => s.Address == sessionURL);

        /* WebSocket JSLIB functions */
        [DllImport("__Internal")]
        public static extern int InworldWebSocketConnect(int instanceId);

        [DllImport("__Internal")]
        public static extern int InworldWebSocketClose(int instanceId, int code, string reason);

        [DllImport("__Internal")]
        public static extern int InworldWebSocketSend(int instanceId, byte[] dataPtr, int dataLength);

        [DllImport("__Internal")]
        public static extern int InworldWebSocketSendStr(int instanceId, string data);

        [DllImport("__Internal")]
        public static extern int InworldWebSocketGetState(int instanceId);

        /* WebSocket JSLIB callback setters and other functions */
        [DllImport("__Internal")]
        public static extern int InworldWebSocketAllocate(string url, string binaryType);

        [DllImport("__Internal")]
        public static extern int InworldWebSocketAddSubProtocol(int instanceId, string protocol);

        [DllImport("__Internal")]
        public static extern void InworldWebSocketFree(int instanceId);

        [DllImport("__Internal")]
        public static extern void InworldWebSocketSetOnOpen(OnOpenCallback callback);

        [DllImport("__Internal")]
        public static extern void InworldWebSocketSetOnMessage(OnMessageCallback callback);

        [DllImport("__Internal")]
        public static extern void InworldWebSocketSetOnMessageStr(OnMessageStrCallback callback);

        [DllImport("__Internal")]
        public static extern void InworldWebSocketSetOnError(OnErrorCallback callback);

        [DllImport("__Internal")]
        public static extern void InworldWebSocketSetOnClose(OnCloseCallback callback);

        /* If callbacks was initialized and set */
        private static bool isInitialized = false;

        /* Initialize WebSocket callbacks to JSLIB */
        private static void Initialize()
        {
            InworldWebSocketSetOnOpen(DelegateOnOpenEvent);
            InworldWebSocketSetOnMessage(DelegateOnMessageEvent);
            InworldWebSocketSetOnMessageStr(DelegateOnMessageStrEvent);
            InworldWebSocketSetOnError(DelegateOnErrorEvent);
            InworldWebSocketSetOnClose(DelegateOnCloseEvent);

            isInitialized = true;
        }

        [MonoPInvokeCallback(typeof(OnOpenCallback))]
        public static void DelegateOnOpenEvent(int instanceId)
        {
            if (sockets.TryGetValue(instanceId, out var socket))
            {
                socket.HandleOnOpen();
            }
        }

        [MonoPInvokeCallback(typeof(OnMessageCallback))]
        public static void DelegateOnMessageEvent(int instanceId, IntPtr msgPtr, int msgSize)
        {
            if (sockets.TryGetValue(instanceId, out var socket))
            {
                var bytes = new byte[msgSize];
                Marshal.Copy(msgPtr, bytes, 0, msgSize);
                socket.HandleOnMessage(bytes);
            }
        }

        [MonoPInvokeCallback(typeof(OnMessageCallback))]
        public static void DelegateOnMessageStrEvent(int instanceId, IntPtr msgStrPtr)
        {
            if (sockets.TryGetValue(instanceId, out var socket))
            {
                string msgStr = Marshal.PtrToStringAuto(msgStrPtr);
                socket.HandleOnMessageStr(msgStr);
            }
        }

        [MonoPInvokeCallback(typeof(OnErrorCallback))]
        public static void DelegateOnErrorEvent(int instanceId, IntPtr errorPtr)
        {
            if (sockets.TryGetValue(instanceId, out var socket))
            {
                string errorMsg = Marshal.PtrToStringAuto(errorPtr);
                socket.HandleOnError(errorMsg);
            }
        }

        [MonoPInvokeCallback(typeof(OnCloseCallback))]
        public static void DelegateOnCloseEvent(int instanceId, int closeCode, IntPtr reasonPtr)
        {
            if (sockets.TryGetValue(instanceId, out var socket))
            {
                string reason = Marshal.PtrToStringAuto(reasonPtr);
                socket.HandleOnClose((ushort)closeCode, reason);
            }
        }

        internal static int AllocateInstance(string address, string binaryType)
        {
            if (!isInitialized) Initialize();
            return InworldWebSocketAllocate(address, binaryType);
        }

        internal static void Add(WebSocket socket)
        {
            if (!sockets.ContainsKey(socket.instanceId))
            {
                sockets.Add(socket.instanceId, socket);
            }
        }

        internal static void Remove(int instanceId)
        {
            if (sockets.ContainsKey(instanceId))
            {
                sockets.Remove(instanceId);
            }
        }
    }
}
#endif

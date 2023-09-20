using System;
using System.Runtime.InteropServices;


namespace Inworld.NDK
{
    public class NDKInterop
    {
        const string DLL_NAME = "InworldUnityWrapper";

#region Life Cycle
        [DllImport(DLL_NAME)]
        public static extern IntPtr Unity_InitWrapper();
        [DllImport(DLL_NAME)]
        public static extern void Unity_DestroyWrapper();
#endregion

#region Inworld Status
        [DllImport(DLL_NAME)]
        public static extern void Unity_GetAccessToken(string serverURL, string apiKey, string apiSecret, NDKCallback callback);
        [DllImport(DLL_NAME)]
        public static extern void Unity_LoadScene(string sceneName, NDKCallback callback);
        [DllImport(DLL_NAME)]
        public static extern void Unity_StartSession();
        [DllImport(DLL_NAME)]
        public static extern void Unity_EndSession();
#endregion
        
#region Send Data
        [DllImport(DLL_NAME)]
        public static extern void Unity_SendText(string agentID, string message);
        [DllImport(DLL_NAME)]
        public static extern void Unity_SendAudio(string agentID, string data);
        [DllImport(DLL_NAME)]
        public static extern void Unity_SendTrigger(string agentID, string triggerName);
        [DllImport(DLL_NAME)]
        public static extern void Unity_SendTriggerParam(string agentID, string triggerName, string param, string paramValue);
        [DllImport(DLL_NAME)]
        public static extern void Unity_CancelResponse(string agentID, string interactionIDToCancel);
        [DllImport(DLL_NAME)]
        public static extern void Unity_StartAudio(string agentID);
        [DllImport(DLL_NAME)]
        public static extern void Unity_StopAudio(string agentID);
#endregion Send Data

#region Setter
        [DllImport(DLL_NAME)]
        public static extern void Unity_SetLogger(NDKLogCallBack callback);
        
        [DllImport(DLL_NAME)]
        public static extern void Unity_SetPacketCallback(
            TextCallBack textCallBack,
            AudioCallBack audioCallBack,
            ControlCallBack controlCallBack,
            EmotionCallBack emotionCallBack,
            CancelResponseCallBack cancelResponseCallBack,
            TriggerCallBack customCallBack,
            PhonemeCallBack phonemeCallBack,
            TriggerParamCallBack triggerParamCallBack);
        
        [DllImport(DLL_NAME)]
        public static extern void Unity_SetClientRequest(string strClientID, string strClientVersion);
        
        [DllImport(DLL_NAME)]
        public static extern void Unity_SetUserRequest(string strUserName, string  strPlayerID);
        
        [DllImport(DLL_NAME)]
        public static extern void Unity_AddUserProfile(string strProfileID, string strProfileValue);
        
        [DllImport(DLL_NAME)]
        public static extern void Unity_SetCapabilities(ref Capabilities capabilities);
#endregion

#region Getter
        [DllImport(DLL_NAME)]
        public static extern AgentInfo Unity_GetAgentInfo(int nIndex);
        
        [DllImport(DLL_NAME)]
        public static extern int Unity_GetAgentCount();
        
        [DllImport(DLL_NAME)]
        public static extern SessionInfo Unity_GetSessionInfo();        
#endregion        
    }
}

using UnityEngine.Device;


namespace Inworld.NDK
{
    public static class InworldNDKAPI
    {
        public static void GetAccessToken(string serverURL, string apiKey, string apiSecret)
        {
            NDKInterop.Unity_GetAccessToken(serverURL, apiKey, apiSecret, InworldNDKCallBack.OnTokenGenerated);
        }
        
        public static void LoadScene(string sceneFullName)
        {
            Inworld.Capabilities capabilities = InworldAI.Capabilities;
            Capabilities cap = new Capabilities
            {
                Text  = capabilities.text,
                Audio = capabilities.audio,
                Emotions = capabilities.emotions,
                Interruptions  = capabilities.interruptions,
                Triggers  = capabilities.triggers,
                PhonemeInfo = capabilities.text,
                TurnBasedSTT  = capabilities.turnBasedStt,
                NarratedActions = capabilities.narratedActions,
            };
            NDKInterop.Unity_SetCapabilities(ref cap);
            if (string.IsNullOrEmpty(InworldAI.User.ID))
                InworldAI.User.ID = SystemInfo.deviceUniqueIdentifier;
            NDKInterop.Unity_SetUserRequest(InworldAI.User.Name, InworldAI.User.ID);
            foreach (InworldPlayerProfile profile in InworldAI.User.PlayerProfiles)
            {
                NDKInterop.Unity_AddUserProfile(profile.property, profile.value);
            }
            NDKInterop.Unity_SetClientRequest("Unity NDK", InworldAI.Version);
            NDKInterop.Unity_LoadScene(sceneFullName, InworldNDKCallBack.OnSceneLoaded);
        }
        
        public static void Init()
        {
            InworldAI.Log("[NDK] Start Init");
            NDKInterop.Unity_InitWrapper();
            InworldAI.Log("[NDK] Start Set Logger");
            NDKInterop.Unity_SetLogger(InworldNDKCallBack.OnLogReceived);
            InworldAI.Log("[NDK] Start Set Back");
            NDKInterop.Unity_SetPacketCallback(InworldNDKCallBack.OnNDKPacketReceived,
                                               InworldNDKCallBack.OnPhonemeReceived, 
                                               InworldNDKCallBack.OnTriggerParamReceived);
        }
    }
}

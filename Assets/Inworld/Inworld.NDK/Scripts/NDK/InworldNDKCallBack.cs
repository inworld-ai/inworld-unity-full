namespace Inworld.NDK
{
    public static class InworldNDKCallBack
    {
        static ProcessingAudioChunk s_ProcessingAudioChunk = new ProcessingAudioChunk();
        static internal void OnTokenGenerated()
        {
            SessionInfo sessionInfo = NDKInterop.Unity_GetSessionInfo();
            InworldController.Client.Token = InworldNDK.From.NDKToken(sessionInfo);
            InworldAI.Log("Get Session ID: " + sessionInfo.sessionId);
            InworldController.Client.Status = InworldConnectionStatus.Initialized;
        }
        static internal void OnSceneLoaded()
        {
            InworldAI.Log("[NDK] Scene loaded");
            if (InworldController.Client is not InworldNDKClient ndkClient)
                return;
            int nAgentSize = NDKInterop.Unity_GetAgentCount();
            ndkClient.AgentList.Clear();
            for (int i = 0; i < nAgentSize; i++)
            {
                ndkClient.AgentList.Add(NDKInterop.Unity_GetAgentInfo(i));
            }
            InworldController.Client.Status = InworldConnectionStatus.LoadingSceneCompleted;
        }
        
        static internal void OnLogReceived(string log, int severity)
        {
            switch (severity)
            {
                case 1:
                    InworldAI.LogWarning($"[NDK]: {log}");
                    break;
                case 2:
                    InworldAI.LogError($"[NDK]: {log}");
                    break;
                case 3:
                    InworldAI.LogException($"[NDK]: {log}");
                    break;
                default:
                    InworldAI.Log($"[NDK]: {log}");
                    break;
            }
        }
        
        static internal void OnNDKPacketReceived(NDKPacket packet)
        {
            if (InworldController.Client is not InworldNDKClient ndkClient)
                return;
            switch (packet.packetType)
            {
                case "TEXT":
                    ndkClient.Enqueue(InworldNDK.From.NDKTextPacket(packet));
                    break;
                case "AUDIO":
                    s_ProcessingAudioChunk.ToInworldPacket();
                    s_ProcessingAudioChunk = new ProcessingAudioChunk(packet);
                    break;
                case "CONTROL":
                    ndkClient.Enqueue(InworldNDK.From.NDKControlPacket(packet));
                    break;
                case "EMOTION":
                    ndkClient.Enqueue(InworldNDK.From.NDKEmotionPacket(packet));
                    break;
                case "CANCEL":
                    ndkClient.Enqueue(InworldNDK.From.NDKCancelResponse(packet));
                    break;
                case "CUSTOM":
                    ndkClient.Enqueue(InworldNDK.From.NDKCustomPacket(packet));
                    break;
                default:
                    ndkClient.Enqueue(InworldNDK.From.NDKUnknownPacket(packet));
                    break;
            }
        }
        static internal void OnPhonemeReceived(PhonemeInfo packet) => s_ProcessingAudioChunk.ReceivePhoneme(packet);

        static internal void OnTriggerParamReceived(TriggerParam packet)
        {
            // Currently server doesn't support send trigger callback with param. 
        }
    }
}

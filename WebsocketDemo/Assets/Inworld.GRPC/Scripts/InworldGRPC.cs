namespace Inworld.Grpc
{
    public static class InworldGRPC
    {
        public static CapabilitiesRequest GetCapabilities(Capabilities rhs) => new CapabilitiesRequest
        {
            Audio = rhs.audio,
            Emotions = rhs.emotions,
            Interruptions = rhs.interruptions,
            NarratedActions = rhs.narratedActions,
            SilenceEvents = rhs.silence,
            Text = rhs.text,
            Triggers = rhs.triggers,
            Continuation = rhs.continuation,
            TurnBasedStt = rhs.turnBasedStt,
            PhonemeInfo = rhs.phonemeInfo
        };
    }
}

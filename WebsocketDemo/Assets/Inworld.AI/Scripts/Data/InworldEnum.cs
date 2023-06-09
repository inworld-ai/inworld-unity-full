namespace Inworld
{
    public enum InworldConnectionStatus
    {
        Idle, // Initial state
        Initializing,
        InitFailed,
        Initialized, // Logged in the server with API Key/Secret or Oculus Nonce/ID
        LoadingScene,
        LoadingSceneCompleted,
        Connecting, // Controller is connecting to World-Engine
        Connected, // Controller is connected to World-Engine and ready to work.
        LostConnect,
        Error // Some error occured.
    }
    public enum PacketType
    {
        UNKNOWN,
        TEXT,
        CONTROL,
        AUDIO,
        GESTURE,
        CUSTOM,
        CANCEL_RESPONSE,
        EMOTION,
        ACTION
    }
    public enum PacketStatus
    {
        RECEIVED,
        SEND,
        PLAYED,
        CANCELLED
    }
}

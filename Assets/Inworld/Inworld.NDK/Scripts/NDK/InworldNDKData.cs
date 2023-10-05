using System.Runtime.InteropServices;


namespace Inworld.NDK 
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Capabilities
    {
        public bool Text;
        public bool Audio;
        public bool Emotions ;
        public bool Interruptions;
        public bool Triggers;
        public bool PhonemeInfo;
        public bool TurnBasedSTT;
        public bool NarratedActions;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AgentInfo
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string BrainName;
        [MarshalAs(UnmanagedType.LPStr)]
        public string AgentId;
        [MarshalAs(UnmanagedType.LPStr)]
        public string GivenName;
        [MarshalAs(UnmanagedType.LPStr)]
        public string RpmModelUri;
        [MarshalAs(UnmanagedType.LPStr)]
        public string RpmImageUriPortrait;
        [MarshalAs(UnmanagedType.LPStr)]
        public string RpmImageUriPosture;
        [MarshalAs(UnmanagedType.LPStr)]
        public string AvatarImg;
        [MarshalAs(UnmanagedType.LPStr)]
        public string AvatarImgOriginal;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SessionInfo
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string sessionId;
        [MarshalAs(UnmanagedType.LPStr)]
        public string token;
        [MarshalAs(UnmanagedType.LPStr)]
        public string sessionSavedState;
        public long expirationTime;
        public bool isValid;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PacketId
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string uid;
        [MarshalAs(UnmanagedType.LPStr)]
        public string utteranceID;
        [MarshalAs(UnmanagedType.LPStr)]
        public string interactionID;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Routing
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string source;
        [MarshalAs(UnmanagedType.LPStr)]
        public string target;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Packet
    {
        public PacketId packetId;
        public Routing routing;
        public long timeStamp;
    };
    [StructLayout(LayoutKind.Sequential)]
    public struct TextPacket
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string text;
        public int isFinal;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct AudioPacket
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string audioChunk;
        public int type;
        public int phonemeCount;
    };
    
    [StructLayout(LayoutKind.Sequential)]
    public struct ControlPacket
    {
        public int action;
    };
    
    [StructLayout(LayoutKind.Sequential)]
    public struct EmotionPacket
    {
        public int behavior;
        public int strength;
    };
    [StructLayout(LayoutKind.Sequential)]
    public struct CancelResponsePacket
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string cancelInteractionID; // Yan: No need to receive utterance as they won't be sent.

    };
    [StructLayout(LayoutKind.Sequential)]
    public struct CustomPacket
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string triggerName;
    };
    [StructLayout(LayoutKind.Sequential)]
    public struct NDKPacket
    {
        public Packet packetInfo;
        [MarshalAs(UnmanagedType.LPStr)]
        public string packetType;
        public TextPacket textPacket;
        public AudioPacket audioPacket;
        public ControlPacket ctrlPacket;
        public EmotionPacket emoPacket;
        public CancelResponsePacket cancelResponsePacket;
        public CustomPacket customPacket;
    };
    [StructLayout(LayoutKind.Sequential)]
    public struct TriggerParam
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string packetID;
        [MarshalAs(UnmanagedType.LPStr)]
        public string paramName;
        [MarshalAs(UnmanagedType.LPStr)]
        public string paramValue;

    };
    [StructLayout(LayoutKind.Sequential)]
    public struct PhonemeInfo
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string packetID;
        [MarshalAs(UnmanagedType.LPUTF8Str)]
        public string code;
        public float timeStamp;
    };
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NDKLogCallBack(string message, int severity);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NDKCallback();
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void TextCallBack(TextPacket packet);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void AudioCallBack(AudioPacket packet);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ControlCallBack(ControlPacket packet);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void EmotionCallBack(EmotionPacket packet);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void CancelResponseCallBack(CancelResponsePacket packet);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void TriggerCallBack(CustomPacket packet);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NDKPacketCallBack(NDKPacket packet);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void TriggerParamCallBack(TriggerParam packet);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void PhonemeCallBack(PhonemeInfo packet);
}

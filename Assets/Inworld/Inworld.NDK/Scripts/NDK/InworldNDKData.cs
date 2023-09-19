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
        [MarshalAs(UnmanagedType.BStr)]
        public string BrainName;
        [MarshalAs(UnmanagedType.BStr)]
        public string AgentId;
        [MarshalAs(UnmanagedType.BStr)]
        public string GivenName;
        [MarshalAs(UnmanagedType.BStr)]
        public string RpmModelUri;
        [MarshalAs(UnmanagedType.BStr)]
        public string RpmImageUriPortrait;
        [MarshalAs(UnmanagedType.BStr)]
        public string RpmImageUriPosture;
        [MarshalAs(UnmanagedType.BStr)]
        public string AvatarImg;
        [MarshalAs(UnmanagedType.BStr)]
        public string AvatarImgOriginal;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SessionInfo
    {
        [MarshalAs(UnmanagedType.BStr)]
        public string sessionId;
        [MarshalAs(UnmanagedType.BStr)]
        public string token;
        [MarshalAs(UnmanagedType.BStr)]
        public string sessionSavedState;
        public long expirationTime;
        public bool isValid;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PacketId
    {
        [MarshalAs(UnmanagedType.BStr)]
        public string uid;
        [MarshalAs(UnmanagedType.BStr)]
        public string utteranceID;
        [MarshalAs(UnmanagedType.BStr)]
        public string interactionID;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Routing
    {
        [MarshalAs(UnmanagedType.BStr)]
        public string source;
        [MarshalAs(UnmanagedType.BStr)]
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
        public Packet packet;
        [MarshalAs(UnmanagedType.BStr)]
        public string text;
        public int isFinal;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct AudioPacket
    {
        public Packet packet;
        [MarshalAs(UnmanagedType.BStr)]
        public string audioChunk;
        public int type;
        public int phonemeCount;
    };
    
    [StructLayout(LayoutKind.Sequential)]
    public struct ControlPacket
    {
        public Packet packet;
        public int action;
    };
    
    [StructLayout(LayoutKind.Sequential)]
    public struct EmotionPacket
    {
        public Packet packet;
        public int behavior;
        public int strength;
    };
    [StructLayout(LayoutKind.Sequential)]
    public struct CancelResponsePacket
    {
        public Packet packet;
        [MarshalAs(UnmanagedType.BStr)]
        public string  cancelInteractionID; // Yan: No need to receive utterance as they won't be sent.

    };
    [StructLayout(LayoutKind.Sequential)]
    public struct CustomPacket
    {
        public Packet packet;
        [MarshalAs(UnmanagedType.BStr)]
        public string  triggerName;
    };
    [StructLayout(LayoutKind.Sequential)]
    public struct TriggerParam
    {
        [MarshalAs(UnmanagedType.BStr)]
        public string  packetID;
        [MarshalAs(UnmanagedType.BStr)]
        public string  paramName;
        [MarshalAs(UnmanagedType.BStr)]
        public string  paramValue;

    };
    [StructLayout(LayoutKind.Sequential)]
    public struct PhonemeInfo
    {
        [MarshalAs(UnmanagedType.BStr)]
        public string packetID;
        [MarshalAs(UnmanagedType.BStr)]
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
    public delegate void TriggerParamCallBack(TriggerParam packet);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void PhonemeCallBack(PhonemeInfo packet);
}

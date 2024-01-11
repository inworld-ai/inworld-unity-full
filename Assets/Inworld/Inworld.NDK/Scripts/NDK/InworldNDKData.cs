/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System.Runtime.InteropServices;


namespace Inworld.NDK 
{
    /// <summary>
    /// This file stores all the NDK acceptable data.
    /// All the Unity packet data need to serialize/deserialize to those data format first.
    /// </summary>
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
        public bool Relations;
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
    public struct RelationPacket
    {
        public int attraction;
        public int familiar;
        public int flirtatious;
        public int respect;
        public int trust;
    };
    [StructLayout(LayoutKind.Sequential)]
    public struct ActionPacket
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string content;
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
        public RelationPacket relationPacket;
        public ActionPacket actionPacket;
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
    public delegate void NDKPacketCallBack(NDKPacket packet);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void TriggerParamCallBack(TriggerParam packet);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void PhonemeCallBack(PhonemeInfo packet);
}

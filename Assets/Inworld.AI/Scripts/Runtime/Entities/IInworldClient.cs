using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioChunk = Inworld.Packets.AudioChunk;
using ControlEvent = Inworld.Grpc.ControlEvent;
using CustomEvent = Inworld.Packets.CustomEvent;
using EmotionEvent = Inworld.Packets.EmotionEvent;
#if  INWORLD_NDK
using GrpcPacket = Inworld.ProtoBuf.InworldPacket;
#else
using GrpcPacket = Inworld.Grpc.InworldPacket;
#endif
using InworldPacket = Inworld.Packets.InworldPacket;
using Routing = Inworld.Packets.Routing;
using TextEvent = Inworld.Packets.TextEvent;
using Inworld;
using Inworld.Grpc;
using System.Threading.Tasks;

public interface IInworldClient
{
    void Initialize(InworldClient _client);
    void Authenticate(string sessionToken);
    void OnAuthComplete();
    void OnAuthFailed(string message);
    void Update();

    bool IsAuthenticated { get; }
    string SessionID { get; }
    bool IsSessionInitialized { get; }
    
    Task<LoadSceneResponse> LoadScene(string sceneName);

    Task StartSession();
    Task EndSession();
    void SendEvent(InworldPacket packet);
    void ResolvePackets(GrpcPacket packet);
    void StartAudio(Routing routing);
    void SendAudio(AudioChunk audioChunk);
    void EndAudio(Routing routing);

    void Destroy();
    //bool GetIncomingEvent(out InworldPacket packet);
    //bool GetAudioChunk(out AudioChunk audioChunk);
    //bool GetAnimationChunk(out AnimationChunk animationChunk);

    //void ResolvePackets(InworldPacket packet);
}

//public interface IInworldClient<T> : IInworldClient
//{
//    //parameter may nee,d to be changed to account for websockets
//    void ResolvePackets(T packet);
//}


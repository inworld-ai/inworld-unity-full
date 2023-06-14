using Google.Protobuf;
using Grpc.Core;
using Inworld.Runtime;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using UnityEngine;


namespace Inworld.Grpc
{
    public class InworldGRPCClient : InworldClient
{
    [SerializeField] string m_SessionToken;
    [SerializeField] string m_APIKey;
    [SerializeField] string m_APISecret;
    
    InworldAuth m_Auth;
    WorldEngine.WorldEngineClient m_WorldEngineClient;
    Channel m_Channel;
    Metadata m_Header;
    
    protected override void Init()
    {
        base.Init();
        m_Auth = new InworldAuth();
        m_Channel = new Channel(m_ServerConfig.RuntimeServer, new SslCredentials());
        m_WorldEngineClient = new WorldEngine.WorldEngineClient(m_Channel);
    }

    public override void GetAccessToken() => _GenerateAccessTokenAsync();
    public override void LoadScene(string sceneFullName) => _LoadSceneAsync(sceneFullName);



    async void _GenerateAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(m_SessionToken))
        {
            _ReceiveCustomToken();
            return;
        }
        GenerateTokenRequest gtRequest = new GenerateTokenRequest
        {
            Key = m_APIKey,
            Resources =
            {
                InworldController.Instance.CurrentWorkspace
            }
                
        };
        Metadata metadata = new Metadata
        {
            {
                "authorization", m_Auth.GetHeader(m_ServerConfig.RuntimeServer, m_APIKey, m_APISecret)
            }
        };
        try
        {
            m_Auth.Token = await m_WorldEngineClient.GenerateTokenAsync(gtRequest, metadata, DateTime.UtcNow.AddHours(1));
            InworldAI.Log("Init Success!");
            m_Header = new Metadata
            {
                {"authorization", $"Bearer {m_Auth.Token.Token}"},
                {"session-id", m_Auth.Token.SessionId}
            };
            Debug.Log(m_Auth.Token.Token);
            Status = InworldConnectionStatus.Initialized;
        }
        catch (RpcException e)
        {
            Error = e.ToString();
        }
    }
    async void _LoadSceneAsync(string sceneName)
    {

        LoadSceneRequest lsRequest = new LoadSceneRequest
        {
            Name = sceneName,
            Capabilities = InworldGRPC.GetCapabilities(InworldAI.Capabilities),
            // User = InworldAI.User.Request,
            // Client = InworldAI.User.Client,
            // UserSettings = InworldAI.User.Settings
        };
        // if (!string.IsNullOrEmpty(LastState))
        // {
        //     lsRequest.SessionContinuation = new SessionContinuation
        //     {
        //         PreviousState = ByteString.FromBase64(LastState)
        //     };
        // }
        try
        {
            LoadSceneResponse response = await m_WorldEngineClient.LoadSceneAsync(lsRequest, m_Header);
            // Yan: They somehow use {WorkSpace}:{sessionKey} as "sessionKey" now. Need to remove the first part.
            m_SessionKey = response.Key.Split(':')[1];
            if (response.PreviousState != null)
            {
                foreach (PreviousState.Types.StateHolder stateHolder in response.PreviousState.StateHolders)
                {
                    InworldAI.Log($"Received Previous Packets: {stateHolder.Packets.Count}");
                }
            }
            m_Header.Add("Authorization", $"Bearer {m_SessionKey}");
            Status = InworldConnectionStatus.LoadingSceneCompleted;
        }
        catch (RpcException e)
        {
            Error = e.ToString();
        }
    }
    void _ReceiveCustomToken()
    {
        JObject data = JObject.Parse(m_SessionToken);
        if (data.ContainsKey("sessionId") && data.ContainsKey("token"))
        {
            InworldAI.Log("Init Success with Custom Token!");
            m_Header = new Metadata
            {
                {"authorization", $"Bearer {data["token"]}"},
                {"session-id", data["sessionId"]?.ToString()}
            };
            Status = InworldConnectionStatus.Initialized;
        }
        else
            Error = "Token Invalid";
    }
}
}


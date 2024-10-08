using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;

public class TestVivox : MonoBehaviour
{
    async void InitializeAsync()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await VivoxService.Instance.InitializeAsync();
        // await VivoxService.Instance.EnableAcousticEchoCancellationAsync();
        Debug.Log("Initialized!");
    }
    public async void LoginToVivoxAsync()
    {
        LoginOptions options = new LoginOptions();
        options.DisplayName = "Jin1234";
        options.EnableTTS = true;
        await VivoxService.Instance.LoginAsync(options);
    }
    public async void JoinEchoChannelAsync()
    {
        string channelToJoin = "Lobby";
        await VivoxService.Instance.JoinEchoChannelAsync(channelToJoin, ChatCapability.TextAndAudio);
    }
    public async void LeaveEchoChannelAsync()
    {
        string channelToLeave = "Lobby";
        await VivoxService.Instance.LeaveChannelAsync(channelToLeave);
    }
    public async void LogoutOfVivoxAsync ()
    {
        VivoxService.Instance.LogoutAsync();
    }
    // Start is called before the first frame update
    void Start()
    {
        InitializeAsync();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Ai.Inworld.Studio.V1Alpha;
using Grpc.Core;
using Inworld.Util;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
namespace Inworld.Studio
{
    /// <summary>
    ///     InworldStudio is a data processing class for communicating GRPC server.
    ///     It's response data would be stored in InworldUserSettings.
    /// </summary>
    public class InworldStudio
    {
        readonly IStudioDataHandler m_Owner;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="owner">
        ///     The StudioDataHandler Interface:
        ///     In Editor, It's InworldEditorStudio
        ///     In Runtime, It's RuntimeInworldStudio.
        /// </param>
        public InworldStudio(IStudioDataHandler owner)
        {
            m_Owner = owner;
        }
        /// <summary>
        ///     Get User Token (Studio Token)
        ///     If returned, data would be overwritten in InworldAI.User.
        ///     Studio Handler would invoke `OnUserTokenCompleted`
        ///     If failed, Studio Handler would invoke `OnStudioError`
        /// </summary>
        /// <param name="tokenForExchange">
        ///     ID Token used to exchange User Token (Studio Token),
        ///     or {OculusNonce|OculusID} if you're using Oculus.
        /// </param>
        /// <param name="authType">authType, by default is FireBase.</param>
        public async Task GetUserToken(string tokenForExchange, AuthType authType = AuthType.Firebase)
        {
            Channel channel = new Channel(InworldAI.Game.StudioServer, new SslCredentials());
            try
            {
                Users.UsersClient client = new Users.UsersClient(channel);
                GenerateTokenUserRequest gtuRequest = new GenerateTokenUserRequest
                {
                    Type = authType,
                    Token = tokenForExchange
                };
                string strAuthType = authType == AuthType.OculusNonce ? "oculus_nonce" : "firebase";
                Metadata headers = new Metadata
                {
                    {"X-Authorization-Bearer-Type", $"{strAuthType}"},
                    {"Authorization", $"Bearer {tokenForExchange}"}
                };
                try
                {
                    GenerateTokenUserResponse response = await client.GenerateTokenUserAsync(gtuRequest, headers).ResponseAsync;
                    InworldAI.User.OnLoggedInCompleted(response.Token, response.ExpirationTime.ToDateTime());
                    m_Owner?.OnUserTokenCompleted();
                }
                catch (RpcException e)
                {
                    m_Owner?.OnStudioError(StudioStatus.InitFailed, e.Message);
                }
            }
            finally
            {
                await channel.ShutdownAsync();
            }
        }

        /// <summary>
        ///     List Workspaces
        ///     If returned, the handler of Inworld Studio would invoke CreateWorkspaces
        ///     Otherwise, the handler would invoke OnStudioError.
        /// </summary>
        public async Task ListWorkspace()
        {
            Channel channel = new Channel(InworldAI.Game.StudioServer, new SslCredentials());
            try
            {
                Workspaces.WorkspacesClient client = new Workspaces.WorkspacesClient(channel);
                ListWorkspacesRequest lwsRequest = new ListWorkspacesRequest();
                try
                {
                    ListWorkspacesResponse response = await client.ListWorkspacesAsync(lwsRequest, InworldAI.User.Header).ResponseAsync;
                    m_Owner?.CreateWorkspaces(response.Workspaces.ToList());
                }
                catch (RpcException e)
                {
                    m_Owner?.OnStudioError(StudioStatus.ListWorkspaceFailed, e.Message);
                }
            }
            finally
            {
                await channel.ShutdownAsync();
            }
        }

        /// <summary>
        ///     List Scenes
        ///     If returned, the handler of Inworld Studio would invoke CreateScenes
        ///     Otherwise, the handler would invoke OnStudioError.
        /// </summary>
        public async Task ListScenes(InworldWorkspaceData workspace)
        {
            Channel channel = new Channel(InworldAI.Game.StudioServer, new SslCredentials());
            try
            {
                Scenes.ScenesClient client = new Scenes.ScenesClient(channel);
                ListScenesRequest request = new ListScenesRequest
                {
                    Parent = workspace.fullName
                };
                try
                {
                    ListScenesResponse response = await client.ListScenesAsync(request, InworldAI.User.Header).ResponseAsync;
                    m_Owner?.CreateScenes(workspace, response.Scenes.ToList());
                }
                catch (RpcException e)
                {
                    m_Owner?.OnStudioError(StudioStatus.ListSceneFailed, e.Message);
                }
            }
            finally
            {
                await channel.ShutdownAsync();
            }
        }
        public async Task LoginStudio()
        {
            Channel channel = new Channel(InworldAI.Game.StudioServer, new SslCredentials());
            try
            {

                BillingAccounts.BillingAccountsClient client = new BillingAccounts.BillingAccountsClient(channel);
                MeListBillingAccountsRequest request = new MeListBillingAccountsRequest();
                try
                {
                    ListBillingAccountsResponse response = await client.MeListBillingAccountsAsync(request, InworldAI.User.Header).ResponseAsync;
                    if (response.BillingAccounts.Count > 0)
                    {
                        InworldAI.User.Account = $"{response.BillingAccounts[0].Name}:{response.BillingAccounts[0].DisplayName}";
                        if (InworldAI.User.Name == "InworldUser") //YAN: Set Name again if you didn't have it.
                        {
                            string[] splits = response.BillingAccounts[0].DisplayName.Split('@');
                            InworldAI.User.Name = splits[0];
                        }
                    }
                }
                catch (RpcException e)
                {
                    Debug.LogError(e.Message);
                }
            }
            finally
            {
                await channel.ShutdownAsync();
            }
        }
        /// <summary>
        ///     List Characters
        ///     If returned, the handler of Inworld Studio would invoke CreateCharacters
        ///     Otherwise, the handler would invoke OnStudioError.
        /// </summary>
        public async Task ListCharacters(InworldWorkspaceData workspace)
        {
            Channel channel = new Channel(InworldAI.Game.StudioServer, new SslCredentials());
            try
            {
                Characters.CharactersClient client = new Characters.CharactersClient(channel);
                ListCharactersRequest request = new ListCharactersRequest
                {
                    Parent = workspace.fullName,
                    View = CharacterView.WithScenes
                };
                try
                {
                    ListCharactersResponse response = await client.ListCharactersAsync(request, InworldAI.User.Header).ResponseAsync;
                    m_Owner?.CreateCharacters(workspace, response.Characters.ToList());
                }
                catch (RpcException e)
                {
                    m_Owner?.OnStudioError(StudioStatus.ListCharacterFailed, e.Message);
                }
            }
            finally
            {
                await channel.ShutdownAsync();
            }
        }

        /// <summary>
        ///     List Shared Characters
        ///     This function is worked in Oculus only.
        ///     It needs OculusNonce|ID instead of ID Token to get shared Characters.
        ///     If returned, the handler of Inworld Studio would invoke CreateCharacters
        ///     Otherwise, the handler would invoke OnStudioError.
        /// </summary>
        public async Task ListSharedCharacters(InworldWorkspaceData sharedWorkspace, string oculusNonce, string oculusID)
        {
            Channel channel = new Channel(InworldAI.Game.StudioServer, new SslCredentials());
            try
            {
                Characters.CharactersClient client = new Characters.CharactersClient(channel);
                ListSharedCharactersRequest request = new ListSharedCharactersRequest
                {
                    Parent = "workspaces/-"
                };
                Metadata headers = new Metadata
                {
                    {"X-Authorization-Bearer-Type", "OCULUS_NONCE_GUEST"},
                    {"Authorization", $"Bearer {oculusNonce}|{oculusID}"}
                };
                try
                {
                    ListSharedCharactersResponse response = await client.ListSharedCharactersAsync(request, headers).ResponseAsync;
                    m_Owner?.CreateCharacters(sharedWorkspace, response.SharedCharacters.ToList());
                }
                catch (RpcException e)
                {
                    m_Owner?.OnStudioError(StudioStatus.ListSharedCharacterFailed, e.Message);
                }
            }
            finally
            {
                await channel.ShutdownAsync();
            }
        }

        /// <summary>
        ///     List API Key
        ///     If returned, the handler of Inworld Studio would invoke CreateIntegrations
        ///     Otherwise, the handler would invoke OnStudioError.
        /// </summary>
        /// <param name="workspaceData"></param>
        public async Task ListAPIKey(InworldWorkspaceData workspaceData)
        {
            Channel channel = new Channel(InworldAI.Game.StudioServer, new SslCredentials());
            try
            {
                ApiKeys.ApiKeysClient client = new ApiKeys.ApiKeysClient(channel);
                ListApiKeysRequest request = new ListApiKeysRequest
                {
                    Parent = workspaceData.fullName
                };

                try
                {
                    ListApiKeysResponse response = await client.ListApiKeysAsync(request, InworldAI.User.Header).ResponseAsync;
                    m_Owner?.CreateIntegrations(workspaceData, response.ApiKeys.ToList());
                }
                catch (RpcException e)
                {
                    m_Owner?.OnStudioError(StudioStatus.ListSharedCharacterFailed, e.Message);
                }
            }
            finally
            {
                await channel.ShutdownAsync();
            }
        }
    }
}

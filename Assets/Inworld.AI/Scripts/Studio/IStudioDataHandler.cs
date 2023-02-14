/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using Ai.Inworld.Studio.V1Alpha;
using Inworld.Util;
using System.Collections.Generic;
namespace Inworld.Studio
{
    /// <summary>
    ///     The Studio Data Handler interface for implement Studio connection APIs.
    ///     In Editor, It's inherited as InworldEditorStudio
    ///     In Runtime, It's inherited as RuntimeInworldStudio.
    /// </summary>
    public interface IStudioDataHandler
    {
        void CreateWorkspaces(List<Workspace> workspaces);
        void CreateScenes(InworldWorkspaceData workspace, List<Scene> scenes);
        void CreateCharacters(InworldWorkspaceData workspace, List<Character> characters);
        void CreateIntegrations(InworldWorkspaceData workspace, List<ApiKey> apiKeys);
        void OnStudioError(StudioStatus studioStatus, string msg);
        void OnUserTokenCompleted();
    }
}

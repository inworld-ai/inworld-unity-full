/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Inworld.Entities
{
    [Serializable]
    public class WorkspaceInfo
    {
        public string workspace;
        public string displayName;
    }

    [Serializable]
    public class UserShareDetails
    {
        public string workspace;
        public Role role;
    }

    [Serializable]
    public class UserShareInfo
    {
        public string email;
        public List<UserShareDetails> userShareDetails;
    }
    
    [Serializable]
    public class InworldProjectData
    {
        public string name;
        public string displayName;
        public List<string> owners;
        public List<string> collaborators;
        public List<string> workspaceNames;
        public List<WorkspaceInfo> workspaceInfo;
        public List<UserShareInfo> ownerInfo;
        public List<UserShareInfo> collaboratorInfo;

        [JsonIgnore] public string DisplayName => name.Substring(name.LastIndexOf('/') + 1);

        [JsonIgnore]
        public List<string> WorkspaceList => workspaceInfo.Select(ws => ws.displayName).ToList();

    }
    [Serializable]
    public class ListProjectResponse
    {
        public List<InworldProjectData> workspaceCollections;
        public string nextPageToken;
    }
}
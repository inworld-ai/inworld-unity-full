/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inworld.Entities
{
    [Serializable]
    public class InworldWorkspaceData
    {
        public string name; // Full Name
        public string displayName;
        [HideInInspector] public List<string> experimentalFeatures;
        [HideInInspector] public string billingAccount;
        [HideInInspector] public string meta;
        [HideInInspector] public string runtimeAccess;
        // YAN: Now charRef in scenes would be updated. No need to list characters.
        public List<InworldSceneData> scenes;
        public List<InworldKeySecret> keySecrets;
        public InworldKeySecret DefaultKey => keySecrets.Count > 0 ? keySecrets[0] : null;
    }
    [Serializable]
    public class ListWorkspaceResponse
    {
        public List<InworldWorkspaceData> workspaces;
        public string nextPageToken;
    }
}

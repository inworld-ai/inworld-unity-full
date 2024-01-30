/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Entities;
using UnityEditor.Experimental.GraphView;
namespace Inworld.Editors.Graph
{
    public class InworldGraphNode : Node
    {
        public string guid;
        public InworldNode nodeData;
        public InworldGraphView holder;
        public bool isEntryPoint = false;
    }
}

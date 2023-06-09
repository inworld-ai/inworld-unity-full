/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using UnityEngine;
namespace Inworld.Util
{
    /// <summary>
    ///     This class is used to save all kinds of urls related to Inworld Server.
    /// </summary>
    [CreateAssetMenu(fileName = "InworldServerConfig", menuName = "Inworld/Server Config", order = 4)]
    public class InworldServerConfig : ScriptableObject
    {
        [Header("Server Info:")]
        public string studio;
        public string runtime;
        public string web;
        public string tutorialPage;
        public int port;
        public string RuntimeServer => $"{runtime}:{port}";
        public string StudioServer => $"{studio}:{port}";
    }
}

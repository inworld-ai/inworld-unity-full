/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using UnityEngine;
namespace Inworld.Model
{
    /// <summary>
    ///     This is the interface for lip syncing.
    ///     By default it's null.
    ///     However, our model also supports Oculus Lip Sync.
    ///     To implement this, you need to fetch and download Oculus VR unity package:
    ///     https://developer.oculus.com/downloads/package/oculus-lipsync-unity/
    ///     Create a prefab with script inherit ILipAnimation, as well as
    ///     `OvrLipSync`, `OvrLipSyncContent` and `Audio Source`.
    ///     FYI:
    ///     The Viseme of SkinnedMeshRender in Ready Player Me Character starts at index 57,
    ///     and ends in 74.
    ///     The data value ranged from 0 to 1, while OVR's original demo ranged from 0 to 100.
    /// </summary>
    public interface ILipAnimations
    {
        public void ConfigureModel(GameObject model);
        public void StartLipSync();
        public void StopLipSync();
    }
}

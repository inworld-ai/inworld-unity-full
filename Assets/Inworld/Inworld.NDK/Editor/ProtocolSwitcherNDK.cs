/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using Inworld.NDK;
using UnityEditor;

namespace Inworld.Editors
{
    public class ProtocolSwitcherNDK
    {
        [MenuItem("Inworld/Switch Protocol/NDK")]
        public static void UpgradeNDK() => InworldEditorUtil.UpgradeProtocol<InworldNDKClient>();
    }
}
#endif

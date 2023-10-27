#if UNITY_EDITOR
using Inworld.AI.Editor;
using Inworld.NDK;
using UnityEditor;

namespace Inworld
{
    public class ProtocolSwitcherNDK
    {
        [MenuItem("Inworld/Switch Protocol/NDK")]
        public static void UpgradeNDK() => InworldEditorUtil.UpgradeProtocol<InworldNDKClient>();
    }
}
#endif

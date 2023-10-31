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

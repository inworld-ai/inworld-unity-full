#if UNITY_EDITOR
using Inworld.AI.Editor;
using Inworld.NDK;
using UnityEditor;

namespace Inworld
{
    /// <summary>
    ///     This class would be called when package is imported, or Unity Editor is opened.
    /// </summary>
    [InitializeOnLoad]
    public class ProtocolSwitcherNDK
    {
        const string k_UpdateTitle = "Upgrade to NDK";
        const string k_UpdateContent = "This scene is not using NDK, do you want to upgrade?\nNote: NDK is faster, but worked for Windows application only.";
        
        static ProtocolSwitcherNDK()
        {
            AssetDatabase.importPackageCompleted += packName =>
            {
                if (!InworldController.Instance || InworldController.Client is InworldNDKClient)
                    return;
                if (EditorUtility.DisplayDialog(k_UpdateTitle, k_UpdateContent, "OK", "Cancel",
                                                DialogOptOutDecisionType.ForThisMachine, "UpgradeNDK"))
                    UpgradeNDK();
            };
        }
        [MenuItem("Inworld/Switch Protocol/NDK")]
        public static void UpgradeNDK() => InworldEditorUtil.UpgradeProtocol<InworldNDKClient>();
    }
}
#endif

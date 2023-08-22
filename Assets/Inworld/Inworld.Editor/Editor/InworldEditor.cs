using UnityEditor;
using UnityEngine;

namespace Inworld.AI.Editor
{
    public class InworldEditor : EditorWindow
    {
        /// <summary>
        ///     Get Instance of the InworldEditor.
        ///     It'll create a Inworld Studio Panel if the panel hasn't opened.
        /// </summary>
        public static InworldEditor Instance => GetWindow<InworldEditor>("Inworld Studio");
        /// <summary>
        ///     Open Inworld Studio Panel
        ///     It will detect and pop import window if you dont have TMP imported.
        /// </summary>
        public void ShowPanel()
        {
            titleContent = new GUIContent("Inworld Studio");
            Show();
        }
    }
}

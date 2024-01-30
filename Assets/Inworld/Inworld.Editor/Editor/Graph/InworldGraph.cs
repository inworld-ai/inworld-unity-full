/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Entities;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace Inworld.Editors.Graph
{
    public class InworldGraph : EditorWindow
    {
        static string s_CurrentGraphName = "Default Graph";
        static InworldGraphData s_GraphData;
        InworldGraphView m_GraphView;
        
        [MenuItem("Inworld/Open Graph")]
        public static void OpenGraphWindow()
        {
            InworldGraph window = GetWindow<InworldGraph>();
            window.titleContent = new GUIContent("Inworld Graph");
        }
        public static void CloseWindow() => GetWindow<InworldGraph>().Close();

        public static void OpenGraphWindow(string graphName, InworldGraphData graphData)
        {
            s_CurrentGraphName = graphName;
            s_GraphData = graphData;
            InworldGraph window = GetWindow<InworldGraph>();
            window.titleContent = new GUIContent(graphName);
        }
        void OnEnable()
        {
            m_GraphView = new InworldGraphView(s_GraphData);
            m_GraphView.name = s_CurrentGraphName;
            m_GraphView.StretchToParentSize();
            rootVisualElement.Add(m_GraphView);
        }
        void OnDisable()
        {
            rootVisualElement.Remove(m_GraphView);
        }
    }
}

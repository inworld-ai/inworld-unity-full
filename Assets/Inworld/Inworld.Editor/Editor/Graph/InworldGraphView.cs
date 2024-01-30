/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Entities;
using System;

using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Inworld.Editors.Graph
{
    public class InworldGraphView : GraphView
    {
        Vector2 m_Size = new Vector2(150, 200);
        Vector2 m_OffSet = new Vector2(400, 150);
        public List<InworldGraphNode> m_Nodes = new List<InworldGraphNode>();
        public InworldGraphView(InworldGraphData graphData)
        {
            Debug.Log($"Graph is null? {graphData == null}");
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            ClearAllNodes();
            GenerateGraph(graphData);
        }
        void ClearAllNodes()
        {
            foreach (InworldGraphNode node in m_Nodes)
            {
                RemoveElement(node);
            }
            m_Nodes.Clear();
        }
        Port GeneratePort(InworldGraphNode node, Direction direction)
        {
            return node.InstantiatePort(Orientation.Horizontal, direction, Port.Capacity.Multi, typeof(InworldGraphNode));
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach
            (
                port =>
                {
                    if (startPort != port && startPort.node != port.node)
                        compatiblePorts.Add(port);
                }
            );
            return compatiblePorts;
        }
        public InworldGraphNode InstantiateNode(InworldNode nodeData, bool isEntry = false, int nIndex = 0)
        {
            InworldGraphNode node = new InworldGraphNode()
            {
                nodeData = nodeData,
                title = nodeData.NodeName,
                holder = this,
                guid = Guid.NewGuid().ToString(),
                isEntryPoint = isEntry
            };
            foreach (InworldNodeQuote quote in nodeData.quotes)
            {
                node.mainContainer.Add(new Label(quote.character));
            }
            
            Port input = GeneratePort(node, Direction.Input);
            input.portName = "Before";
            node.inputContainer.Add(input);
            Port output = GeneratePort(node, Direction.Output);
            output.portName = "Next";
            node.outputContainer.Add(output);
            node.SetPosition(new Rect(new Vector2(m_OffSet.x * (nIndex % 5), m_OffSet.y * (nIndex / 5)), m_Size));
            return node;
        }
        public Edge InstantiateEdge(InworldGraphNode from, InworldGraphNode to)
        {
            Edge edge = new Edge
            {
                input = from.outputContainer.Children().FirstOrDefault(p => p is Port) as Port,
                output = to.inputContainer.Children().FirstOrDefault(p => p is Port) as Port
            };
            from.RefreshPorts();
            from.RefreshExpandedState();
            to.RefreshPorts();
            to.RefreshExpandedState();
            return edge;
        }
        void GenerateGraph(InworldGraphData data)
        {
            // 1. Render nodes.
            for (int i = 0; i < data.nodes.Count; i++)
            {
                InworldGraphNode node = InstantiateNode(data.nodes[i], i == 0, i);
                m_Nodes.Add(node);
                AddElement(node);
            }
            // 2. Render Edges
            foreach (InworldEdge edge in data.connections)
            {
                InworldGraphNode fromNode = m_Nodes.FirstOrDefault(n => n.nodeData.scene == edge.nodeFrom);
                InworldGraphNode toNode = m_Nodes.FirstOrDefault(n => n.nodeData.scene == edge.nodeTo);
                if (fromNode != null && toNode != null)
                {
                    AddElement(InstantiateEdge(fromNode, toNode));
                }
            }
        }
    }
}


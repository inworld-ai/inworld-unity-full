/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Inworld.Editors.Graph
{
    public class InworldGraphView : GraphView
    {
        Vector2 m_OffSet = new Vector2(10, 0);
        public List<InworldGraphNode> m_Nodes = new List<InworldGraphNode>();
        public InworldGraphView(InworldGraphData graphData)
        {
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
        VisualElement _CreateDescriptionContainer(InworldNode nodeData) 
        {
            VisualElement descriptionContainer = new VisualElement();
            descriptionContainer.Add(new Label(_GetSceneDescription(nodeData))
            {
                style =
                {
                    paddingTop = 10,
                    paddingBottom = 10,
                    paddingLeft = 20,
                    paddingRight = 20,
                    maxWidth = 250,
                    whiteSpace = WhiteSpace.Normal
                }
            });
            return descriptionContainer;
        }
        VisualElement _CreateThumbTextContainer(Texture2D texture, string text)
        {
            VisualElement thumbText = new VisualElement();
            thumbText.style.flexDirection = FlexDirection.Row;

            thumbText.Add(new Image
            {
                image = texture,
                style =
                {
                    width = 50,
                    height = 50,
                    paddingLeft = 20,
                }
            });
            thumbText.Add(new Label(text)               
            { 
                style =
                {
                    fontSize = 14,
                    paddingTop = 20,
                    justifyContent = Justify.FlexEnd
                }
            });
            return thumbText;
        }

        VisualElement _CreateBulbContainer(string text)
        {
            VisualElement bulbText = new VisualElement();
            bulbText.style.flexDirection = FlexDirection.Row;

            bulbText.Add(new Image
            {
                image = InworldEditor.Bulb,
                style =
                {
                    width = 50,
                    height = 50,
                    paddingTop = 10,
                }
            });
            bulbText.Add(new Label(text)               
            { 
                style =
                {
                    fontSize = 14,
                    paddingTop = 20,
                    paddingLeft = -40,
                    paddingRight = 20,
                    justifyContent = Justify.FlexEnd
                }
            });
            return bulbText;
        }
        public InworldGraphNode InstantiateNode(InworldNode nodeData, bool isEntry, int nIndex)
        {
            InworldGraphNode node = new InworldGraphNode()
            {
                nodeData = nodeData,
                title = nodeData.NodeName,
                holder = this,
                guid = Guid.NewGuid().ToString(),
                isEntryPoint = isEntry
            };
            var descriptionContainer = _CreateDescriptionContainer(nodeData);
            VisualElement charContainer = new VisualElement();
            VisualElement namesContainer = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                }
            };

            List<string> characters = new List<string>();
            InworldSceneData scene = InworldAI.User.GetSceneByFullName(nodeData.scene);
            foreach (CharacterReference quote in scene.characterReferences)
            {
                string charDisplayName = InworldAI.User.GetCharacterByFullName(quote.character)?.givenName;
                if (!characters.Contains(charDisplayName))
                    characters.Add(charDisplayName);
            }
            foreach (string charDisplayName in characters)
            {
                namesContainer.Add(_CreateThumbTextContainer(InworldEditor.DefaultThumbnail, charDisplayName));
            }

            VisualElement numberContainer = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    alignItems = Align.FlexEnd,
                    justifyContent = Justify.FlexEnd
                }
            };
            numberContainer.Add(_CreateBulbContainer(_GetCommonKnowledges(nodeData)));

            charContainer.Add(namesContainer);
            charContainer.Add(numberContainer);
            charContainer.style.flexDirection = FlexDirection.Row;
            
            node.mainContainer.Add(descriptionContainer);
            node.mainContainer.Add(charContainer);
            
            Port input = GeneratePort(node, Direction.Input);
            input.portName = "Before";
            node.inputContainer.Add(input);
            
            Port output = GeneratePort(node, Direction.Output);
            output.portName = "Next";
            node.outputContainer.Add(output);
            return node;
        }
        string _GetSceneDescription(InworldNode nodeData) => InworldAI.User.GetSceneByFullName(nodeData.scene)?.description;

        string _GetCommonKnowledges(InworldNode nodeData)
        {
            HashSet<string> knowledges = new HashSet<string>();
            InworldSceneData scene = InworldAI.User.GetSceneByFullName(nodeData.scene);
            if (scene != null && scene.commonKnowledges.Count != 0)
            {
                foreach (string knowledge in scene.commonKnowledges)
                {
                    knowledges.Add(knowledge);
                }
            }
            foreach (InworldNodeQuote quote in nodeData.quotes)
            {
                InworldCharacterData charData = InworldAI.User.GetCharacterByFullName(quote.character);
                if (charData == null)
                    continue;
                foreach (string knowledge in charData.commonKnowledges)
                {
                    knowledges.Add(knowledge);
                }
            }
            return $": {knowledges.Count}";
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
            if (data == null)
                return;
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
        public void ArrangeNodes()
        {
            float initWidth = 0, initHeight = 0;
            if (m_Nodes.Count > 0)
                initHeight = (Screen.height - m_Nodes[0].GetPosition().size.y) * 0.5f;
            foreach (var node in m_Nodes)
            {
                initWidth = node.GetInitPosition(initWidth, m_OffSet.x, initHeight);
            }
        }
    }
}


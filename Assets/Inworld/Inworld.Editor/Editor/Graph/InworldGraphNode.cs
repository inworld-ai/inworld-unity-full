/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Entities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Inworld.Editors.Graph
{
    public class InworldGraphNode : Node
    {
        public string guid;
        public InworldNode nodeData;
        public InworldGraphView holder;
        public bool isEntryPoint = false;
        public Rect m_Position;

        public InworldGraphNode ()
        {
            string btnName = "collapse-button";
            VisualElement foldout = this.Q(btnName);
            if (foldout != null)
            {
                foldout.RemoveFromHierarchy();
            }
            else
            {
                Debug.LogError($"Cannot Find {btnName}!!!");
            }
            RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 2) 
                {
                    OnDoubleClick();
                }
            });
        }

        void OnDoubleClick()
        {
            if (InworldEditor.Instance.Status != EditorStatus.SelectGameData)
                return;
            InworldEditor.Instance.CurrentState.ProcessData(nodeData.scene);
            InworldGraph.CloseWindow();
        }
        public float GetInitPosition(float width, float offset, float height)
        {
            m_Position = GetPosition();
            Vector2 newPosition = new Vector2(width, height);
            float result= width + m_Position.size.x + offset;
            SetPosition(new Rect(newPosition, m_Position.size));
            return result;
        }

    }
}

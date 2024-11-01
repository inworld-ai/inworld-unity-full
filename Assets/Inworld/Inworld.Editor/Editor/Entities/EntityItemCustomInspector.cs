/*************************************************************************************************
 * Copyright 2024 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using Inworld.BehaviorEngine;
using System;
using UnityEditor;
using UnityEngine;

namespace Inworld.Editors
{
    [CustomEditor(typeof(EntityItem))]
    public class EntityItemCustomInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Generate ID"))
            {
                serializedObject.FindProperty("m_ID").stringValue = Guid.NewGuid().ToString();
            }
            serializedObject.ApplyModifiedProperties();
            
            base.OnInspectorGUI();
        }
    }
}
#endif

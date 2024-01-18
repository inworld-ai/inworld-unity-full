/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
#if UNITY_EDITOR
using Inworld.UI;
using UnityEngine;
using UnityEditor;

namespace Inworld.Editors
{
    [CustomEditor(typeof(InworldReadme))][InitializeOnLoad]
    public class InworldReadmeEditor : Editor 
    {
        [SerializeField] GUIStyle m_LinkStyle;
        [SerializeField] GUIStyle m_TitleStyle;
        [SerializeField] GUIStyle m_HeadingStyle;
        [SerializeField] GUIStyle m_BodyStyle;

        const float k_Space = 16f;
        static InworldReadmeEditor()
	    {
		    EditorApplication.delayCall += SelectReadmeAutomatically;
	    }
	    
	    static void SelectReadmeAutomatically()
        {
            if (InworldEditor.LoadedReadme)
                return;
            SelectReadme();
            InworldEditor.LoadedReadme = true;
        }
    
	    [MenuItem("Inworld/About")]
	    static void SelectReadme() => Selection.activeObject = InworldEditor.ReadMe;
	    
	    protected override void OnHeaderGUI()
	    {
		    InworldReadme inworldReadme = (InworldReadme)target;
		    Init(inworldReadme);
		    
		    float iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth/3f - 20f, 48f);
		    
		    GUILayout.BeginHorizontal("In BigTitle");
		    {
			    GUILayout.Label(inworldReadme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
			    GUILayout.Label(inworldReadme.title, TitleStyle);
		    }
		    GUILayout.EndHorizontal();
            
	    }
	    
	    public override void OnInspectorGUI()
	    {
		    InworldReadme inworldReadme = (InworldReadme)target;
		    Init(inworldReadme);
		    foreach (InworldReadme.Section section in inworldReadme.sections)
		    {
			    if (!string.IsNullOrEmpty(section.heading))
			    {
				    GUILayout.Label(section.heading, HeadingStyle);
			    }
			    if (!string.IsNullOrEmpty(section.text))
			    {
				    GUILayout.Label(section.text, BodyStyle);
			    }
			    if (!string.IsNullOrEmpty(section.linkText))
			    {
				    if (LinkLabel(new GUIContent(section.linkText)))
				    {
					    Application.OpenURL(section.url);
				    }
			    }
			    GUILayout.Space(k_Space);
		    }
	    }
	    
	    
	    bool m_Initialized;
	    
	    GUIStyle LinkStyle => m_LinkStyle;
        GUIStyle TitleStyle => m_TitleStyle;
        GUIStyle HeadingStyle => m_HeadingStyle;
        GUIStyle BodyStyle => m_BodyStyle;

	    
	    void Init(InworldReadme inworldReadme)
	    {
		    if (m_Initialized)
			    return;
		    m_BodyStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                fontSize = 14,
                font = inworldReadme.contentFont,
                richText = true
            };

            m_TitleStyle = new GUIStyle(m_BodyStyle)
            {
                fontSize = 32,
                font = inworldReadme.titleFont,
                alignment = TextAnchor.LowerCenter
            };

            m_HeadingStyle = new GUIStyle(m_BodyStyle)
            {
                fontStyle = FontStyle.Bold
            };
            m_TitleStyle.font = inworldReadme.titleFont;
		    m_HeadingStyle.fontSize = 18 ;
		    
		    m_LinkStyle = new GUIStyle(m_BodyStyle)
            {
                wordWrap = false,
                normal =
                {
                    // Match selection color which works nicely for both light and dark skins
                    textColor = new Color (0x00/255f, 0x78/255f, 0xDA/255f, 1f)
                },
                stretchWidth = false
            };

            m_Initialized = true;
	    }
	    
	    bool LinkLabel (GUIContent label, params GUILayoutOption[] options)
	    {
		    Rect position = GUILayoutUtility.GetRect(label, LinkStyle, options);

		    Handles.BeginGUI ();
		    Handles.color = LinkStyle.normal.textColor;
		    Handles.DrawLine (new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
		    Handles.color = Color.white;
		    Handles.EndGUI ();

		    EditorGUIUtility.AddCursorRect (position, MouseCursor.Link);

		    return GUI.Button (position, label, LinkStyle);
	    }
    }
}
#endif


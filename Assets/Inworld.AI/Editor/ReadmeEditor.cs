using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;
using Object = UnityEngine.Object;


namespace Inworld
{
    [CustomEditor(typeof(Readme))][InitializeOnLoad]
    public class ReadmeEditor : Editor 
    {
        const string k_ShowedReadmeSessionStateName = "ReadmeEditor.showedReadme";
        const float k_Space = 16f;
        static ReadmeEditor()
	    {
		    EditorApplication.delayCall += SelectReadmeAutomatically;
	    }
	    
	    static void SelectReadmeAutomatically()
        {
            if (SessionState.GetBool(k_ShowedReadmeSessionStateName, false))
                return;
            Readme readme = SelectReadme();
            SessionState.SetBool(k_ShowedReadmeSessionStateName, true);

            if (!readme || readme.loadedLayout)
                return;
            LoadLayout();
            readme.loadedLayout = true;
        }
	    
	    static void LoadLayout()
	    {
		    Assembly assembly = typeof(EditorApplication).Assembly;
		    Type windowLayoutType = assembly.GetType("UnityEditor.WindowLayout", true);
            MethodInfo method = windowLayoutType.GetMethod("LoadWindowLayout", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string), typeof(bool) }, null);
            if (method != null)
                method.Invoke
                (
                    null, new object[]
                    {
                        Path.Combine(Application.dataPath, "Inworld.AI/Default.dwlt"), false
                    }
                );
        }
	    
	    [MenuItem("Inworld/About")]
	    static Readme SelectReadme() 
	    {
		    string[] ids = AssetDatabase.FindAssets("Readme t:Readme");
		    if (ids.Length >= 1)
		    {
			    Object readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));
			    
			    Selection.objects = new[]{readmeObject};
			    
			    return (Readme)readmeObject;
		    }
		    else
		    {
			    Debug.Log("Couldn't find a readme");
			    return null;
		    }
	    }
	    
	    protected override void OnHeaderGUI()
	    {
		    Readme readme = (Readme)target;
		    Init(readme);
		    
		    float iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth/3f - 20f, 48f);
		    
		    GUILayout.BeginHorizontal("In BigTitle");
		    {
			    GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
			    GUILayout.Label(readme.title, TitleStyle);
		    }
		    GUILayout.EndHorizontal();
	    }
	    
	    public override void OnInspectorGUI()
	    {
		    Readme readme = (Readme)target;
		    Init(readme);
		    
		    foreach (Readme.Section section in readme.sections)
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
	    
	    GUIStyle LinkStyle { get { return m_LinkStyle; } }
	    [SerializeField] GUIStyle m_LinkStyle;
	    
	    GUIStyle TitleStyle { get { return m_TitleStyle; } }
	    [SerializeField] GUIStyle m_TitleStyle;
	    
	    GUIStyle HeadingStyle { get { return m_HeadingStyle; } }
	    [SerializeField] GUIStyle m_HeadingStyle;
	    
	    GUIStyle BodyStyle { get { return m_BodyStyle; } }
	    [SerializeField] GUIStyle m_BodyStyle;
	    
	    void Init(Readme readme)
	    {
		    if (m_Initialized)
			    return;
		    m_BodyStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                fontSize = 14,
                font = readme.contentFont,
                richText = true
            };

            m_TitleStyle = new GUIStyle(m_BodyStyle)
            {
                fontSize = 32,
                font = readme.titleFont,
                alignment = TextAnchor.LowerCenter
            };

            m_HeadingStyle = new GUIStyle(m_BodyStyle)
            {
                fontStyle = FontStyle.Bold
            };
            m_TitleStyle.font = readme.titleFont;
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



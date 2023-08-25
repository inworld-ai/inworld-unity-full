using Inworld;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;


public class InworldGameData : ScriptableObject
{
    [FormerlySerializedAs("m_WorkspaceFullName")]public string workspaceFullName;
    [FormerlySerializedAs("m_SceneFullName")]public string sceneFullName;
    [FormerlySerializedAs("m_APIKey")]public string apiKey;
    [FormerlySerializedAs("m_APISecret")]public string apiSecret;
    [FormerlySerializedAs("m_Capabilities")]public Capabilities capabilities;

    public void SetData(string wsFullName, string sceneFullName, string apiKey, string apiSecret)
    {
        workspaceFullName = wsFullName;
        this.sceneFullName = sceneFullName;
        this.apiKey = apiKey;
        this.apiSecret = apiSecret;
        capabilities = new Capabilities(InworldAI.Capabilities);
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }
}

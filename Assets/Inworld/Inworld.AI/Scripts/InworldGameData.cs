using Inworld;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class InworldGameData : ScriptableObject
{
    public string sceneFullName;
    public string apiKey;
    public string apiSecret;
    public List<InworldCharacterData> characters;
    public Capabilities capabilities;

    public string SceneFileName
    {
        get
        {
            string[] data = sceneFullName.Split('/');
            return data.Length < 4 ? sceneFullName : $"{data[3]}_{data[1]}";
        }
    }

    public void SetData(InworldSceneData sceneData, InworldKeySecret keySecret)
    {
        if (sceneData != null)
        {
            sceneFullName = sceneData.name;
            characters ??= new List<InworldCharacterData>();
            characters.Clear();
            foreach (CharacterReference charRef in sceneData.characterReferences)
            {
                characters.Add(new InworldCharacterData(charRef));
            }
        }
        if (keySecret != null)
        {
            apiKey = keySecret.key;
            apiSecret = keySecret.secret;
        }
        capabilities = new Capabilities(InworldAI.Capabilities);
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }
}

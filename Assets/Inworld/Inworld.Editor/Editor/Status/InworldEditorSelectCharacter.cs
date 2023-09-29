#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Inworld.AI.Editor
{
    public class InworldEditorSelectCharacter: IEditorState
    {
        Vector2 m_ScrollPosition;
        public void OnOpenWindow()
        {
            if (!InworldController.Instance || !InworldController.Instance.GameData)
            {
                InworldEditor.Instance.Status = EditorStatus.SelectGameData; // YAN: Fall back.
            }
        }
        public void DrawTitle()
        {
            EditorGUILayout.Space();
            if (InworldEditor.Is3D)
                EditorGUILayout.LabelField("Please select characters and drag to the scene.", InworldEditor.Instance.TitleStyle);
            else
            {
                EditorGUILayout.LabelField("Done!", InworldEditor.Instance.TitleStyle);
                EditorGUILayout.LabelField("You can close the tab now.", InworldEditor.Instance.TitleStyle);
            }
            EditorGUILayout.Space();
        }
        public void DrawContent()
        {
            if (!InworldEditor.Is3D || !InworldController.Instance || !InworldController.Instance.GameData)
                return;
            // 1. Get the character prefab for character in current scene. (Default or Specific)
            InworldSceneData sceneData = InworldAI.User.GetSceneByFullName(InworldController.Instance.GameData.sceneFullName);
            if (sceneData == null)
                return;
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
            EditorGUILayout.BeginHorizontal();
            foreach (CharacterReference charRef in sceneData.characterReferences.Where(charRef => GUILayout.Button(charRef.characterOverloads[0].defaultCharacterDescription.givenName, InworldEditor.Instance.BtnCharStyle(_GetTexture2D(charRef)))))
            {
                Selection.activeObject = _GetPrefab(charRef);
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Add PlayerController to Scene", GUILayout.ExpandWidth(true)))
            {
                if (!Object.FindObjectOfType<PlayerController>())
                    Object.Instantiate(InworldEditor.PlayerController);
            }
        }
        public void DrawButtons()
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Back", InworldEditor.Instance.BtnStyle))
            {
                InworldEditor.Instance.Status = EditorStatus.SelectGameData;
            }
            GUILayout.EndHorizontal();
        }
        public void OnExit()
        {
            
        }
        public void OnEnter()
        {
            EditorUtility.ClearProgressBar();
            _CreatePrefabVariants();
        }
        public void PostUpdate()
        {
            
        }
        void _CreatePrefabVariants()
        {
            // 1. Get the character prefab for character in current scene. (Default or Specific)
            InworldSceneData sceneData = InworldAI.User.GetSceneByFullName(InworldController.Instance.GameData.sceneFullName);
            if (sceneData == null)
                return;
            foreach (CharacterReference charRef in sceneData.characterReferences)
            {
                GameObject downloadedModel = _GetModel(charRef);
                _CreateVariant(charRef, downloadedModel);
            }
            // 2. Save the prefab variant as the new data.
        }
        static void _CreateVariant(CharacterReference charRef, GameObject customModel)
        { 
            // Use Current Model
            InworldCharacter avatar = customModel ?
                Object.Instantiate(InworldEditor.Instance.RPMPrefab) :
                Object.Instantiate(InworldEditor.Instance.InnequinPrefab);

            InworldCharacter iwChar = avatar.GetComponent<InworldCharacter>();
            iwChar.Data = new InworldCharacterData(charRef);
            if (customModel)
            {
                GameObject newModel = PrefabUtility.InstantiatePrefab(customModel) as GameObject;
                if (newModel)
                {
                    
                    Transform oldArmature = avatar.transform.Find("Armature");
                    if (oldArmature)
                        Object.DestroyImmediate(oldArmature.gameObject);
                    newModel.transform.name = "Armature";
                    newModel.transform.SetParent(avatar.transform);
                }
            }
            if (!Directory.Exists($"{InworldEditorUtil.UserDataPath}/{InworldEditor.PrefabPath}"))
            {
                Directory.CreateDirectory($"{InworldEditorUtil.UserDataPath}/{InworldEditor.PrefabPath}");
            }
            string newAssetPath = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.PrefabPath}/{charRef.CharacterFileName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(avatar.gameObject, newAssetPath);
            AssetDatabase.SaveAssets();
            Object.DestroyImmediate(avatar.gameObject);
            AssetDatabase.Refresh();
        }
        GameObject _GetModel(CharacterReference charRef)
        {
            string filePath = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.AvatarPath}/{charRef.CharacterFileName}.glb";
            return !File.Exists(filePath) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
        }
        Texture2D _GetTexture2D(CharacterReference charRef)
        {
            string filePath = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.ThumbnailPath}/{charRef.CharacterFileName}.png";
            if (!File.Exists(filePath))
                return InworldAI.DefaultThumbnail;
            byte[] imgBytes = File.ReadAllBytes(filePath);
            Texture2D loadedTexture = new Texture2D(0,0); 
            loadedTexture.LoadImage(imgBytes);
            return loadedTexture;
        }
        GameObject _GetPrefab(CharacterReference charRef)
        {
            string filePath = $"{InworldEditorUtil.UserDataPath}/{InworldEditor.PrefabPath}/{charRef.CharacterFileName}.prefab";
            if (File.Exists(filePath))
                return AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
            InworldAI.LogError($"Cannot find {charRef.CharacterFileName}.prefab");
            return null;
        }
    }
}
#endif
using Inworld.Audio;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

#if INWORLD_AUDIO_FEATURES
[CustomEditor(typeof(AudioCapture))]
public class AudioCaptureEditor : Editor
{
    private ReorderableList reorderableList;

    private void OnEnable()
    {
        var audioCapture = (AudioCapture)target;
        var audioFeatures = serializedObject.FindProperty("audioFeatures");

        reorderableList = new ReorderableList(serializedObject, audioFeatures, true, true, true, true)
        {
            drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Audio Features");
            },
            drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = audioFeatures.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
            },
            onAddCallback = list =>
            {
                var menu = new GenericMenu();

                // Dynamically find all available AudioFeatureBase types
                var featureTypes = GetAllAudioFeatureTypes();
                foreach (var type in featureTypes)
                {
                    menu.AddItem(new GUIContent(type.Name), false, () =>
                    {
                        var newFeature = CreateInstance(type);
                        newFeature.name = type.Name;

                        // Save the ScriptableObject as an asset
                        AssetDatabase.AddObjectToAsset(newFeature, audioCapture);
                        AssetDatabase.SaveAssets();

                        // Add the feature to the list
                        audioFeatures.arraySize++;
                        audioFeatures.GetArrayElementAtIndex(audioFeatures.arraySize - 1).objectReferenceValue = newFeature;

                        serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.ShowAsContext();
            },
            onRemoveCallback = list =>
            {
                var element = audioFeatures.GetArrayElementAtIndex(list.index);
                var obj = element.objectReferenceValue;

                if (obj != null)
                {
                    // Remove from asset
                    DestroyImmediate(obj, true);
                    AssetDatabase.SaveAssets();
                }

                ReorderableList.defaultBehaviours.DoRemoveButton(list);
            }
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        reorderableList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

    private static List<System.Type> GetAllAudioFeatureTypes()
    {
        var types = new List<System.Type>();
        var baseType = typeof(AudioFeature);

        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (baseType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                {
                    types.Add(type);
                }
            }
        }

        return types;
    }
}
#endif
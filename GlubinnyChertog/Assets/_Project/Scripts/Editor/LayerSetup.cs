using UnityEditor;
using UnityEngine;

namespace GlubinnyChertog.EditorTools
{
    /// <summary>
    /// Editor-only utility to programmatically add required layers
    /// (e.g. "Enemy") to ProjectSettings/TagManager.asset.
    /// Run via menu: Tools/GlubinnyChertog/Setup Layers
    /// </summary>
    public static class LayerSetup
    {
        private static readonly string[] RequiredLayers = { "Enemy", "Player", "Projectile" };

        [MenuItem("Tools/GlubinnyChertog/Setup Layers")]
        public static void CreateRequiredLayers()
        {
            SerializedObject tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            SerializedProperty layersProp = tagManager.FindProperty("layers");

            foreach (string layerName in RequiredLayers)
            {
                bool alreadyExists = false;
                int firstEmptyIndex = -1;

                for (int i = 8; i < layersProp.arraySize; i++) // first 8 layers are reserved by Unity
                {
                    SerializedProperty element = layersProp.GetArrayElementAtIndex(i);
                    if (element.stringValue == layerName)
                    {
                        alreadyExists = true;
                        break;
                    }
                    if (firstEmptyIndex == -1 && string.IsNullOrEmpty(element.stringValue))
                    {
                        firstEmptyIndex = i;
                    }
                }

                if (!alreadyExists && firstEmptyIndex != -1)
                {
                    layersProp.GetArrayElementAtIndex(firstEmptyIndex).stringValue = layerName;
                    Debug.Log($"[LayerSetup] Created layer '{layerName}' at index {firstEmptyIndex}");
                }
                else if (alreadyExists)
                {
                    Debug.Log($"[LayerSetup] Layer '{layerName}' already exists, skipping.");
                }
                else
                {
                    Debug.LogWarning($"[LayerSetup] No free layer slots available for '{layerName}'.");
                }
            }

            tagManager.ApplyModifiedProperties();
        }
    }
}

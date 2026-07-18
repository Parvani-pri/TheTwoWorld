using System.IO;
using TwoWorlds.Inventory;
using UnityEditor;
using UnityEngine;

namespace TwoWorlds.EditorTools
{
    public static class SetupItemDataCatalog
    {
        const string CatalogPath = "Assets/Resources/ItemDataCatalog.asset";
        const string ItemFolder = "Assets/Data/Item Data";

        [MenuItem("Tools/Two Worlds/Rebuild Item Data Catalog")]
        public static void Rebuild()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<ItemDataCatalog>(CatalogPath);
            if (catalog == null)
            {
                var dir = Path.GetDirectoryName(CatalogPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                catalog = ScriptableObject.CreateInstance<ItemDataCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            var guids = AssetDatabase.FindAssets("t:ItemData", new[] { ItemFolder });
            var items = new ItemData[guids.Length];

            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                items[i] = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            }

            var serializedCatalog = new SerializedObject(catalog);
            serializedCatalog.FindProperty("items").arraySize = items.Length;
            for (var i = 0; i < items.Length; i++)
                serializedCatalog.FindProperty("items").GetArrayElementAtIndex(i).objectReferenceValue = items[i];

            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log($"[SetupItemDataCatalog] Rebuilt catalog with {items.Length} items.");
        }
    }
}

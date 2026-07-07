#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using TwoWorlds.Inventory;
using UnityEditor;
using UnityEngine;

namespace TwoWorlds.Dialogue.Editor
{
    public static class DialogueCsvImporter
    {
        [MenuItem("Two Worlds/Dialogue/Import CSV To Dialogue Assets")]
        static void ImportFromSelectedCsv()
        {
            var csvPath = EditorUtility.OpenFilePanel("Select Dialogue CSV", Application.dataPath, "csv");
            if (string.IsNullOrEmpty(csvPath))
                return;

            ImportCsvFile(csvPath, Path.GetDirectoryName(csvPath));
        }

        [MenuItem("Assets/Two Worlds/Import Dialogue CSV", true)]
        static bool ValidateImportSelectedAsset()
        {
            return Selection.activeObject is TextAsset;
        }

        [MenuItem("Assets/Two Worlds/Import Dialogue CSV")]
        static void ImportSelectedTextAsset()
        {
            var textAsset = Selection.activeObject as TextAsset;
            if (textAsset == null)
                return;

            var assetPath = AssetDatabase.GetAssetPath(textAsset);
            ImportCsvFile(Path.GetFullPath(assetPath), Path.GetDirectoryName(assetPath));
        }

        public static void ImportCsvFile(string csvFullPath, string outputFolderFullPath)
        {
            if (!File.Exists(csvFullPath))
            {
                Debug.LogError($"[DialogueCsvImporter] CSV not found: {csvFullPath}");
                return;
            }

            var rewardItems = LoadAllItemData();
            var portraitDatabase = LoadPortraitDatabase();
            var csvText = File.ReadAllText(csvFullPath);
            var dialogues = DialogueCsvParser.Parse(csvText, rewardItems, portraitDatabase);
            if (dialogues.Count == 0)
            {
                Debug.LogWarning("[DialogueCsvImporter] No dialogues found in CSV.");
                return;
            }

            var unityOutputFolder = ToUnityRelativePath(outputFolderFullPath);
            if (string.IsNullOrEmpty(unityOutputFolder))
                unityOutputFolder = "Assets/Dialogue Data";

            EnsureFolder(unityOutputFolder);

            foreach (var pair in dialogues)
            {
                var assetPath = $"{unityOutputFolder}/{pair.Key}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<DialogueData>(assetPath);
                if (existing == null)
                {
                    existing = ScriptableObject.CreateInstance<DialogueData>();
                    AssetDatabase.CreateAsset(existing, assetPath);
                }

                existing.ApplySessionData(pair.Value);
                EditorUtility.SetDirty(existing);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[DialogueCsvImporter] Imported {dialogues.Count} dialogue asset(s) to {unityOutputFolder}.");
        }

        static ItemData[] LoadAllItemData()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(ItemData)}");
            var items = new List<ItemData>(guids.Length);

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (item != null)
                    items.Add(item);
            }

            return items.ToArray();
        }

        static CharacterPortraitDatabase LoadPortraitDatabase()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(CharacterPortraitDatabase)}");
            if (guids.Length == 0)
                return null;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<CharacterPortraitDatabase>(path);
        }

        static string ToUnityRelativePath(string fullPath)
        {
            fullPath = fullPath.Replace('\\', '/');
            var dataPath = Application.dataPath.Replace('\\', '/');
            if (!fullPath.StartsWith(dataPath))
                return null;

            return "Assets" + fullPath.Substring(dataPath.Length);
        }

        static void EnsureFolder(string unityFolderPath)
        {
            if (AssetDatabase.IsValidFolder(unityFolderPath))
                return;

            var parts = unityFolderPath.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);

                current = next;
            }
        }
    }
}
#endif

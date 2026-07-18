using System.IO;
using TwoWorlds.Combat;
using TwoWorlds.Inventory;
using UnityEditor;
using UnityEngine;

namespace TwoWorlds.EditorTools
{
    public static class SetupEnemyLoot
    {
        const string MenuPath = "Tools/Two Worlds/Setup Enemy Loot";
        const string LootFolder = "Assets/Data/Enemy Loot";

        static readonly (string prefabPath, string tableName, LootEntrySpec[] drops)[] Configurations =
        {
            (
                "Assets/Character Perfab/WangLiang_Scripted.prefab",
                "EnemyLootTable_WangLiang",
                new[]
                {
                    Fixed("Assets/Data/Item Data/Shrike Feather.asset", 1),
                    Random("Assets/Data/Item Data/Spirit Wood.asset", 3, 5)
                }
            ),
            (
                "Assets/Character Perfab/sMonster1_lvl1 Variant.prefab",
                "EnemyLootTable_sMonster1",
                new[] { Fixed("Assets/Data/Item Data/Dream Silk.asset", 1) }
            ),
            (
                "Assets/Character Perfab/sMonster2_lvl1 Variant.prefab",
                "EnemyLootTable_sMonster2",
                new[] { Fixed("Assets/Data/Item Data/Plague Core.asset", 1) }
            ),
            (
                "Assets/Character Perfab/Li Variant.prefab",
                "EnemyLootTable_Li",
                new[]
                {
                    Fixed("Assets/Data/Item Data/Demon Mask.asset", 1),
                    Random("Assets/Data/Item Data/Spirit Wood.asset", 3, 5)
                }
            ),
            (
                "Assets/Character Perfab/sMonster3 Variant.prefab",
                "EnemyLootTable_sMonster3",
                new[]
                {
                    Fixed("Assets/Data/Item Data/Mandrill Horn.asset", 1),
                    Fixed("Assets/Data/Item Data/Spirit Wood.asset", 1)
                }
            ),
            (
                "Assets/Character Perfab/sMonster4 Variant.prefab",
                "EnemyLootTable_sMonster4",
                new[]
                {
                    Fixed("Assets/Data/Item Data/Kodama Crown.asset", 1),
                    Fixed("Assets/Data/Item Data/Spirit Wood.asset", 1)
                }
            )
        };

        [MenuItem(MenuPath)]
        public static void Run()
        {
            if (!Directory.Exists(LootFolder))
                Directory.CreateDirectory(LootFolder);

            var updatedPrefabs = 0;

            foreach (var config in Configurations)
            {
                var table = EnsureLootTable(config.tableName, config.drops);
                if (table == null)
                    continue;

                if (WirePrefab(config.prefabPath, table))
                    updatedPrefabs++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SetupEnemyLoot] Updated {updatedPrefabs} prefab(s) and ensured loot tables in {LootFolder}.");
        }

        static EnemyLootTable EnsureLootTable(string tableName, LootEntrySpec[] specs)
        {
            var assetPath = $"{LootFolder}/{tableName}.asset";
            var table = AssetDatabase.LoadAssetAtPath<EnemyLootTable>(assetPath);
            if (table == null)
            {
                table = ScriptableObject.CreateInstance<EnemyLootTable>();
                AssetDatabase.CreateAsset(table, assetPath);
            }

            var serialized = new SerializedObject(table);
            var dropsProperty = serialized.FindProperty("drops");
            dropsProperty.arraySize = specs.Length;

            for (var i = 0; i < specs.Length; i++)
            {
                var spec = specs[i];
                var item = AssetDatabase.LoadAssetAtPath<ItemData>(spec.itemPath);
                if (item == null)
                {
                    Debug.LogError($"[SetupEnemyLoot] Missing ItemData at {spec.itemPath}");
                    return null;
                }

                var element = dropsProperty.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("item").objectReferenceValue = item;
                element.FindPropertyRelative("amountMode").enumValueIndex = (int)spec.amountMode;
                element.FindPropertyRelative("amount").intValue = spec.amount;
                element.FindPropertyRelative("minAmount").intValue = spec.minAmount;
                element.FindPropertyRelative("maxAmount").intValue = spec.maxAmount;
                element.FindPropertyRelative("dropChance").floatValue = 1f;
            }

            serialized.FindProperty("scatterRadius").floatValue = 0.35f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(table);
            return table;
        }

        static bool WirePrefab(string prefabPath, EnemyLootTable table)
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"[SetupEnemyLoot] Missing prefab at {prefabPath}");
                return false;
            }

            if (prefabRoot.GetComponent<CombatHealth>() == null)
            {
                Debug.LogError($"[SetupEnemyLoot] Prefab has no CombatHealth: {prefabPath}");
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                return false;
            }

            var dropper = prefabRoot.GetComponent<EnemyLootDropper>();
            if (dropper == null)
                dropper = prefabRoot.AddComponent<EnemyLootDropper>();

            var serialized = new SerializedObject(dropper);
            serialized.FindProperty("lootTable").objectReferenceValue = table;
            serialized.FindProperty("dropOnlyOnce").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return true;
        }

        static LootEntrySpec Fixed(string itemPath, int amount) =>
            new(itemPath, LootAmountMode.Fixed, amount, amount, amount);

        static LootEntrySpec Random(string itemPath, int min, int max) =>
            new(itemPath, LootAmountMode.RandomRange, 1, min, max);

        readonly struct LootEntrySpec
        {
            public readonly string itemPath;
            public readonly LootAmountMode amountMode;
            public readonly int amount;
            public readonly int minAmount;
            public readonly int maxAmount;

            public LootEntrySpec(string itemPath, LootAmountMode amountMode, int amount, int minAmount, int maxAmount)
            {
                this.itemPath = itemPath;
                this.amountMode = amountMode;
                this.amount = amount;
                this.minAmount = minAmount;
                this.maxAmount = maxAmount;
            }
        }
    }
}

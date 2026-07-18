using TwoWorlds.Combat;
using TwoWorlds.Core;
using TwoWorlds.Inventory;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using XuFu.MaskSystem;

namespace TwoWorlds.EditorTools
{
    public static class SetupLevelXuFuWiring
    {
        const string MenuPath = "Tools/Two Worlds/Wire Level XuFu References";

        static readonly string[] LevelScenePaths =
        {
            "Assets/Scenes/Level1.unity",
            "Assets/Scenes/Level2.unity",
            "Assets/Scenes/Level3.unity",
        };

        [MenuItem(MenuPath)]
        public static void RunActiveScene()
        {
            WireScene(EditorSceneManager.GetActiveScene().path);
        }

        [MenuItem(MenuPath + "/All Levels")]
        public static void RunAllLevels()
        {
            foreach (var path in LevelScenePaths)
                WireScene(path);

            Debug.Log("[SetupLevelXuFuWiring] Finished wiring XuFu in Level1, Level2, and Level3.");
        }

        static void WireScene(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError("[SetupLevelXuFuWiring] No scene path.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var inputReader = Object.FindFirstObjectByType<InputReader>();
            if (inputReader == null)
            {
                Debug.LogError($"[SetupLevelXuFuWiring] {scene.name}: GameSystem InputReader not found.");
                return;
            }

            var attackController = Object.FindFirstObjectByType<PlayerAttackController>();
            var combatController = Object.FindFirstObjectByType<PlayerCombatController>();
            var maskController = Object.FindFirstObjectByType<MaskController>();
            var combatHealth = Object.FindFirstObjectByType<CombatHealth>();

            if (attackController == null || combatController == null || maskController == null || combatHealth == null)
            {
                Debug.LogError($"[SetupLevelXuFuWiring] {scene.name}: Missing XuFu combat components.");
                return;
            }

            var playerInventory = attackController.GetComponent<PlayerInventory>()
                ?? attackController.gameObject.AddComponent<PlayerInventory>();

            var hitbox = attackController.GetComponentInChildren<CombatHitbox>(true);
            var healthBar = FindHealthBarTransform(scene.name);

            SetRef(attackController, "inputReader", inputReader);
            SetRef(attackController, "hitbox", hitbox);
            SetRef(combatController, "inputReader", inputReader);
            SetRef(maskController, "inputReader", inputReader);
            SetRef(combatHealth, "healthBarTransform", healthBar);

            WireCanvasInventory(scene.name, playerInventory);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[SetupLevelXuFuWiring] {scene.name}: XuFu wiring complete.");
        }

        static Transform FindHealthBarTransform(string sceneName)
        {
            switch (sceneName)
            {
                case "Level1":
                    return GameObject.Find("HealthBar")?.transform;
                case "Level2":
                    return GameObject.Find("HealthBar")?.transform;
                case "Level3":
                    return GameObject.Find("HealthBar")?.transform;
                default:
                    return GameObject.Find("HealthBar")?.transform;
            }
        }

        static void WireCanvasInventory(string sceneName, PlayerInventory playerInventory)
        {
            var canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                Debug.LogWarning($"[SetupLevelXuFuWiring] {sceneName}: Canvas not found.");
                return;
            }

            var inventoryUi = canvas.GetComponent<InventoryUI>();
            if (inventoryUi != null)
                SetRef(inventoryUi, "playerInventory", playerInventory);

            var craftingUi = canvas.GetComponent<CraftingUI>();
            if (craftingUi != null)
                SetRef(craftingUi, "playerInventory", playerInventory);
        }

        static void SetRef(Object target, string propertyName, Object value)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"[SetupLevelXuFuWiring] Property '{propertyName}' not found on {target.GetType().Name}.");
                return;
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}

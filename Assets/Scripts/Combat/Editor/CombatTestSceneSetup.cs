using TMPro;
using TwoWorlds.Combat;
using TwoWorlds.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace TwoWorlds.Combat.Editor
{
    public static class CombatTestSceneSetup
    {
        const string ScenePath = "Assets/Scenes/CombatTest.unity";
        const string PlayerAttackPath = "Assets/Data/Combat/Attacks/PlayerLightAttack.asset";
        const string EnemyAttackPath = "Assets/Data/Combat/Attacks/EnemyMeleeAttack.asset";

        [MenuItem("Two Worlds/Combat/Setup CombatTest Scene")]
        public static void SetupScene()
        {
            if (EditorSceneManager.GetActiveScene().path != ScenePath)
                EditorSceneManager.OpenScene(ScenePath);

            var player = GameObject.Find("Player");
            var enemy = GameObject.Find("Enemy");
            var gameSystem = GameObject.Find("GameSystem");

            if (player == null || enemy == null || gameSystem == null)
            {
                Debug.LogError("[CombatTestSceneSetup] Missing Player, Enemy, or GameSystem in CombatTest.");
                return;
            }

            var inputReader = gameSystem.GetComponent<InputReader>();
            var playerAttackData = AssetDatabase.LoadAssetAtPath<AttackData>(PlayerAttackPath);
            var enemyAttackData = AssetDatabase.LoadAssetAtPath<AttackData>(EnemyAttackPath);

            SetupCombatManager(gameSystem);
            SetupActor(player, playerAttackData, inputReader, destroyOnDeath: false);
            SetupActor(enemy, enemyAttackData, inputReader: null, destroyOnDeath: true, isEnemy: true);
            SetupCombatUI(player.GetComponent<CombatHealth>(), enemy.GetComponent<CombatHealth>());

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[CombatTestSceneSetup] CombatTest scene wired for full combat testing.");
        }

        static void SetupCombatManager(GameObject gameSystem)
        {
            var combatManager = gameSystem.GetComponent<CombatManager>();
            if (combatManager == null)
                combatManager = gameSystem.AddComponent<CombatManager>();

            SetBool(combatManager, "autoStartOnLoad", true);
        }

        static void SetupActor(
            GameObject actorObject,
            AttackData attackData,
            InputReader inputReader,
            bool destroyOnDeath,
            bool isEnemy = false)
        {
            EnsureKinematicRb(actorObject);

            var actor = actorObject.GetComponent<CombatActor>();
            var health = actorObject.GetComponent<CombatHealth>();
            if (health == null)
                health = actorObject.AddComponent<CombatHealth>();

            SetInt(health, "maxHealth", 10);
            SetBool(health, "destroyOnDeath", destroyOnDeath);

            var hitbox = SetupHitbox(
                actorObject.transform,
                "Hitbox",
                new Vector2(0.9f, 0f),
                new Vector2(1.2f, 1.4f),
                actor);

            SetupHurtbox(
                actorObject.transform,
                "Hurtbox",
                new Vector2(0.9f, 1.4f),
                actor,
                health);

            if (isEnemy)
            {
                var attackAI = actorObject.GetComponent<EnemyAttackAI>();
                if (attackAI == null)
                    attackAI = actorObject.AddComponent<EnemyAttackAI>();

                SetObject(attackAI, "attackData", attackData);
                SetObject(attackAI, "hitbox", hitbox);
            }
            else
            {
                var attackController = actorObject.GetComponent<PlayerAttackController>();
                if (attackController == null)
                    attackController = actorObject.AddComponent<PlayerAttackController>();

                SetObject(attackController, "inputReader", inputReader);
                SetObject(attackController, "attackData", attackData);
                SetObject(attackController, "hitbox", hitbox);
            }
        }

        static void SetupCombatUI(CombatHealth playerHealth, CombatHealth enemyHealth)
        {
            var existingCanvas = GameObject.Find("CombatUI");
            if (existingCanvas != null)
                Object.DestroyImmediate(existingCanvas);

            var canvasGo = new GameObject("CombatUI");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            var resultRoot = new GameObject("ResultPanel");
            resultRoot.transform.SetParent(canvasGo.transform, false);
            var resultRect = resultRoot.AddComponent<RectTransform>();
            resultRect.anchorMin = Vector2.zero;
            resultRect.anchorMax = Vector2.one;
            resultRect.offsetMin = Vector2.zero;
            resultRect.offsetMax = Vector2.zero;
            var resultBg = resultRoot.AddComponent<Image>();
            resultBg.color = new Color(0f, 0f, 0f, 0.65f);
            resultRoot.SetActive(false);

            var resultTextGo = new GameObject("ResultText");
            resultTextGo.transform.SetParent(resultRoot.transform, false);
            var resultTextRect = resultTextGo.AddComponent<RectTransform>();
            resultTextRect.anchorMin = new Vector2(0.5f, 0.5f);
            resultTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            resultTextRect.sizeDelta = new Vector2(800, 200);
            var resultText = resultTextGo.AddComponent<TextMeshProUGUI>();
            resultText.alignment = TextAlignmentOptions.Center;
            resultText.fontSize = 72;
            resultText.color = Color.white;
            resultText.text = "Victory!";

            var resultUI = canvasGo.AddComponent<CombatResultUI>();
            SetObject(resultUI, "panelRoot", resultRoot);
            SetObject(resultUI, "resultText", resultText);
            SetString(resultUI, "victoryMessage", "Victory!");
            SetString(resultUI, "defeatMessage", "Defeat...");

            CreateHealthBar(canvasGo.transform, "PlayerHealthBar", new Vector2(0.05f, 0.92f),
                new Color(0.2f, 0.85f, 0.35f), playerHealth);
            CreateHealthBar(canvasGo.transform, "EnemyHealthBar", new Vector2(0.95f, 0.92f),
                new Color(0.9f, 0.25f, 0.25f), enemyHealth);
        }

        static void CreateHealthBar(Transform parent, string barName, Vector2 anchor, Color fillColor, CombatHealth target)
        {
            var barRoot = new GameObject(barName);
            barRoot.transform.SetParent(parent, false);
            var rootRect = barRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = anchor;
            rootRect.anchorMax = anchor;
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = new Vector2(420, 36);

            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(barRoot.transform, false);
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgGo.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);
            bgImage.raycastTarget = false;
            AssignDefaultUISprite(bgImage);

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(bgGo.transform, false);
            var fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fillGo.AddComponent<Image>();
            fillImage.color = fillColor;
            fillImage.type = Image.Type.Simple;
            fillImage.raycastTarget = false;
            AssignDefaultUISprite(fillImage);

            var barUI = barRoot.AddComponent<CombatPlayerStatsUI>();
            SetObject(barUI, "targetHealth", target);
            SetObject(barUI, "fillImage", fillImage);
            SetBool(barUI, "hideWhenFull", false);
        }

        static CombatHitbox SetupHitbox(Transform parent, string childName, Vector2 localPos, Vector2 size, CombatActor owner)
        {
            var child = EnsureChild(parent, childName);
            child.localPosition = new Vector3(localPos.x, localPos.y, 0f);

            var collider = child.GetComponent<BoxCollider2D>();
            if (collider == null)
                collider = child.gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = size;

            var hitbox = child.GetComponent<CombatHitbox>();
            if (hitbox == null)
                hitbox = child.gameObject.AddComponent<CombatHitbox>();

            SetObject(hitbox, "owner", owner);
            SetObject(hitbox, "hitCollider", collider);
            return hitbox;
        }

        static void SetupHurtbox(Transform parent, string childName, Vector2 size, CombatActor owner, CombatHealth health)
        {
            var child = EnsureChild(parent, childName);
            child.localPosition = Vector3.zero;

            var collider = child.GetComponent<BoxCollider2D>();
            if (collider == null)
                collider = child.gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = size;

            var hurtbox = child.GetComponent<CombatHurtbox>();
            if (hurtbox == null)
                hurtbox = child.gameObject.AddComponent<CombatHurtbox>();

            SetObject(hurtbox, "owner", owner);
            SetObject(hurtbox, "health", health);
        }

        static Transform EnsureChild(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child != null)
                return child;

            var go = new GameObject(childName);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        static void EnsureKinematicRb(GameObject go)
        {
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb == null)
                rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.simulated = true;
        }

        static void SetObject(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
                return;
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetBool(Object target, string field, bool value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
                return;
            prop.boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetInt(Object target, string field, int value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
                return;
            prop.intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AssignDefaultUISprite(Image image)
        {
            if (image == null || image.sprite != null)
                return;

            image.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        }

        static void SetString(Object target, string field, string value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
                return;
            prop.stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}

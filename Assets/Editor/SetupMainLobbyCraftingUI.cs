using System.IO;
using TMPro;
using TwoWorlds.Inventory;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace TwoWorlds.EditorTools
{
    public static class SetupMainLobbyCraftingUI
    {
        const string MenuPath = "Tools/Two Worlds/Setup MainLobby Crafting UI";
        const string DatabasePath = "Assets/Data/CraftRecipeDatabase.asset";
        const string RowPrefabPath = "Assets/Prefab/CraftRecipeRowPrefab.prefab";

        [MenuItem(MenuPath)]
        public static void Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "MainLobby")
            {
                Debug.LogError("[SetupMainLobbyCraftingUI] Please open MainLobby scene first.");
                return;
            }

            var canvasGo = GameObject.Find("Canvas");
            var inventoryPanel = GameObject.Find("InventoryPanel");
            var discardButtonGo = GameObject.Find("DiscardButton");
            var playerInv = Object.FindFirstObjectByType<PlayerInventory>();

            if (canvasGo == null || inventoryPanel == null || discardButtonGo == null)
            {
                Debug.LogError("[SetupMainLobbyCraftingUI] Missing Canvas, InventoryPanel, or DiscardButton.");
                return;
            }

            if (playerInv != null)
            {
                var dropper = playerInv.GetComponent<ItemDropper>();
                if (dropper != null)
                    Object.DestroyImmediate(dropper, true);
            }

            var discardText = discardButtonGo.GetComponentInChildren<TMP_Text>(true);
            if (discardText != null)
                discardText.text = "合成";

            var discardBtn = discardButtonGo.GetComponent<Button>();
            var db = EnsureDatabase();
            var craftPanelGo = EnsureCraftPanel(inventoryPanel.transform);
            var rowPrefab = EnsureRowPrefab();
            var craftingUi = canvasGo.GetComponent<CraftingUI>() ?? canvasGo.AddComponent<CraftingUI>();

            WireCraftingUi(
                craftingUi,
                playerInv,
                db,
                discardBtn,
                craftPanelGo,
                rowPrefab);

            craftPanelGo.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[SetupMainLobbyCraftingUI] MainLobby crafting UI setup complete.");
        }

        static CraftRecipeDatabase EnsureDatabase()
        {
            var db = AssetDatabase.LoadAssetAtPath<CraftRecipeDatabase>(DatabasePath);
            if (db != null)
                return db;

            var dir = Path.GetDirectoryName(DatabasePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            db = ScriptableObject.CreateInstance<CraftRecipeDatabase>();
            AssetDatabase.CreateAsset(db, DatabasePath);
            AssetDatabase.SaveAssets();
            return db;
        }

        static GameObject EnsureCraftPanel(Transform inventoryPanel)
        {
            var existing = inventoryPanel.Find("CraftPanel");
            if (existing != null)
                return existing.gameObject;

            var craftPanelGo = CreateUiObject("CraftPanel", inventoryPanel);
            StretchFull(craftPanelGo.GetComponent<RectTransform>());
            craftPanelGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

            var listArea = CreateUiObject("RecipeListArea", craftPanelGo.transform);
            SetAnchors(listArea.GetComponent<RectTransform>(), new Vector2(0.02f, 0.08f), new Vector2(0.48f, 0.92f));
            listArea.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);

            var scrollGo = CreateUiObject("RecipeScrollView", listArea.transform);
            StretchFull(scrollGo.GetComponent<RectTransform>());
            var scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scrollGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
            scrollGo.AddComponent<Mask>().showMaskGraphic = false;

            var viewport = CreateUiObject("Viewport", scrollGo.transform);
            StretchFull(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<Image>().color = Color.white;
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            var content = CreateUiObject("Content", viewport.transform);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = Vector2.zero;

            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.spacing = 4f;
            layout.padding = new RectOffset(4, 4, 4, 4);

            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRt;

            var previewArea = CreateUiObject("PreviewArea", craftPanelGo.transform);
            SetAnchors(previewArea.GetComponent<RectTransform>(), new Vector2(0.52f, 0.08f), new Vector2(0.98f, 0.92f));
            previewArea.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);

            CreatePreviewText(previewArea.transform, "PreviewNameText", 22, FontStyles.Bold,
                new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.95f));
            CreatePreviewText(previewArea.transform, "PreviewIngredientsText", 16, FontStyles.Normal,
                new Vector2(0.05f, 0.35f), new Vector2(0.95f, 0.75f));
            CreatePreviewText(previewArea.transform, "PreviewOutputText", 16, FontStyles.Normal,
                new Vector2(0.05f, 0.18f), new Vector2(0.95f, 0.33f));

            var status = CreatePreviewText(previewArea.transform, "PreviewStatusText", 14, FontStyles.Normal,
                new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.16f));
            status.color = new Color(0.85f, 0.85f, 0.85f, 1f);

            CreateButton(previewArea.transform, "ConfirmCraftButton", "確認合成",
                new Vector2(0.25f, 0.02f), new Vector2(0.75f, 0.08f));
            CreateButton(craftPanelGo.transform, "CraftCloseButton", "關閉",
                new Vector2(0.92f, 0.92f), new Vector2(0.98f, 0.98f));

            return craftPanelGo;
        }

        static CraftRecipeRowUI EnsureRowPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<CraftRecipeRowUI>(RowPrefabPath);
            if (existing != null)
                return existing;

            var rowGo = CreateUiObject("CraftRecipeRowPrefab", null);
            var rowRt = rowGo.GetComponent<RectTransform>();
            rowRt.sizeDelta = new Vector2(0f, 36f);

            var layoutElement = rowGo.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 36f;
            layoutElement.minHeight = 36f;

            var rowImage = rowGo.AddComponent<Image>();
            rowImage.color = new Color(1f, 1f, 1f, 0.08f);

            var rowButton = rowGo.AddComponent<Button>();
            rowButton.targetGraphic = rowImage;

            var canvasGroup = rowGo.AddComponent<CanvasGroup>();
            var label = CreateLabel(rowGo.transform, "Label", 16, TextAlignmentOptions.MidlineLeft);
            var labelRt = label.GetComponent<RectTransform>();
            labelRt.offsetMin = new Vector2(8f, 0f);
            labelRt.offsetMax = new Vector2(-8f, 0f);

            var rowUi = rowGo.AddComponent<CraftRecipeRowUI>();
            var rowSo = new SerializedObject(rowUi);
            rowSo.FindProperty("button").objectReferenceValue = rowButton;
            rowSo.FindProperty("labelText").objectReferenceValue = label;
            rowSo.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
            rowSo.FindProperty("backgroundImage").objectReferenceValue = rowImage;
            rowSo.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(rowGo, RowPrefabPath).GetComponent<CraftRecipeRowUI>();
            Object.DestroyImmediate(rowGo);
            return prefab;
        }

        static void WireCraftingUi(
            CraftingUI craftingUi,
            PlayerInventory playerInventory,
            CraftRecipeDatabase database,
            Button craftOpenButton,
            GameObject craftPanelRoot,
            CraftRecipeRowUI rowPrefab)
        {
            var root = craftPanelRoot.transform;
            var serialized = new SerializedObject(craftingUi);
            serialized.FindProperty("playerInventory").objectReferenceValue = playerInventory;
            serialized.FindProperty("recipeDatabase").objectReferenceValue = database;
            serialized.FindProperty("craftOpenButton").objectReferenceValue = craftOpenButton;
            serialized.FindProperty("craftCloseButton").objectReferenceValue =
                root.Find("CraftCloseButton")?.GetComponent<Button>();
            serialized.FindProperty("craftPanelRoot").objectReferenceValue = craftPanelRoot;
            serialized.FindProperty("recipeListContainer").objectReferenceValue =
                root.Find("RecipeListArea/RecipeScrollView/Viewport/Content");
            serialized.FindProperty("recipeRowPrefab").objectReferenceValue = rowPrefab;
            serialized.FindProperty("previewNameText").objectReferenceValue =
                root.Find("PreviewArea/PreviewNameText")?.GetComponent<TMP_Text>();
            serialized.FindProperty("previewIngredientsText").objectReferenceValue =
                root.Find("PreviewArea/PreviewIngredientsText")?.GetComponent<TMP_Text>();
            serialized.FindProperty("previewOutputText").objectReferenceValue =
                root.Find("PreviewArea/PreviewOutputText")?.GetComponent<TMP_Text>();
            serialized.FindProperty("previewStatusText").objectReferenceValue =
                root.Find("PreviewArea/PreviewStatusText")?.GetComponent<TMP_Text>();
            serialized.FindProperty("confirmCraftButton").objectReferenceValue =
                root.Find("PreviewArea/ConfirmCraftButton")?.GetComponent<Button>();
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null)
            {
                go.layer = parent.gameObject.layer;
                go.transform.SetParent(parent, false);
            }

            return go;
        }

        static void StretchFull(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        static void SetAnchors(RectTransform rectTransform, Vector2 min, Vector2 max)
        {
            rectTransform.anchorMin = min;
            rectTransform.anchorMax = max;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        static TMP_Text CreatePreviewText(
            Transform parent,
            string name,
            int fontSize,
            FontStyles fontStyle,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            var text = CreateLabel(parent, name, fontSize, TextAlignmentOptions.TopLeft);
            text.fontStyle = fontStyle;
            var rectTransform = text.GetComponent<RectTransform>();
            SetAnchors(rectTransform, anchorMin, anchorMax);
            return text;
        }

        static TMP_Text CreateLabel(Transform parent, string name, int fontSize, TextAlignmentOptions alignment)
        {
            var go = CreateUiObject(name, parent);
            StretchFull(go.GetComponent<RectTransform>());
            var text = go.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        static Button CreateButton(
            Transform parent,
            string name,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            var go = CreateUiObject(name, parent);
            var rectTransform = go.GetComponent<RectTransform>();
            SetAnchors(rectTransform, anchorMin, anchorMax);

            var image = go.AddComponent<Image>();
            image.color = new Color(0.85f, 0.75f, 0.45f, 1f);

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;

            var text = CreateLabel(go.transform, "Text", 18, TextAlignmentOptions.Center);
            text.text = label;
            return button;
        }
    }
}

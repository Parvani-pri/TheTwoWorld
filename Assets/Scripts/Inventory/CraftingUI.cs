using System.Collections.Generic;
using System.Text;
using TMPro;
using TwoWorlds.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TwoWorlds.Inventory
{
    public class CraftingUI : MonoBehaviour
    {
        [SerializeField] PlayerInventory playerInventory;
        [SerializeField] CraftRecipeDatabase recipeDatabase;
        [SerializeField] Button craftOpenButton;
        [SerializeField] Button craftCloseButton;
        [SerializeField] GameObject craftPanelRoot;
        [SerializeField] Transform recipeListContainer;
        [SerializeField] CraftRecipeRowUI recipeRowPrefab;
        [SerializeField] TMP_Text previewNameText;
        [SerializeField] TMP_Text previewIngredientsText;
        [SerializeField] TMP_Text previewOutputText;
        [SerializeField] TMP_Text previewStatusText;
        [SerializeField] Button confirmCraftButton;

        readonly List<CraftRecipeRowUI> recipeRows = new();
        CraftRecipeData selectedRecipe;
        bool isPanelOpen;

        void Awake()
        {
            if (craftPanelRoot != null)
                craftPanelRoot.SetActive(false);
        }

        void Start()
        {
            if (playerInventory == null)
                playerInventory = FindFirstObjectByType<PlayerInventory>();

            if (craftOpenButton != null)
                craftOpenButton.onClick.AddListener(TogglePanel);

            if (craftCloseButton != null)
                craftCloseButton.onClick.AddListener(() => SetPanelOpen(false));

            if (confirmCraftButton != null)
                confirmCraftButton.onClick.AddListener(OnConfirmCraft);

            SetPanelOpen(false);
            ClearPreview();
        }

        void OnEnable()
        {
            GameEvents.InventoryChanged += OnInventoryChanged;
            GameEvents.InventoryOpenChanged += OnInventoryOpenChanged;
        }

        void OnDisable()
        {
            GameEvents.InventoryChanged -= OnInventoryChanged;
            GameEvents.InventoryOpenChanged -= OnInventoryOpenChanged;

            if (isPanelOpen)
                SetPanelOpen(false);
        }

        void OnDestroy()
        {
            if (craftOpenButton != null)
                craftOpenButton.onClick.RemoveAllListeners();

            if (craftCloseButton != null)
                craftCloseButton.onClick.RemoveAllListeners();

            if (confirmCraftButton != null)
                confirmCraftButton.onClick.RemoveAllListeners();
        }

        public void TogglePanel()
        {
            SetPanelOpen(!isPanelOpen);
        }

        void SetPanelOpen(bool open)
        {
            if (isPanelOpen == open)
                return;

            isPanelOpen = open;

            if (craftPanelRoot != null)
                craftPanelRoot.SetActive(open);

            if (open)
            {
                selectedRecipe = null;
                RefreshRecipeList();
                ClearPreview();
            }
        }

        void OnInventoryOpenChanged(bool isOpen)
        {
            if (!isOpen)
                SetPanelOpen(false);
        }

        void OnInventoryChanged(PlayerInventory _) => RefreshAll();

        void RefreshAll()
        {
            if (!isPanelOpen)
                return;

            RefreshRecipeList();
            RefreshPreview();
        }

        void RefreshRecipeList()
        {
            ClearRecipeRows();

            if (recipeListContainer == null || recipeRowPrefab == null || recipeDatabase == null)
                return;

            foreach (var recipe in recipeDatabase.Recipes)
            {
                if (recipe == null || !recipe.IsValid)
                    continue;

                var row = Instantiate(recipeRowPrefab, recipeListContainer);
                var canCraft = CraftingService.CanCraft(recipe, playerInventory);
                var isSelected = selectedRecipe == recipe;
                row.Bind(recipe, canCraft, isSelected, OnRecipeSelected);
                recipeRows.Add(row);
            }
        }

        void ClearRecipeRows()
        {
            foreach (var row in recipeRows)
            {
                if (row != null)
                    Destroy(row.gameObject);
            }

            recipeRows.Clear();
        }

        void OnRecipeSelected(CraftRecipeData recipe)
        {
            selectedRecipe = recipe;
            RefreshRecipeList();
            RefreshPreview();
        }

        void RefreshPreview()
        {
            if (selectedRecipe == null)
            {
                ClearPreview();
                return;
            }

            if (previewNameText != null)
                previewNameText.text = selectedRecipe.DisplayName;

            if (previewIngredientsText != null)
                previewIngredientsText.text = BuildIngredientsText(selectedRecipe);

            if (previewOutputText != null)
                previewOutputText.text = BuildOutputText(selectedRecipe);

            var canCraft = CraftingService.CanCraft(selectedRecipe, playerInventory);
            var statusMessage = BuildStatusMessage(selectedRecipe, canCraft);

            if (previewStatusText != null)
                previewStatusText.text = statusMessage;

            if (confirmCraftButton != null)
                confirmCraftButton.interactable = canCraft;
        }

        void ClearPreview()
        {
            if (previewNameText != null)
                previewNameText.text = string.Empty;

            if (previewIngredientsText != null)
                previewIngredientsText.text = string.Empty;

            if (previewOutputText != null)
                previewOutputText.text = string.Empty;

            if (previewStatusText != null)
                previewStatusText.text = "选择一条配方";

            if (confirmCraftButton != null)
                confirmCraftButton.interactable = false;
        }

        string BuildIngredientsText(CraftRecipeData recipe)
        {
            if (playerInventory == null)
                return string.Empty;

            var builder = new StringBuilder();
            builder.AppendLine("材料：");

            foreach (var ingredient in recipe.Ingredients)
            {
                var owned = playerInventory.GetItemCount(ingredient.Item);
                var mark = owned >= ingredient.Amount ? "✓" : "✗";
                builder.AppendLine($"{mark} {ingredient.Item.DisplayName}  {owned}/{ingredient.Amount}");
            }

            return builder.ToString().TrimEnd();
        }

        static string BuildOutputText(CraftRecipeData recipe)
        {
            if (recipe.OutputItem == null)
                return string.Empty;

            return $"产出：{recipe.OutputItem.DisplayName} ×{recipe.OutputAmount}";
        }

        string BuildStatusMessage(CraftRecipeData recipe, bool canCraft)
        {
            if (canCraft)
                return "可以合成";

            if (!CraftingService.HasIngredients(recipe, playerInventory))
                return "材料不足";

            if (playerInventory != null && !playerInventory.CanAddItem(recipe.OutputItem, recipe.OutputAmount))
                return "背包空间不足";

            return "无法合成";
        }

        void OnConfirmCraft()
        {
            if (selectedRecipe == null || playerInventory == null)
                return;

            var result = CraftingService.TryCraft(selectedRecipe, playerInventory);

            switch (result)
            {
                case CraftResult.Success:
                    GameEvents.RaiseCraftCompleted(selectedRecipe);
                    if (previewStatusText != null)
                        previewStatusText.text = "合成成功";
                    RefreshAll();
                    break;

                case CraftResult.MissingIngredients:
                    GameEvents.RaiseCraftFailed(selectedRecipe, result);
                    if (previewStatusText != null)
                        previewStatusText.text = "材料不足";
                    RefreshAll();
                    break;

                case CraftResult.InventoryFull:
                    GameEvents.RaiseCraftFailed(selectedRecipe, result);
                    if (previewStatusText != null)
                        previewStatusText.text = "背包空间不足";
                    RefreshAll();
                    break;

                default:
                    GameEvents.RaiseCraftFailed(selectedRecipe, result);
                    if (previewStatusText != null)
                        previewStatusText.text = "配方无效";
                    break;
            }
        }
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TwoWorlds.Inventory
{
    public class CraftRecipeRowUI : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] TMP_Text labelText;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Image backgroundImage;
        [SerializeField] Color selectedColor = new(0.85f, 0.75f, 0.45f, 0.35f);
        [SerializeField] Color normalColor = new(1f, 1f, 1f, 0.08f);
        [SerializeField] float disabledAlpha = 0.45f;

        CraftRecipeData recipe;
        Action<CraftRecipeData> onSelected;

        void Awake()
        {
            if (button != null)
                button.onClick.AddListener(OnClicked);
        }

        void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnClicked);
        }

        public void Bind(
            CraftRecipeData recipe,
            bool canCraft,
            bool isSelected,
            Action<CraftRecipeData> onSelected)
        {
            this.recipe = recipe;
            this.onSelected = onSelected;

            if (labelText != null)
                labelText.text = recipe != null ? recipe.DisplayName : string.Empty;

            if (canvasGroup != null)
                canvasGroup.alpha = canCraft ? 1f : disabledAlpha;

            if (backgroundImage != null)
                backgroundImage.color = isSelected ? selectedColor : normalColor;
        }

        void OnClicked()
        {
            if (recipe != null)
                onSelected?.Invoke(recipe);
        }
    }
}

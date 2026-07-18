using UnityEngine;

namespace TwoWorlds.Inventory
{
    [CreateAssetMenu(fileName = "NewCraftRecipe", menuName = "Two Worlds/Craft Recipe")]
    public class CraftRecipeData : ScriptableObject
    {
        [SerializeField] string recipeId;
        [SerializeField] string displayName;
        [SerializeField] CraftIngredient[] ingredients;
        [SerializeField] ItemData outputItem;
        [SerializeField] int outputAmount = 1;

        public string RecipeId => string.IsNullOrWhiteSpace(recipeId) ? name : recipeId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public CraftIngredient[] Ingredients => ingredients ?? System.Array.Empty<CraftIngredient>();
        public ItemData OutputItem => outputItem;
        public int OutputAmount => Mathf.Max(1, outputAmount);

        public bool IsValid
        {
            get
            {
                if (outputItem == null || outputAmount <= 0)
                    return false;

                var ingredientList = Ingredients;
                if (ingredientList.Length == 0)
                    return false;

                foreach (var ingredient in ingredientList)
                {
                    if (!ingredient.IsValid)
                        return false;
                }

                return true;
            }
        }

        void OnValidate()
        {
            outputAmount = Mathf.Max(1, outputAmount);
        }
    }
}

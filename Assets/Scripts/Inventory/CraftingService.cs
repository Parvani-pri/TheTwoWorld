namespace TwoWorlds.Inventory
{
    public static class CraftingService
    {
        public static bool HasIngredients(CraftRecipeData recipe, PlayerInventory inventory)
        {
            if (!IsRecipeValid(recipe) || inventory == null)
                return false;

            foreach (var ingredient in recipe.Ingredients)
            {
                if (!inventory.HasItem(ingredient.Item, ingredient.Amount))
                    return false;
            }

            return true;
        }

        public static bool CanCraft(CraftRecipeData recipe, PlayerInventory inventory)
        {
            if (!IsRecipeValid(recipe) || inventory == null)
                return false;

            if (!HasIngredients(recipe, inventory))
                return false;

            return inventory.CanAddItem(recipe.OutputItem, recipe.OutputAmount);
        }

        public static CraftResult TryCraft(CraftRecipeData recipe, PlayerInventory inventory)
        {
            if (!IsRecipeValid(recipe) || inventory == null)
                return CraftResult.InvalidRecipe;

            if (!HasIngredients(recipe, inventory))
                return CraftResult.MissingIngredients;

            if (!inventory.CanAddItem(recipe.OutputItem, recipe.OutputAmount))
                return CraftResult.InventoryFull;

            foreach (var ingredient in recipe.Ingredients)
            {
                if (!inventory.RemoveItem(ingredient.Item, ingredient.Amount))
                    return CraftResult.MissingIngredients;
            }

            var addResult = inventory.AddItem(recipe.OutputItem, recipe.OutputAmount);
            if (!addResult.IsFullSuccess)
                return CraftResult.InventoryFull;

            return CraftResult.Success;
        }

        static bool IsRecipeValid(CraftRecipeData recipe) => recipe != null && recipe.IsValid;
    }
}

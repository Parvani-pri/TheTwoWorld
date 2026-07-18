using UnityEngine;

namespace TwoWorlds.Inventory
{
    [CreateAssetMenu(fileName = "CraftRecipeDatabase", menuName = "Two Worlds/Craft Recipe Database")]
    public class CraftRecipeDatabase : ScriptableObject
    {
        [SerializeField] CraftRecipeData[] recipes;

        public CraftRecipeData[] Recipes => recipes ?? System.Array.Empty<CraftRecipeData>();
    }
}

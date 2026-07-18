using System;
using UnityEngine;

namespace TwoWorlds.Inventory
{
    [Serializable]
    public struct CraftIngredient
    {
        [SerializeField] ItemData item;
        [SerializeField] int amount;

        public ItemData Item => item;
        public int Amount => Mathf.Max(1, amount);

        public bool IsValid => item != null && amount > 0;
    }
}

using System;
using TwoWorlds.Inventory;
using UnityEngine;

namespace TwoWorlds.Combat
{
    [Serializable]
    public struct LootEntry
    {
        [SerializeField] ItemData item;
        [SerializeField] LootAmountMode amountMode;
        [SerializeField] int amount;
        [SerializeField] int minAmount;
        [SerializeField] int maxAmount;
        [SerializeField] float dropChance;

        public ItemData Item => item;
        public bool IsValid => item != null && GetDropChance() > 0f;

        public int RollAmount()
        {
            if (item == null)
                return 0;

            if (amountMode == LootAmountMode.RandomRange)
            {
                var min = Mathf.Max(1, minAmount);
                var max = Mathf.Max(min, maxAmount);
                return UnityEngine.Random.Range(min, max + 1);
            }

            return Mathf.Max(1, amount);
        }

        public bool RollDrop()
        {
            if (!IsValid)
                return false;

            var chance = GetDropChance();
            return chance >= 1f || UnityEngine.Random.value <= chance;
        }

        float GetDropChance() => Mathf.Clamp01(dropChance <= 0f ? 1f : dropChance);
    }
}

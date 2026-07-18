using System;
using System.Collections.Generic;
using UnityEngine;

namespace TwoWorlds.Inventory
{
    [CreateAssetMenu(fileName = "ItemDataCatalog", menuName = "Two Worlds/Item Data Catalog")]
    public class ItemDataCatalog : ScriptableObject
    {
        [SerializeField] ItemData[] items;

        Dictionary<string, ItemData> lookup;

        public bool TryGetItem(string itemId, out ItemData item)
        {
            item = null;
            if (string.IsNullOrWhiteSpace(itemId))
                return false;

            EnsureLookup();
            return lookup.TryGetValue(itemId.Trim(), out item) && item != null;
        }

        void EnsureLookup()
        {
            if (lookup != null)
                return;

            lookup = new Dictionary<string, ItemData>(StringComparer.OrdinalIgnoreCase);
            if (items == null)
                return;

            foreach (var entry in items)
            {
                if (entry == null)
                    continue;

                lookup[entry.ItemId] = entry;
            }
        }

        void OnValidate() => lookup = null;
    }
}

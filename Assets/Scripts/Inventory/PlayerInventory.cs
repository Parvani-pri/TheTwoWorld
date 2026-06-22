using System;
using System.Collections.Generic;
using TwoWorlds.Core;
using UnityEngine;

namespace TwoWorlds.Inventory
{
    [Serializable]
    public struct InventorySlot
    {
        public ItemData item;
        public int quantity;

        public bool IsEmpty => item == null || quantity <= 0;
    }

    public class PlayerInventory : MonoBehaviour
    {
        [SerializeField] int capacity = 20;
        [SerializeField] List<InventorySlot> slots = new();

        public int Capacity => capacity;
        public IReadOnlyList<InventorySlot> Slots => slots;

        void Awake()
        {
            EnsureCapacity();
        }

        void EnsureCapacity()
        {
            while (slots.Count < capacity)
                slots.Add(default);
        }

        public bool AddItem(ItemData item, int amount = 1)
        {
            if (item == null || amount <= 0)
                return false;

            EnsureCapacity();
            var remaining = amount;

            if (!item.IsUnique)
            {
                remaining = TryStackExisting(item, remaining);
                if (remaining <= 0)
                {
                    NotifyChanged();
                    return true;
                }
            }

            while (remaining > 0)
            {
                var emptyIndex = FindEmptySlotIndex();
                if (emptyIndex < 0)
                    return false;

                var stackAmount = item.IsUnique ? 1 : Mathf.Min(remaining, item.MaxStackSize);
                slots[emptyIndex] = new InventorySlot { item = item, quantity = stackAmount };
                remaining -= stackAmount;
            }

            NotifyChanged();
            return true;
        }

        int TryStackExisting(ItemData item, int amount)
        {
            var remaining = amount;

            for (var i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot.IsEmpty || slot.item != item)
                    continue;

                var space = item.MaxStackSize - slot.quantity;
                if (space <= 0)
                    continue;

                var toAdd = Mathf.Min(space, remaining);
                slot.quantity += toAdd;
                slots[i] = slot;
                remaining -= toAdd;

                if (remaining <= 0)
                    break;
            }

            return remaining;
        }

        public bool RemoveItem(ItemData item, int amount = 1)
        {
            if (item == null || amount <= 0 || !HasItem(item, amount))
                return false;

            var remaining = amount;

            for (var i = slots.Count - 1; i >= 0 && remaining > 0; i--)
            {
                var slot = slots[i];
                if (slot.IsEmpty || slot.item != item)
                    continue;

                var removeCount = Mathf.Min(slot.quantity, remaining);
                slot.quantity -= removeCount;
                remaining -= removeCount;

                if (slot.quantity <= 0)
                    slot = default;

                slots[i] = slot;
            }

            NotifyChanged();
            return true;
        }

        public bool HasItem(ItemData item, int amount = 1)
        {
            if (item == null || amount <= 0)
                return false;

            var total = 0;
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty && slot.item == item)
                    total += slot.quantity;
            }

            return total >= amount;
        }

        public int GetItemCount(ItemData item)
        {
            if (item == null)
                return 0;

            var total = 0;
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty && slot.item == item)
                    total += slot.quantity;
            }

            return total;
        }

        int FindEmptySlotIndex()
        {
            for (var i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsEmpty)
                    return i;
            }

            return -1;
        }

        void NotifyChanged() => GameEvents.RaiseInventoryChanged(this);
    }
}

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

        public InventoryAddResult AddItem(ItemData item, int amount = 1)
        {
            if (item == null || amount <= 0)
                return InventoryAddResult.Failed(amount);

            EnsureCapacity();
            var remaining = amount;

            if (!item.IsUnique)
                remaining = TryStackExisting(item, remaining);

            while (remaining > 0)
            {
                var emptyIndex = FindEmptySlotIndex();
                if (emptyIndex < 0)
                    break;

                var stackAmount = item.IsUnique ? 1 : Mathf.Min(remaining, item.MaxStackSize);
                slots[emptyIndex] = new InventorySlot { item = item, quantity = stackAmount };
                remaining -= stackAmount;
            }

            var addedAmount = amount - remaining;
            InventoryAddResult result;

            if (remaining <= 0)
                result = InventoryAddResult.Success(addedAmount);
            else if (addedAmount > 0)
                result = InventoryAddResult.Partial(addedAmount, amount);
            else
                result = InventoryAddResult.Failed(amount);

            if (addedAmount > 0)
                NotifyChanged();

            GameEvents.RaiseInventoryAddResult(result, item);
            return result;
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

        public bool TryTakeFromSlot(int slotIndex, int amount, out ItemData item, out int taken)
        {
            item = null;
            taken = 0;

            if (slotIndex < 0 || slotIndex >= slots.Count || amount <= 0)
                return false;

            var slot = slots[slotIndex];
            if (slot.IsEmpty)
                return false;

            taken = Mathf.Min(amount, slot.quantity);
            item = slot.item;
            slot.quantity -= taken;

            if (slot.quantity <= 0)
                slot = default;

            slots[slotIndex] = slot;
            NotifyChanged();
            return true;
        }

        public InventorySlot GetSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count)
                return default;

            return slots[slotIndex];
        }

        public bool SwapSlots(int fromIndex, int toIndex) => MoveSlot(fromIndex, toIndex, allowSwap: true);

        public bool MoveSlot(int fromIndex, int toIndex, bool allowSwap = true)
        {
            if (fromIndex == toIndex ||
                fromIndex < 0 || fromIndex >= slots.Count ||
                toIndex < 0 || toIndex >= slots.Count)
                return false;

            var from = slots[fromIndex];
            if (from.IsEmpty)
                return false;

            var to = slots[toIndex];

            if (to.IsEmpty)
            {
                slots[toIndex] = from;
                slots[fromIndex] = default;
                NotifyChanged();
                return true;
            }

            if (!from.item.IsUnique && from.item == to.item)
            {
                var space = to.item.MaxStackSize - to.quantity;
                if (space > 0)
                {
                    var moveAmount = Mathf.Min(space, from.quantity);
                    to.quantity += moveAmount;
                    from.quantity -= moveAmount;

                    if (from.quantity <= 0)
                        from = default;

                    slots[fromIndex] = from;
                    slots[toIndex] = to;
                    NotifyChanged();
                    return true;
                }
            }

            if (!allowSwap)
                return false;

            slots[fromIndex] = to;
            slots[toIndex] = from;
            NotifyChanged();
            return true;
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

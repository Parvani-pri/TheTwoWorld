using System;
using TwoWorlds.Inventory;

namespace TwoWorlds.Core
{
    public static class GameEvents
    {
        public static event Action<PlayerInventory> InventoryChanged;
        public static event Action<InventoryAddResult, ItemData> InventoryAddCompleted;
        public static event Action<bool> InventoryOpenChanged;
        public static event Action DialogueStarted;
        public static event Action DialogueEnded;
        public static event Action<bool> GameplayInputBlocked;

        public static bool IsInventoryOpen { get; private set; }

        public static void RaiseInventoryChanged(PlayerInventory inventory) =>
            InventoryChanged?.Invoke(inventory);

        public static void RaiseInventoryAddResult(InventoryAddResult result, ItemData item) =>
            InventoryAddCompleted?.Invoke(result, item);

        public static void RaiseInventoryOpenChanged(bool isOpen)
        {
            IsInventoryOpen = isOpen;
            InventoryOpenChanged?.Invoke(isOpen);
        }

        public static void RaiseDialogueStarted()
        {
            GameplayInputBlocked?.Invoke(true);
            DialogueStarted?.Invoke();
        }

        public static void RaiseDialogueEnded()
        {
            GameplayInputBlocked?.Invoke(false);
            DialogueEnded?.Invoke();
        }
    }
}

using System;
using TwoWorlds.Inventory;

namespace TwoWorlds.Core
{
    public static class GameEvents
    {
        public static event Action<PlayerInventory> InventoryChanged;
        public static event Action DialogueStarted;
        public static event Action DialogueEnded;
        public static event Action<bool> GameplayInputBlocked;

        public static void RaiseInventoryChanged(PlayerInventory inventory) =>
            InventoryChanged?.Invoke(inventory);

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

using System;
using TwoWorlds.Combat;
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

        public static event Action CombatStarted;
        public static event Action<CombatResult> CombatEnded;
        public static event Action<CombatHealth, int, CombatActor> ActorDamaged;
        public static event Action<CombatHealth> ActorDied;
        public static event Action<CombatHealth, int, int> ActorHealthChanged;

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

        public static void RaiseCombatStarted() => CombatStarted?.Invoke();

        public static void RaiseCombatEnded(CombatResult result) => CombatEnded?.Invoke(result);

        public static void RaiseActorDamaged(CombatHealth health, int amount, CombatActor source) =>
            ActorDamaged?.Invoke(health, amount, source);

        public static void RaiseActorDied(CombatHealth health) => ActorDied?.Invoke(health);

        public static void RaiseActorHealthChanged(CombatHealth health, int current, int max) =>
            ActorHealthChanged?.Invoke(health, current, max);
    }
}

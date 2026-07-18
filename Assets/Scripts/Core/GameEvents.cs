using System;
using TwoWorlds.Combat;
using TwoWorlds.Inventory;
using UnityEngine.UI;

namespace TwoWorlds.Core
{
    public static class GameEvents
    {
        public static event Action<PlayerInventory> InventoryChanged;
        public static event Action<InventoryAddResult, ItemData> InventoryAddCompleted;
        public static event Action<bool> InventoryOpenChanged;
        public static event Action<CraftRecipeData> CraftCompleted;
        public static event Action<CraftRecipeData, CraftResult> CraftFailed;
        public static event Action DialogueStarted;
        public static event Action DialogueEnded;
        public static event Action<DialogueEndInfo> ScriptDialogueEnded;
        public static event Action<DialogueLineShownInfo> DialogueLineShown;
        public static event Action<bool> GameplayInputBlocked;

        public static event Action CombatStarted;
        public static event Action<CombatResult> CombatEnded;
        public static event Action<CombatHealth, int, CombatActor> ActorDamaged;
        public static event Action<CombatHealth> ActorDied;
        public static event Action<CombatHealth, int, int> ActorHealthChanged;

        public static event Action<float, float> OnActorSprint;
        public static event Action<float, float> OnActorFlight;

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

        public static void RaiseCraftCompleted(CraftRecipeData recipe) =>
            CraftCompleted?.Invoke(recipe);

        public static void RaiseCraftFailed(CraftRecipeData recipe, CraftResult result) =>
            CraftFailed?.Invoke(recipe, result);

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

        public static void RaiseDialogueEnded(DialogueEndInfo info)
        {
            GameplayInputBlocked?.Invoke(false);
            DialogueEnded?.Invoke();

            if (info.HasInteractor)
                ScriptDialogueEnded?.Invoke(info);
        }

        public static void RaiseDialogueLineShown(DialogueLineShownInfo info) =>
            DialogueLineShown?.Invoke(info);

        public static void RaiseCombatStarted() => CombatStarted?.Invoke();

        public static void RaiseCombatEnded(CombatResult result) => CombatEnded?.Invoke(result);

        public static void RaiseActorDamaged(CombatHealth health, int amount, CombatActor source) =>
            ActorDamaged?.Invoke(health, amount, source);

        public static void RaiseActorDied(CombatHealth health) => ActorDied?.Invoke(health);

        public static void RaiseActorHealthChanged(CombatHealth health, int current, int max) =>
            ActorHealthChanged?.Invoke(health, current, max);

        public static void UpdateSprintBar(float target, float max)
        {
            OnActorSprint?.Invoke(target, max);
        }

        public static void UpdateFlightBar(float target, float max)
        {
            OnActorFlight?.Invoke(target, max);
        }

    }
}

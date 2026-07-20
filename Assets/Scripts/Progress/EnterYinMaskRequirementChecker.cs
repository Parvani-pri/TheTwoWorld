using System.Collections.Generic;
using TwoWorlds.Inventory;

namespace TwoWorlds.Progress
{
    public readonly struct EnterYinMaskCheckResult
    {
        public bool CanEnter { get; }
        public IReadOnlyList<string> MissingItemIds { get; }

        public EnterYinMaskCheckResult(bool canEnter, IReadOnlyList<string> missingItemIds)
        {
            CanEnter = canEnter;
            MissingItemIds = missingItemIds ?? System.Array.Empty<string>();
        }
    }

    public static class EnterYinMaskRequirementChecker
    {
        public static EnterYinMaskCheckResult Evaluate(int chapter, PlayerInventory inventory)
        {
            var requiredItemIds = EnterYinMaskRequirements.GetRequiredItemIds(chapter);
            if (requiredItemIds.Count == 0)
                return new EnterYinMaskCheckResult(true, System.Array.Empty<string>());

            if (inventory == null)
                return new EnterYinMaskCheckResult(false, requiredItemIds);

            var missingItemIds = new List<string>();
            foreach (var itemId in requiredItemIds)
            {
                if (!inventory.HasItemId(itemId))
                    missingItemIds.Add(itemId);
            }

            return new EnterYinMaskCheckResult(missingItemIds.Count == 0, missingItemIds);
        }
    }
}

using System.Collections.Generic;
using TwoWorlds.Inventory;
using UnityEngine;

namespace TwoWorlds.Dialogue
{
    /// <summary>
    /// Runtime dialogue payload shared by ScriptableObject assets and CSV sources.
    /// </summary>
    public sealed class DialogueSessionData
    {
        public string DialogueId { get; }
        public IReadOnlyList<DialogueLine> Lines { get; }
        public bool PlayOnce { get; }
        public ItemData RewardItem { get; }
        public int RewardAmount { get; }
        public string ProgressNote { get; }

        public DialogueSessionData(
            string dialogueId,
            IReadOnlyList<DialogueLine> lines,
            bool playOnce = false,
            ItemData rewardItem = null,
            int rewardAmount = 1,
            string progressNote = null)
        {
            DialogueId = dialogueId ?? string.Empty;
            Lines = lines ?? System.Array.Empty<DialogueLine>();
            PlayOnce = playOnce;
            RewardItem = rewardItem;
            RewardAmount = Mathf.Max(1, rewardAmount);
            ProgressNote = progressNote ?? string.Empty;
        }

        public static DialogueSessionData FromAsset(DialogueData asset)
        {
            if (asset == null)
                return null;

            return new DialogueSessionData(
                asset.name,
                asset.Lines,
                asset.PlayOnce,
                asset.RewardItem,
                asset.RewardAmount,
                asset.ProgressNote);
        }
    }
}

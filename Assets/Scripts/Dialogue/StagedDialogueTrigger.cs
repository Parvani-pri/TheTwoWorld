using System;
using TwoWorlds.Core;
using TwoWorlds.Inventory;
using TwoWorlds.Progress;
using UnityEngine;

namespace TwoWorlds.Dialogue
{
    /// <summary>
    /// Multi-stage dialogue selector on one NPC.
    /// For chapter story beats, prefer <see cref="ChapterDialogueTrigger"/> plus <see cref="GameProgress"/> segment flags.
    /// </summary>
    [Serializable]
    public class DialogueStageEntry
    {
        [Tooltip("CSV dialogue_id to play when this stage is selected.")]
        public string csvDialogueId;

        [Header("Story Prerequisites")]
        [Tooltip("Must have completed this dialogue. This alone does NOT auto-unlock the next interaction.")]
        public string requiresDialogueId;

        [Tooltip("Optional. All listed dialogues must be completed.")]
        public string[] requiresAllDialogueIds;

        [Tooltip("Skip this stage if this dialogue was already completed.")]
        public string blockedByDialogueId;

        [Header("Explicit Unlock Gates")]
        [Tooltip("Stage stays locked until this flag is set by gameplay (battle, item, action:setflag, etc.).")]
        public string requiresUnlockFlag;

        [Tooltip("Skip if this flag is set.")]
        public string blockedByFlag;

        [Header("Player / World State")]
        [Tooltip("Optional. Player must carry this item.")]
        public ItemData requiredItem;

        [Min(1)]
        public int requiredItemAmount = 1;

        [Tooltip("Optional. Must match GameProgress.CurrentStageLabel exactly.")]
        public string requiredStageLabel;

        [Header("Playback")]
        [Tooltip("If true, skip once csvDialogueId is already in GameProgress.")]
        public bool playOnce = true;
    }

    [RequireComponent(typeof(Collider2D))]
    public class StagedDialogueTrigger : MonoBehaviour, IInteractable, IScriptDialogueSource
    {
        [SerializeField] DialogueCsvLibrary csvLibrary;
        [SerializeField] DialogueStageEntry[] stages;
        [SerializeField] GameProgress gameProgress;
        [SerializeField] string promptText = "对话";

        void Awake()
        {
            if (gameProgress == null)
                gameProgress = GameProgress.Instance ?? FindFirstObjectByType<GameProgress>();
        }

        public bool CanInteract(GameObject interactor) => IsDialogueAvailable(interactor);

        public bool IsDialogueAvailable(GameObject interactor) => ResolveStage(interactor) != null;

        public void Interact(GameObject interactor) => TriggerDialogue(interactor);

        public void TriggerDialogue(GameObject interactor)
        {
            var stage = ResolveStage(interactor);
            if (stage == null || csvLibrary == null)
                return;

            if (!csvLibrary.TryGetDialogue(stage.csvDialogueId, out var sessionData))
            {
                Debug.LogWarning($"[StagedDialogueTrigger] Missing CSV dialogue: {stage.csvDialogueId}");
                return;
            }

            if (DialogueManager.Instance != null)
                DialogueManager.Instance.StartDialogue(sessionData, interactor);
        }

        public string GetPromptText() => promptText;

        DialogueStageEntry ResolveStage(GameObject interactor)
        {
            if (csvLibrary == null || stages == null || stages.Length == 0)
                return null;

            foreach (var stage in stages)
            {
                if (stage == null || string.IsNullOrWhiteSpace(stage.csvDialogueId))
                    continue;

                if (!csvLibrary.TryGetDialogue(stage.csvDialogueId, out _))
                    continue;

                if (!MeetsRequirements(stage, interactor))
                    continue;

                if (stage.playOnce && HasCompletedDialogue(stage.csvDialogueId))
                    continue;

                return stage;
            }

            return null;
        }

        bool MeetsRequirements(DialogueStageEntry stage, GameObject interactor)
        {
            if (!string.IsNullOrWhiteSpace(stage.requiresDialogueId) &&
                !HasCompletedDialogue(stage.requiresDialogueId))
                return false;

            if (stage.requiresAllDialogueIds != null)
            {
                foreach (var dialogueId in stage.requiresAllDialogueIds)
                {
                    if (!string.IsNullOrWhiteSpace(dialogueId) && !HasCompletedDialogue(dialogueId))
                        return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(stage.blockedByDialogueId) &&
                HasCompletedDialogue(stage.blockedByDialogueId))
                return false;

            if (!string.IsNullOrWhiteSpace(stage.requiresUnlockFlag) &&
                !HasFlag(stage.requiresUnlockFlag))
                return false;

            if (!string.IsNullOrWhiteSpace(stage.blockedByFlag) &&
                HasFlag(stage.blockedByFlag))
                return false;

            if (!string.IsNullOrWhiteSpace(stage.requiredStageLabel) &&
                !MatchesStageLabel(stage.requiredStageLabel))
                return false;

            if (stage.requiredItem != null && !HasRequiredItem(interactor, stage))
                return false;

            return true;
        }

        bool HasRequiredItem(GameObject interactor, DialogueStageEntry stage)
        {
            if (interactor == null)
                return false;

            var inventory = interactor.GetComponent<PlayerInventory>();
            return inventory != null &&
                   inventory.HasItem(stage.requiredItem, stage.requiredItemAmount);
        }

        bool MatchesStageLabel(string requiredLabel)
        {
            if (gameProgress == null || string.IsNullOrWhiteSpace(requiredLabel))
                return false;

            return string.Equals(
                gameProgress.CurrentStageLabel?.Trim(),
                requiredLabel.Trim(),
                StringComparison.Ordinal);
        }

        bool HasCompletedDialogue(string dialogueId)
        {
            if (gameProgress == null)
                return false;

            return gameProgress.HasDialogue(dialogueId);
        }

        bool HasFlag(string flagId)
        {
            if (gameProgress == null)
                return false;

            return gameProgress.HasFlag(flagId);
        }
    }
}

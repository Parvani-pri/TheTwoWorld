using TwoWorlds.Core;
using TwoWorlds.Inventory;
using UnityEngine;

namespace TwoWorlds.Dialogue
{
    public enum DialogueSourceMode
    {
        Asset,
        Csv
    }

    [RequireComponent(typeof(Collider2D))]
    public class DialogueTrigger : MonoBehaviour, IInteractable, IScriptDialogueSource
    {
        [SerializeField] DialogueSourceMode sourceMode = DialogueSourceMode.Asset;
        [SerializeField] DialogueData dialogue;
        [SerializeField] DialogueData fallbackDialogue;
        [SerializeField] DialogueCsvLibrary csvLibrary;
        [SerializeField] string csvDialogueId;
        [SerializeField] string fallbackCsvDialogueId;
        [SerializeField] ItemData requiredItem;
        [SerializeField] int requiredAmount = 1;
        [SerializeField] bool consumeRequiredItem;
        [SerializeField] bool requireInteractHub;
        [SerializeField] string promptText = "對話";

        bool hasPlayedOnce;

        public bool CanInteract(GameObject interactor)
        {
            if (requireInteractHub)
                return false;

            return IsDialogueAvailable(interactor);
        }

        public bool IsDialogueAvailable(GameObject interactor)
        {
            if (!HasPrimaryDialogue())
                return false;

            if (ShouldPlayOnce() && hasPlayedOnce)
                return false;

            return true;
        }

        public void Interact(GameObject interactor) => TriggerDialogue(interactor);

        public void TriggerDialogue(GameObject interactor)
        {
            var inventory = interactor.GetComponent<PlayerInventory>();
            var selectedDialogue = ResolveDialogue(inventory);

            if (selectedDialogue == null)
                return;

            if (requiredItem != null && inventory != null && inventory.HasItem(requiredItem, requiredAmount))
            {
                if (consumeRequiredItem)
                    inventory.RemoveItem(requiredItem, requiredAmount);
            }

            if (selectedDialogue.PlayOnce)
                hasPlayedOnce = true;

            if (DialogueManager.Instance != null)
                DialogueManager.Instance.StartDialogue(selectedDialogue, interactor);
        }

        DialogueSessionData ResolveDialogue(PlayerInventory inventory)
        {
            if (requiredItem != null)
            {
                var hasItem = inventory != null && inventory.HasItem(requiredItem, requiredAmount);
                return hasItem ? ResolvePrimaryDialogue() : ResolveFallbackDialogue();
            }

            return ResolvePrimaryDialogue();
        }

        DialogueSessionData ResolvePrimaryDialogue() =>
            sourceMode == DialogueSourceMode.Asset
                ? DialogueSessionData.FromAsset(dialogue)
                : ResolveCsvDialogue(csvDialogueId);

        DialogueSessionData ResolveFallbackDialogue()
        {
            if (sourceMode == DialogueSourceMode.Asset)
                return DialogueSessionData.FromAsset(fallbackDialogue);

            return ResolveCsvDialogue(fallbackCsvDialogueId);
        }

        DialogueSessionData ResolveCsvDialogue(string dialogueId)
        {
            if (csvLibrary == null || string.IsNullOrWhiteSpace(dialogueId))
                return null;

            return csvLibrary.TryGetDialogue(dialogueId, out var sessionData) ? sessionData : null;
        }

        bool HasPrimaryDialogue()
        {
            if (sourceMode == DialogueSourceMode.Asset)
                return dialogue != null;

            return csvLibrary != null && !string.IsNullOrWhiteSpace(csvDialogueId);
        }

        bool ShouldPlayOnce()
        {
            if (sourceMode == DialogueSourceMode.Asset)
                return dialogue != null && dialogue.PlayOnce;

            if (csvLibrary != null && csvLibrary.TryGetDialogue(csvDialogueId, out var sessionData))
                return sessionData.PlayOnce;

            return false;
        }

        public string GetPromptText() => promptText;
    }
}

using TwoWorlds.Core;
using TwoWorlds.Inventory;
using UnityEngine;

namespace TwoWorlds.Dialogue
{
    [RequireComponent(typeof(Collider2D))]
    public class DialogueTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] DialogueData dialogue;
        [SerializeField] DialogueData fallbackDialogue;
        [SerializeField] ItemData requiredItem;
        [SerializeField] int requiredAmount = 1;
        [SerializeField] bool consumeRequiredItem;
        [SerializeField] string promptText = "对话";

        bool hasPlayedOnce;

        public bool CanInteract(GameObject interactor)
        {
            if (dialogue == null)
                return false;

            if (dialogue.PlayOnce && hasPlayedOnce)
                return false;

            return true;
        }

        public void Interact(GameObject interactor)
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

        DialogueData ResolveDialogue(PlayerInventory inventory)
        {
            if (requiredItem != null)
            {
                var hasItem = inventory != null && inventory.HasItem(requiredItem, requiredAmount);
                return hasItem ? dialogue : fallbackDialogue;
            }

            return dialogue;
        }

        public string GetPromptText() => promptText;
    }
}

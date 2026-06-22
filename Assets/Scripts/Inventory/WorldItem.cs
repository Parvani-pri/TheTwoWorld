using TwoWorlds.Core;
using TwoWorlds.Player;
using UnityEngine;

namespace TwoWorlds.Inventory
{
    [RequireComponent(typeof(Collider2D))]
    public class WorldItem : MonoBehaviour, IInteractable
    {
        [SerializeField] ItemData itemData;
        [SerializeField] int amount = 1;
        [SerializeField] bool destroyOnPickup = true;
        [SerializeField] string promptText = "拾取";

        public bool CanInteract(GameObject interactor) =>
            itemData != null && interactor.GetComponent<PlayerInventory>() != null;

        public void Interact(GameObject interactor)
        {
            var inventory = interactor.GetComponent<PlayerInventory>();
            if (inventory == null || itemData == null)
                return;

            if (!inventory.AddItem(itemData, amount))
            {
                Debug.Log("[WorldItem] Inventory is full.");
                return;
            }

            if (destroyOnPickup)
                Destroy(gameObject);
        }

        public string GetPromptText() =>
            itemData != null ? $"{promptText} {itemData.DisplayName}" : promptText;
    }
}

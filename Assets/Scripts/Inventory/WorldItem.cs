using TwoWorlds.Core;
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

        public ItemData ItemData => itemData;
        public int Amount => amount;

        public void Configure(ItemData data, int count)
        {
            itemData = data;
            amount = Mathf.Max(1, count);
        }

        public bool CanInteract(GameObject interactor) =>
            itemData != null && interactor.GetComponent<PlayerInventory>() != null;

        public void Interact(GameObject interactor)
        {
            var inventory = interactor.GetComponent<PlayerInventory>();
            if (inventory == null || itemData == null)
                return;

            var result = inventory.AddItem(itemData, amount);
            if (result.AddedAmount <= 0)
                return;

            if (result.IsFullSuccess)
            {
                if (destroyOnPickup)
                    Destroy(gameObject);
                return;
            }

            amount -= result.AddedAmount;
            Configure(itemData, amount);
        }

        public string GetPromptText() =>
            itemData != null ? $"{promptText} {itemData.DisplayName}" : promptText;
    }
}

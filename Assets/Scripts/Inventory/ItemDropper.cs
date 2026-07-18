using UnityEngine;

namespace TwoWorlds.Inventory
{
    public class ItemDropper : MonoBehaviour
    {
        [SerializeField] float dropOffset = 0.8f;
        [SerializeField] Material spriteMaterial;

        public bool DropFromSlot(PlayerInventory inventory, int slotIndex, int amount = 1)
        {
            if (inventory == null)
                return false;

            var slot = inventory.GetSlot(slotIndex);
            if (slot.IsEmpty)
                return false;

            if (slot.item.Icon == null)
            {
                Debug.LogWarning($"[ItemDropper] '{slot.item.name}' has no icon assigned. Cannot drop.", slot.item);
                return false;
            }

            if (!inventory.TryTakeFromSlot(slotIndex, amount, out var item, out var taken))
                return false;

            var dropPosition = transform.position + GetDropDirection() * dropOffset;
            WorldItemSpawner.Spawn(item, taken, dropPosition, spriteMaterial);
            return true;
        }

        Vector3 GetDropDirection()
        {
            var facing = transform.localScale.x >= 0f ? 1f : -1f;
            return new Vector3(facing, 0f, 0f);
        }
    }
}

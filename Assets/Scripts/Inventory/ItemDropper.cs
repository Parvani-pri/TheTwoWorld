using UnityEngine;

namespace TwoWorlds.Inventory
{
    public class ItemDropper : MonoBehaviour
    {
        [SerializeField] float dropOffset = 0.8f;

        public bool DropFromSlot(PlayerInventory inventory, int slotIndex, int amount = 1)
        {
            if (inventory == null || !inventory.TryTakeFromSlot(slotIndex, amount, out var item, out var taken))
                return false;

            SpawnWorldItem(item, taken);
            return true;
        }

        void SpawnWorldItem(ItemData item, int amount)
        {
            if (item == null)
                return;

            var dropPosition = transform.position + GetDropDirection() * dropOffset;
            var dropObject = new GameObject($"Drop_{item.name}");
            dropObject.transform.position = dropPosition;
            dropObject.layer = LayerMask.NameToLayer("Interactable");

            var spriteRenderer = dropObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = item.Icon;

            var collider = dropObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = GetColliderSize(item);

            var worldItem = dropObject.AddComponent<WorldItem>();
            worldItem.Configure(item, amount);
        }

        Vector3 GetDropDirection()
        {
            var facing = transform.localScale.x >= 0f ? 1f : -1f;
            return new Vector3(facing, 0f, 0f);
        }

        static Vector2 GetColliderSize(ItemData item)
        {
            if (item.Icon != null)
                return item.Icon.bounds.size;

            return Vector2.one;
        }
    }
}

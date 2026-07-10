using UnityEngine;

namespace TwoWorlds.Inventory
{
    public class ItemDropper : MonoBehaviour
    {
        [SerializeField] float dropOffset = 0.8f;
        [SerializeField] float zDepthOffset = 0.15f;
        [SerializeField] int sortingOrder = 5;
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

            SpawnWorldItem(item, taken);
            return true;
        }

        void SpawnWorldItem(ItemData item, int amount)
        {
            if (item == null || item.Icon == null)
                return;

            var dropPosition = transform.position + GetDropDirection() * dropOffset;
            dropPosition.z += zDepthOffset;

            var dropObject = new GameObject($"Drop_{item.name}");
            dropObject.transform.position = dropPosition;
            dropObject.layer = LayerMask.NameToLayer("Interactable");

            var spriteRenderer = dropObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = item.Icon;
            spriteRenderer.sortingOrder = sortingOrder;
            ApplySpriteMaterial(spriteRenderer);

            var collider = dropObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = GetColliderSize(item);

            var worldItem = dropObject.AddComponent<WorldItem>();
            worldItem.Configure(item, amount);
        }

        void ApplySpriteMaterial(SpriteRenderer spriteRenderer)
        {
            if (spriteMaterial != null)
                spriteRenderer.sharedMaterial = spriteMaterial;
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

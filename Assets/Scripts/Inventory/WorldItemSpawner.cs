using UnityEngine;

namespace TwoWorlds.Inventory
{
    public static class WorldItemSpawner
    {
        const int DefaultSortingOrder = 5;
        const float DefaultZDepthOffset = 0.15f;

        public static GameObject Spawn(ItemData item, int amount, Vector3 position, Material spriteMaterial = null)
        {
            if (item == null || amount <= 0 || item.Icon == null)
                return null;

            var spawnPosition = position;
            spawnPosition.z += DefaultZDepthOffset;

            var dropObject = new GameObject($"Drop_{item.name}");
            dropObject.transform.position = spawnPosition;

            var interactableLayer = LayerMask.NameToLayer("Interactable");
            if (interactableLayer >= 0)
                dropObject.layer = interactableLayer;

            var spriteRenderer = dropObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = item.Icon;
            spriteRenderer.sortingOrder = DefaultSortingOrder;

            if (spriteMaterial != null)
                spriteRenderer.sharedMaterial = spriteMaterial;

            var collider = dropObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = item.Icon != null ? (Vector2)item.Icon.bounds.size : Vector2.one;

            var worldItem = dropObject.AddComponent<WorldItem>();
            worldItem.Configure(item, amount);
            return dropObject;
        }
    }
}

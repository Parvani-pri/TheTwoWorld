using TwoWorlds.Inventory;
using UnityEngine;

namespace TwoWorlds.Combat
{
    public class LootFlyPickup : MonoBehaviour
    {
        const int SortingOrder = 6;

        ItemData item;
        int amount;
        PlayerInventory inventory;
        Transform pickupTarget;

        Vector3 startPosition;
        Vector3 popPosition;
        float popDuration;
        float flyDuration;
        float phaseElapsed;
        Phase phase;
        bool collected;

        enum Phase
        {
            Pop,
            Fly
        }

        public void Launch(
            ItemData itemData,
            int itemAmount,
            Vector3 origin,
            Vector3 scatterOffset,
            PlayerInventory targetInventory,
            Transform targetTransform,
            float popSeconds,
            float flySeconds)
        {
            item = itemData;
            amount = itemAmount;
            inventory = targetInventory;
            pickupTarget = targetTransform;
            startPosition = origin;
            popPosition = origin + scatterOffset;
            popDuration = Mathf.Max(0.01f, popSeconds);
            flyDuration = Mathf.Max(0.01f, flySeconds);
            phase = Phase.Pop;
            phaseElapsed = 0f;
            collected = false;

            transform.position = startPosition;

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && item != null)
                spriteRenderer.sprite = item.Icon;
        }

        void Update()
        {
            if (collected || item == null || inventory == null)
                return;

            phaseElapsed += Time.deltaTime;

            switch (phase)
            {
                case Phase.Pop:
                    UpdatePopPhase();
                    break;
                case Phase.Fly:
                    UpdateFlyPhase();
                    break;
            }
        }

        void UpdatePopPhase()
        {
            var t = Mathf.Clamp01(phaseElapsed / popDuration);
            transform.position = Vector3.Lerp(startPosition, popPosition, EaseOutQuad(t));

            if (phaseElapsed < popDuration)
                return;

            phase = Phase.Fly;
            phaseElapsed = 0f;
        }

        void UpdateFlyPhase()
        {
            if (pickupTarget == null)
            {
                Collect();
                return;
            }

            var targetPosition = pickupTarget.position;
            targetPosition.z = transform.position.z;

            var t = Mathf.Clamp01(phaseElapsed / flyDuration);
            transform.position = Vector3.Lerp(popPosition, targetPosition, EaseInQuad(t));

            if (phaseElapsed >= flyDuration || Vector3.Distance(transform.position, targetPosition) <= 0.08f)
                Collect();
        }

        void Collect()
        {
            if (collected)
                return;

            collected = true;
            inventory.AddItem(item, amount);
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (collected || item == null || inventory == null)
                return;

            inventory.AddItem(item, amount);
            collected = true;
        }

        static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

        static float EaseInQuad(float t) => t * t;

        public static LootFlyPickup Spawn(
            ItemData itemData,
            int itemAmount,
            Vector3 origin,
            Vector3 scatterOffset,
            PlayerInventory targetInventory,
            float popSeconds,
            float flySeconds)
        {
            if (itemData == null || itemAmount <= 0 || itemData.Icon == null || targetInventory == null)
                return null;

            var flyObject = new GameObject($"LootFly_{itemData.name}");
            flyObject.transform.position = origin;

            var spriteRenderer = flyObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = itemData.Icon;
            spriteRenderer.sortingOrder = SortingOrder;

            var pickup = flyObject.AddComponent<LootFlyPickup>();
            pickup.Launch(
                itemData,
                itemAmount,
                origin,
                scatterOffset,
                targetInventory,
                targetInventory.transform,
                popSeconds,
                flySeconds);

            return pickup;
        }
    }
}

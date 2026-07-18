using TwoWorlds.Inventory;
using UnityEngine;

namespace TwoWorlds.Combat
{
    [RequireComponent(typeof(CombatHealth))]
    public class EnemyLootDropper : MonoBehaviour
    {
        [SerializeField] EnemyLootTable lootTable;
        [SerializeField] bool dropOnlyOnce = true;
        [SerializeField] Vector3 dropOffset;
        [SerializeField] float popDuration = 0.18f;
        [SerializeField] float flyDuration = 0.42f;

        CombatHealth combatHealth;
        bool hasDropped;

        void Awake()
        {
            combatHealth = GetComponent<CombatHealth>();
        }

        void OnEnable()
        {
            if (combatHealth != null)
                combatHealth.Died += OnDied;
        }

        void OnDisable()
        {
            if (combatHealth != null)
                combatHealth.Died -= OnDied;
        }

        void OnDied(CombatHealth _)
        {
            if (lootTable == null || (dropOnlyOnce && hasDropped))
                return;

            var inventory = FindFirstObjectByType<PlayerInventory>();
            if (inventory == null)
            {
                Debug.LogWarning("[EnemyLootDropper] PlayerInventory not found. Loot was not granted.");
                return;
            }

            hasDropped = true;

            var origin = transform.position + dropOffset;
            origin.z += 0.15f;

            var spawnedCount = 0;
            var dropIndex = 0;

            foreach (var entry in lootTable.Drops)
            {
                if (!entry.IsValid || !entry.RollDrop())
                    continue;

                var amount = entry.RollAmount();
                if (amount <= 0)
                    continue;

                var scatterOffset = GetScatterOffset(dropIndex, lootTable.ScatterRadius);
                if (LootFlyPickup.Spawn(entry.Item, amount, origin, scatterOffset, inventory, popDuration, flyDuration) != null)
                {
                    spawnedCount++;
                    dropIndex++;
                }
            }

            if (spawnedCount <= 0)
                hasDropped = false;
        }

        static Vector3 GetScatterOffset(int index, float radius)
        {
            if (index <= 0 || radius <= 0f)
                return Vector3.zero;

            var angle = index * 137.5f * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
        }
    }
}

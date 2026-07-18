using UnityEngine;

namespace TwoWorlds.Combat
{
    [CreateAssetMenu(fileName = "NewEnemyLootTable", menuName = "Two Worlds/Enemy Loot Table")]
    public class EnemyLootTable : ScriptableObject
    {
        [SerializeField] LootEntry[] drops;
        [SerializeField] float scatterRadius = 0.35f;

        public LootEntry[] Drops => drops ?? System.Array.Empty<LootEntry>();
        public float ScatterRadius => Mathf.Max(0f, scatterRadius);
    }
}

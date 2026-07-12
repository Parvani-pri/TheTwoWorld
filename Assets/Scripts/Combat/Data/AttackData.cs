using UnityEngine;

namespace TwoWorlds.Combat
{
    [CreateAssetMenu(fileName = "AttackData", menuName = "Two Worlds/Combat/Attack Data")]
    public class AttackData : ScriptableObject
    {
        [SerializeField] int damage = 3;
        [SerializeField] float hitboxActiveDuration = 0.15f;
        [SerializeField] float cooldown = 0.5f;
        [SerializeField] float attackRange = 1.2f;
        [SerializeField] float heightTolerance = 0.5f;

        public int Damage => damage;
        public float HitboxActiveDuration => hitboxActiveDuration;
        public float Cooldown => cooldown;
        public float AttackRange => attackRange;
        public float HeightTolerance => heightTolerance;
    }
}

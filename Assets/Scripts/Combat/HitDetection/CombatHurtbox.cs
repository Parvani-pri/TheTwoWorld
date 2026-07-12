using UnityEngine;

namespace TwoWorlds.Combat
{
    /// <summary>
    /// Marks a trigger collider as a damage receiver. Place on a Scene child object.
    /// </summary>
    public class CombatHurtbox : MonoBehaviour
    {
        [SerializeField] CombatActor owner;
        [SerializeField] CombatHealth health;

        public CombatActor Owner => owner;
        public CombatHealth Health => health;
        public CombatFaction Faction => owner != null ? owner.Faction : CombatFaction.Enemy;

        void Awake()
        {
            if (owner == null)
                owner = GetComponentInParent<CombatActor>();

            if (health == null)
                health = GetComponentInParent<CombatHealth>();
        }
    }
}

using System.Collections.Generic;
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
        [SerializeField] List<ParticleSystemCollider> pscList;

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

        private void OnEnable()
        {
            if (owner.Faction == CombatFaction.Enemy)
            {
                if (TryGetComponent(out Collider2D collider2D))
                {
                    foreach (ParticleSystemCollider psc in pscList)
                    {
                        psc.AddCollider(collider2D);
                    }

                }
                else if (TryGetComponent(out Collider collider3D))
                {
                    foreach (ParticleSystemCollider psc in pscList)
                    {
                        psc.AddCollider(collider3D);
                    }

                }
            }
        }

    }
}

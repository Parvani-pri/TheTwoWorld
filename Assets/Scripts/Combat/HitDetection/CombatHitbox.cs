using System.Collections.Generic;
using UnityEngine;

namespace TwoWorlds.Combat
{
    /// <summary>
    /// Attack hit logic on a Scene child object with a trigger Collider2D.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CombatHitbox : MonoBehaviour
    {
        [SerializeField] CombatActor owner;
        [SerializeField] Collider2D hitCollider;

        readonly HashSet<CombatHurtbox> hitThisActivation = new();
        AttackData activeAttack;
        float activeTimer;
        bool isActive;

        public bool IsActive => isActive;

        void Awake()
        {
            if (owner == null)
                owner = GetComponentInParent<CombatActor>();

            if (hitCollider == null)
                hitCollider = GetComponent<Collider2D>();

            if (hitCollider != null)
                hitCollider.enabled = false;
        }

        void Update()
        {
            if (!isActive)
                return;

            activeTimer -= Time.deltaTime;
            if (activeTimer <= 0f)
                Deactivate();
        }

        public void Activate(AttackData attackData)
        {
            if (attackData == null || hitCollider == null)
                return;

            activeAttack = attackData;
            activeTimer = attackData.HitboxActiveDuration;
            isActive = true;
            hitThisActivation.Clear();
            hitCollider.enabled = true;
        }

        public void Deactivate()
        {
            isActive = false;
            activeAttack = null;
            activeTimer = 0f;
            hitThisActivation.Clear();

            if (hitCollider != null)
                hitCollider.enabled = false;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActive || activeAttack == null)
                return;

            var hurtbox = other.GetComponent<CombatHurtbox>()
                ?? other.GetComponentInParent<CombatHurtbox>();

            if (hurtbox == null || hurtbox.Health == null || !hurtbox.Health.IsAlive)
                return;

            if (owner != null && hurtbox.Owner == owner)
                return;

            if (owner != null && hurtbox.Faction == owner.Faction)
                return;

            if (hitThisActivation.Contains(hurtbox))
                return;

            if (owner != null && hurtbox.Owner != null)
            {
                var heightDiff = Mathf.Abs(owner.Height - hurtbox.Owner.Height);
                if (heightDiff > activeAttack.HeightTolerance)
                    return;
            }

            hitThisActivation.Add(hurtbox);
            hurtbox.Health.TakeDamage(activeAttack.Damage, owner);
        }
    }
}

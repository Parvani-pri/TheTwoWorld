using System.Collections.Generic;
using UnityEngine;
using XuFu.MaskSystem;

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
        readonly List<Collider2D> overlapResults = new();
        static readonly ContactFilter2D TriggerFilter = ContactFilter2D.noFilter;

        AttackData activeAttack;
        float activeTimer;
        bool isActive;
        bool canAttack = true;

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

        public void Activate(AttackData attackData, float accumulatedMultiplier)
        {
            if (attackData == null || hitCollider == null)
                return;



            activeAttack = attackData;
            activeTimer = attackData.HitboxActiveDuration;
            isActive = true;
            hitThisActivation.Clear();
            hitCollider.enabled = true;
            Physics2D.SyncTransforms();
            ScanCurrentOverlaps(accumulatedMultiplier);
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

        void OnTriggerEnter2D(Collider2D other) => TryHit(other, 1);

        void OnTriggerStay2D(Collider2D other) => TryHit(other, 1);

        void ScanCurrentOverlaps(float accumulatedMultiplier)
        {
            if (!isActive || hitCollider == null)
                return;

            overlapResults.Clear();
            var filter = TriggerFilter;
            filter.useTriggers = true;
            hitCollider.Overlap(filter, overlapResults);
            print(overlapResults.Count);
            foreach (var other in overlapResults)
            {
                if (other.GetComponent<CombatHurtbox>() != null && other.gameObject != this)
                {
                    TryHit(other, accumulatedMultiplier);
                }
            }


        }

        void TryHit(Collider2D other, float accumulatedMultiplier)
        {
            if (!canAttack) return;
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
            if (hurtbox.transform.parent.GetComponentInChildren<Shield>(true) != null)
            {
                if (hurtbox.transform.parent.GetComponentInChildren<Shield>(true).gameObject.activeSelf == true)
                {
                    PlayerAttackUI.OnMaskAbilityCast(1, 10f);
                    MaskController.maskAbilityTimer = 10f;
                    hurtbox.transform.parent.GetComponentInChildren<Shield>(true).gameObject.SetActive(false);
                    print("block successful");
                    return;
                }
            }

            print("will take damage");
            hurtbox.Health.TakeDamage((int)(activeAttack.Damage * accumulatedMultiplier), owner);
            if (hurtbox.Health.CurrentHealth <= 0 && owner.Faction == CombatFaction.Enemy)
            {
                if (GetComponentInParent<EnemyAttackAI>() != null)
                {
                    GetComponentInParent<EnemyAttackAI>().IsPlayerDead(true);
                }
            }
        }
    }
}

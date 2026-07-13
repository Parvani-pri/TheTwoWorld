using System;
using TwoWorlds.Core;
using UnityEngine;

namespace TwoWorlds.Combat
{
    public class CombatHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] int maxHealth = 10;
        [SerializeField] bool destroyOnDeath = true;
        [SerializeField] float invincibilityDuration = 0.3f;
        [SerializeField] GameObject damagePopupPrefab;
        [SerializeField] Transform healthBarTransform;

        CombatActor actor;
        float invincibleUntil;

        public int MaxHealth => maxHealth;
        public int CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;
        public bool IsAlive => !IsDead;
        public CombatFaction Faction => actor != null ? actor.Faction : CombatFaction.Enemy;

        public event Action<int, int> HealthChanged;
        public event Action<CombatHealth> Died;

        void Awake()
        {
            actor = GetComponent<CombatActor>();
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            TakeDamage(amount, null);
        }

        public void TakeDamage(int amount, CombatActor source)
        {
            if (IsDead || amount <= 0 || Time.time < invincibleUntil)
                return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            invincibleUntil = Time.time + invincibilityDuration;

            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            DamagePopup.Create(healthBarTransform.position, amount, damagePopupPrefab);
            GameEvents.RaiseActorHealthChanged(this, CurrentHealth, maxHealth);
            GameEvents.RaiseActorDamaged(this, amount, source);

            if (IsDead)
            {
                Died?.Invoke(this);
                GameEvents.RaiseActorDied(this);

                if (destroyOnDeath)
                    Destroy(gameObject);
            }
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0)
                return;

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            GameEvents.RaiseActorHealthChanged(this, CurrentHealth, maxHealth);
        }
    }
}

using System;
using UnityEngine;

namespace TwoWorlds.Combat
{
    /// <summary>
    /// Minimal health for stage-one prototype. Damage sources and death
    /// handling (loot, VFX) hook into the events in later stages.
    /// </summary>
    public class CombatHealth : MonoBehaviour
    {
        [SerializeField] int maxHealth = 10;
        [SerializeField] bool destroyOnDeath = true;

        public int MaxHealth => maxHealth;
        public int CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        public event Action<int, int> HealthChanged; // (current, max)
        public event Action<CombatHealth> Died;

        void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0)
                return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (IsDead)
            {
                Died?.Invoke(this);
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
        }
    }
}

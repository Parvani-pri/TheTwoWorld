using TwoWorlds.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TwoWorlds.Combat
{
    public class CombatHealthBarUI : MonoBehaviour
    {
        [SerializeField] CombatHealth targetHealth;
        [SerializeField] Image fillImage;
        [SerializeField] bool hideWhenFull;

        void OnEnable()
        {
            GameEvents.ActorHealthChanged += OnActorHealthChanged;

            if (targetHealth != null)
                Refresh(targetHealth.CurrentHealth, targetHealth.MaxHealth);
        }

        void OnDisable()
        {
            GameEvents.ActorHealthChanged -= OnActorHealthChanged;
        }

        void Start()
        {
            if (targetHealth != null)
                Refresh(targetHealth.CurrentHealth, targetHealth.MaxHealth);
        }

        void OnActorHealthChanged(CombatHealth health, int current, int max)
        {
            if (health != targetHealth)
                return;

            Refresh(current, max);
        }

        void Refresh(int current, int max)
        {
            if (fillImage == null)
                return;

            var normalized = max > 0 ? (float)current / max : 0f;
            fillImage.fillAmount = normalized;

            if (hideWhenFull)
                fillImage.transform.parent.gameObject.SetActive(current < max);
        }
    }
}

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

        RectTransform fillRect;

        void Awake()
        {
            if (fillImage != null)
                fillRect = fillImage.rectTransform;
        }

        void OnEnable()
        {
            GameEvents.ActorHealthChanged += OnActorHealthChanged;
            GameEvents.CombatStarted += OnCombatStarted;

            RefreshFromTarget();
        }

        void OnDisable()
        {
            GameEvents.ActorHealthChanged -= OnActorHealthChanged;
            GameEvents.CombatStarted -= OnCombatStarted;
        }

        void Start() => RefreshFromTarget();

        void OnCombatStarted() => RefreshFromTarget();

        void OnActorHealthChanged(CombatHealth health, int current, int max)
        {
            if (health != targetHealth)
                return;

            Refresh(current, max);
        }

        void RefreshFromTarget()
        {
            if (targetHealth == null)
                return;

            Refresh(targetHealth.CurrentHealth, targetHealth.MaxHealth);
        }

        void Refresh(int current, int max)
        {
            var normalized = max > 0 ? (float)current / max : 0f;

            if (fillRect != null)
            {
                var anchorMax = fillRect.anchorMax;
                anchorMax.x = normalized;
                fillRect.anchorMax = anchorMax;
            }
            else if (fillImage != null)
            {
                fillImage.fillAmount = normalized;
            }

            if (hideWhenFull && fillImage != null && fillImage.transform.parent != null)
                fillImage.transform.parent.gameObject.SetActive(current < max);
        }
    }
}

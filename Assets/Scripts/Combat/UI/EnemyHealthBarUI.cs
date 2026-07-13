using System.Collections;
using TwoWorlds.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TwoWorlds.Combat
{
    public class EnemyHealthBarUI : MonoBehaviour
    {
        [SerializeField] CombatHealth targetHealth;
        [SerializeField] Image enemyHealthFill;
        [SerializeField] bool hideWhenFull;


        void Awake()
        {
        }

        void OnEnable()
        {
            GameEvents.ActorHealthChanged += OnActorHealthChanged;
            GameEvents.CombatStarted += OnCombatStarted;



            enemyHealthFill.fillAmount = 1;
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

            if (enemyHealthFill != null)
            {
                StartCoroutine(HealthBarLerp(0.5f, normalized));
            }

            if (hideWhenFull && enemyHealthFill != null && enemyHealthFill.transform.parent != null)
                enemyHealthFill.transform.parent.gameObject.SetActive(current < max);
        }

        IEnumerator HealthBarLerp(float duration, float target)
        {
            float timeLapsed = 0;
            float initialValue = enemyHealthFill.fillAmount;
            while (timeLapsed < duration)
            {
                enemyHealthFill.fillAmount = Mathf.Lerp(initialValue, target, timeLapsed / duration);
                timeLapsed += Time.deltaTime;
                yield return null;
            }
            enemyHealthFill.fillAmount = target;
        }
    }
}

